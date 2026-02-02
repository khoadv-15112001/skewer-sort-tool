using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.Entities;
using Gameplay.LevelData;
using SonatFramework.Scripts.UIModule;
using UnityEngine;

public class PrimaryGrillBomb : PrimaryGrill
{
    public static List<PrimaryGrillBomb> primaryGrillBombs;
    [SerializeField] private GrillVisualBomb visualBomb;


    public ParticleSystem bombExplosionParticle;
    public ParticleSystem bombDetonatorParticle;

    private int maxMove = 0;
    private int moveCount = 0;
    private bool isCloseLid = false;
    private bool bombExploded = false;

    public override async UniTask SetData(GrillData grillData)
    {
        isCloseLid = false;
        bombExploded = false;
        maxMove = grillData.move;
        moveCount = maxMove;
        _ = base.SetData(grillData);
        visualBomb.SetUpBomb(maxMove);
        bombExplosionParticle.gameObject.SetActive(false);

        primaryGrillBombs ??= new List<PrimaryGrillBomb>();
        primaryGrillBombs.Add(this);
    }

    public void OnEnable()
    {
        grillBaseBehaviorSO.eventSystemSO.RegisterEvents_OnCollectItem(OnCollectItem);
        grillBaseBehaviorSO.eventSystemSO.RegisterEvents_OnDropItem(OnItemDropped);
    }

    public void OnDisable()
    {
        grillBaseBehaviorSO.eventSystemSO.UnregisterEvents_OnCollectItem(OnCollectItem);
        grillBaseBehaviorSO.eventSystemSO.UnregisterEvents_OnDropItem(OnItemDropped);
    }

    private void OnItemDropped(Item item, bool changed)
    {
        CheckEmptyGrill();

        if (CheckComplete())
        {
            return;
        }
        if (bombExploded) return;

        moveCount--;
        visualBomb.SetBombCount(moveCount).Forget();

        if (moveCount <= 0)
        {
            bombExploded = true;
            PreBombExploded();
            Debug.LogError("Gameover: bomb exploded!");
        }
    }

    private void PreBombExploded()
    {
        PanelManager.Instance.OpenPanelByName<PopupSkipDynamite>("PopupSkipDynamite",
            new UIData().Add("Dynamite", this).Add("OnSkipDynamite", (Action)SkipDynamite).Add("OnGiveUp", (Action)DynamiteExploded));
    }

    public void SkipDynamite()
    {
        bombExploded = false;
        moveCount = maxMove;
        visualBomb.SetBombCount(moveCount, false).Forget();
    }

    private void DynamiteExploded()
    {
        bombExplosionParticle.Play();
        _ = GameplayController.instance.OnDynamiteExploded();
    }

    private void OnCollectItem(int itemId)
    {

    }

    public override void CheckSubGrills(bool forceShowFirst = true)
    {
        base.CheckSubGrills();
        CheckEmptyGrill();
    }

    protected override void OnComplete()
    {
        base.OnComplete();
        CheckEmptyGrill();
    }

    private void CheckEmptyGrill()
    {
        if (isCloseLid) return;

        foreach (var slot in slots)
        {
            if (!slot.isEmpty())
            {
                return;
            }
        }

        if (subGrills != null && subGrills.Count > 0)
        {
            return;
        }

        isCloseLid = true;
        visualBomb.OnComplete();
        SetLockItems(true);
        bombDetonatorParticle.gameObject.SetActive(false);
        Debug.LogError("Close grill bomb!!");
        grillBaseBehaviorSO.eventSystemSO.UnregisterEvents_OnDropItem(OnItemDropped);
    }
}
