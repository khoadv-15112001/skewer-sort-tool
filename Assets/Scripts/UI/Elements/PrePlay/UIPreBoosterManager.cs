using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sonat.Enums;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.SceneManagement;
using UnityEngine;

namespace GrillSort.PreBooster
{
    public class UIPreBoosterManager : MonoBehaviour
    {
        [SerializeField] private UIButtonPreBooster[] preBoosterButtons;

        private readonly Service<PreBoosterService> preBoosterService = new();

        public static List<GameResource> usedBoosters = new();

        private void Start()
        {
            if (ShowTut())
            {
                PlayerPrefs.SetInt("Tut_PreBooster_Showed", 1);
                SonatUtils.DelayCall(0.25f,
                    () => { PanelManager.Instance.OpenPanel<PopupTutPrebooster>(new PopupTutPrebooster.Data() { preboosterTransform = this.transform }); },
                    this);
            }
        }

        public void OnClickPlay()
        {
            foreach (var booster in usedBoosters)
            {
                preBoosterService.Instance.PrepareBooster(booster);
            }

            //Play();
        }

        public void OnClickAds()
        {
            if (preBoosterService.Instance.unlockAllBooster)
            {
                MySonatFramework.ShowRewardAds(OnWatchedVideo, "x2_coin_win", "x2_coin_win");
            }
            else
            {
                PopupToast.Cretate("Unlock the Preboosters first to use them!");
            }
        }

        private void OnWatchedVideo()
        {
            foreach (var booster in preBoosterButtons)
            {
                preBoosterService.Instance.AddBooster(booster.boosterType, 1, new EarnResourceLogData()
                {
                    spendType = booster.boosterType.ResourceType().ToString(),
                    spendId = booster.boosterType.ToString(),
                    source = "non_iap"
                });
                preBoosterService.Instance.PrepareBooster(booster.boosterType);
            }

            //Play();
        }

        // private void Play()
        // {
        //     if (MySonatFramework.GetService<SceneService>().GetCurrentGamePlacement() == GamePlacement.Gameplay)
        //     {
        //         PanelManager.Instance.ClosePanel<PopupPrePlay>();
        //
        //         var panel = PanelManager.Instance.GetPanel<BasePanel>();
        //         if (panel != null)
        //         {
        //             panel.Close();
        //         }
        //
        //         GameplayController.instance.StartPlay().Forget();
        //     }
        //     else
        //     {
        //         LoadingScreenInstance.Instance.Show(5f);
        //         SonatUtils.DelayCall(0.25f, () => { MySonatFramework.GetService<SceneService>().SwitchScene(GamePlacement.Gameplay); });
        //     }
        // }

        private bool ShowTut()
        {
            return !PlayerPrefs.HasKey("Tut_PreBooster_Showed");
        }
    }
}