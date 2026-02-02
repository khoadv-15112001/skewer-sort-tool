using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using UnityEngine;
using GrillSort.PreBooster;
using System.Collections.Generic;
using System;
using Cysharp.Threading.Tasks;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.EventBus;

public class PopupUsePreBooster : Panel
{
    [SerializeField] private float delayClose = 5f;
    [SerializeField] private UIPreBooster[] uiPreBoosters;

    public override void Open(UIData uiData)
    {
        base.Open(uiData);
        SonatUtils.DelayCall(delayClose, Close, this);

        var list = uiData.Get<List<GameResource>>("listPreBooster");
        foreach (var booster in uiPreBoosters)
        {
            if (list.Contains(booster.boosterType))
            {
                booster.gameObject.SetActive(true);
            }
            else
            {
                booster.gameObject.SetActive(false);
            }
        }
        // EventBus<GameStateChangeEvent>.Raise(new GameStateChangeEvent(){gameState = GameState.Paused});
    }

    public override void Close()
    {
        int delay = 0;
        foreach (var booster in uiPreBoosters)
        {
            if (booster.gameObject.activeSelf)
            {
                booster.UseBooster(delay).Forget();
                delay += 500;
            }
        }

        gameObject.SetActive(false);
        SonatUtils.DelayCall(delay * 1.0f / 1000 + 2f, () =>
        {
            base.Close();
            // sau khi dùng hết prebooster thì mới có thể play
            UIFlowController.isShowedEffectPreBooster = false;
        });
    }
}