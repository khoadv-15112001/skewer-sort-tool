using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.BoosteeManagement;
using Gameplay.Entities;
using Gameplay.Entities.Grills;
using Gameplay.LevelData;
using SonatFramework.Scripts.Utils;
using UnityEngine;

public class PrimaryGrillIceMain : PrimaryGrill
{
    [SerializeField] private int iceState = 3;
    [SerializeField] private GrillVisualIce visualIce;
    private int currentState = 0;
    public static List<PrimaryGrillIceMain> primaryGrillIces;
    private bool started;
    [HideInInspector] public int priority;

    public byte mainGrillId;

    public int CurrentState => currentState;

    public static Action<byte> OnMainGrillIceComplete;

    public override async UniTask SetData(GrillData grillData)
    {
        base.SetData(grillData);
        mainGrillId = grillData.mainGrillId;

        primaryGrillIces ??= new List<PrimaryGrillIceMain>();
        primaryGrillIces.Add(this);
    }
    public static void CheckAndStartProgress()
    {
        if (primaryGrillIces is not { Count: > 0 }) return;
        primaryGrillIces.Sort((a, b) => a.priority - b.priority);
        primaryGrillIces[0].StartProgress();
    }

    public void StartProgress()
    {
        if (started) return;
        started = true;
    }

    public void NextProgress()
    {
        primaryGrillIces.Remove(this);
        if (primaryGrillIces.Count > 0)
        {
            primaryGrillIces[0].StartProgress();
        }
    }

    private void OnDisable()
    {

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

    public void HideIceMain()
    {
        visualIce.SetIceState(0);
    }

    private void UnlockIce()
    {
        NextProgress();
        SetLockItems(false);
    }

    protected override void OnComplete()
    {
        base.OnComplete();
        OnMainGrillIceComplete?.Invoke(mainGrillId);
    }

    public override void EnableClick()
    {
        base.EnableClick();
        SetSlotCollider(false);
    }
}
