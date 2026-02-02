using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.BoosteeManagement;
using Gameplay.LevelData;
using Manager;
using Sonat.Enums;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using UnityEngine;

namespace Gameplay.Entities.Grills
{
    public class PrimaryGrillLockAds : PrimaryGrill
    {
        public override async UniTask SetData(GrillData grillData)
        {
            lockState = 1;
            base.SetData(grillData);
            grillVisual.CloseGrill(false);
            SetLockItems(true);
            SetSlotCollider(false);
        }

        public override void OpenGrill()
        {
        }

        public override ShuffleLayerData GetShuffleLayerData()
        {
            if (IsLock)
                return null;
            else
                return base.GetShuffleLayerData();
        }

        public override List<ShuffleLayerData> GetSubsShuffleLayerData()
        {
            if (IsLock)
                return null;
            return base.GetSubsShuffleLayerData();
        }

        private void OnMouseUpAsButton()
        {
            if (!IsLock || GameplayController.instance.gameState != GameState.Playing) return;
            PanelManager.Instance.ClosePanel<PopupSuggest>();

            if (GameplayController.level <= 6)
            {
                PanelManager.Instance.OpenPanel<PopupUnlockGrillFree>(
                    new UIData().Add("Callback", (Action)Unlock).Add("PrimaryGrill", this)
                );
            }
            else
            {
                if (GameRemoteConfigValue.popupUnlockTray)
                {
                    PanelManager.Instance.OpenPanel<PopupUnlockGrill>(new UIData().Add("Callback", (Action)Unlock).Add("PrimaryGrill", this)
                    );
                }
                else
                {
                    MySonatFramework.ShowRewardAds(Unlock, "booster", "unlock_lock_grill");
                }
            }
        }

        public override void Unlock()
        {
            base.Unlock();
            grillVisual.OpenGrill();
            SetSlotCollider(true);
        }
    }
}