using Cysharp.Threading.Tasks;
using Gameplay.BoosteeManagement;
using Gameplay.Entities;
using Gameplay.Entities.Grills;
using Gameplay.LevelData;
using SonatFramework.Scripts.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimaryGrillIceNeighbor : PrimaryGrill
{
    [SerializeField] private int iceState = 3;
    [SerializeField] private GrillVisualIce visualIce;
    private int currentState = 0;
    private int numStep = 0;
    private bool success = false;
    public static List<PrimaryGrillIceNeighbor> primaryGrillIces;
    private bool started;
    [HideInInspector] public int priority;

    public byte mainGrillId;

    public int CurrentState => currentState;
    public int NumStep => numStep;

    public override async UniTask SetData(GrillData grillData)
    {
        currentState = iceState;
        lockState = 1;
        numStep = 0;
        started = false;
        success = false;
        base.SetData(grillData);
        mainGrillId = grillData.mainGrillId;
        visualIce.SetIceState(currentState);
        SetLockItems(true);

        primaryGrillIces ??= new List<PrimaryGrillIceNeighbor>();
        primaryGrillIces.Add(this);
    }
    public static void CheckAndStartProgress()
    {
        // if (primaryGrillIces is not { Count: > 0 }) return;
        // primaryGrillIces.Sort((a, b) => a.priority - b.priority);
        // primaryGrillIces[0].StartProgress();

        if (primaryGrillIces is not { Count: > 0 }) return;

        foreach (var primaryGrillIce in primaryGrillIces)
        {
            primaryGrillIce.StartProgress();
        }
    }

    public void StartProgress()
    {
        if (started) return;
        started = true;

        grillBaseBehaviorSO.eventSystemSO.RegisterEvents_OnDropItem(OnItemDropped);
        PrimaryGrillIceMain.OnMainGrillIceComplete += OnMainGrillIceComplete;
    }

    public void NextProgress()
    {
        grillBaseBehaviorSO.eventSystemSO.UnregisterEvents_OnDropItem(OnItemDropped);
        PrimaryGrillIceMain.OnMainGrillIceComplete -= OnMainGrillIceComplete;
        primaryGrillIces.Remove(this);
        if (primaryGrillIces.Count > 0)
        {
            primaryGrillIces[0].StartProgress();
        }
    }

    private void OnDisable()
    {
        grillBaseBehaviorSO.eventSystemSO.UnregisterEvents_OnDropItem(OnItemDropped);
        PrimaryGrillIceMain.OnMainGrillIceComplete -= OnMainGrillIceComplete;
    }

    private void OnMainGrillIceComplete(byte mainGrillId)
    {
        if (this.mainGrillId == mainGrillId)
        {
            success = true;
            DownState();
            numStep = 0;
        }
    }

    private void OnItemDropped(Item item, bool changed)
    {
        // if (!changed || !isLock) return;
        if (!changed || lockState != 1) return;
        numStep++;
        success = false;
        if (numStep >= GetIceGrillStep() && currentState < iceState)
        {
            SonatUtils.DelayCall(0.35f, () =>
            {
                if (!success) UpState();
            }, this);
            numStep = 0;
        }
    }

    private int GetIceGrillStep()
    {
        return 7;
    }

    public override ShuffleLayerData GetShuffleLayerData()
    {
        if (IsLock)
            return null;
        else
        {
            return base.GetShuffleLayerData();
        }
    }

    public override List<ShuffleLayerData> GetSubsShuffleLayerData()
    {
        if (IsLock)
            return null;
        return base.GetSubsShuffleLayerData();
    }

    public void UpState()
    {
        if (currentState >= iceState) return;
        this.currentState++;
        visualIce.SetIceState(currentState);
    }

    public void DownState()
    {
        if (currentState <= 0) return;
        this.currentState--;
        visualIce.SetIceState(currentState);

        if (currentState == 0)
        {
            UnlockIce();
        }
    }

    private void UnlockIce()
    {
        NextProgress();
        SetLockItems(false);
    }


    public override void EnableClick()
    {
        base.EnableClick();
        SetSlotCollider(false);
    }

    public override void Unlock()
    {
        foreach (var primaryGrillIce in PrimaryGrillIceMain.primaryGrillIces)
        {
            if (primaryGrillIce.mainGrillId == mainGrillId)
            {
                primaryGrillIce.Unlock();
            }
        }
        StartCoroutine(PlayUnlockIce());
    }

    private IEnumerator PlayUnlockIce()
    {
        for (int i = currentState; i > 0; i--)
        {
            DownState();
            yield return new WaitForSeconds(0.3f);
        }
        base.Unlock();
        SetSlotCollider(true);
    }

    public void SetIceState(int currentState, int numStep)
    {
        this.currentState = currentState;
        this.numStep = numStep;
        visualIce.ForceSetIceState(currentState);

        if (currentState == 0)
        {
            UnlockIce();
        }
    }
}
