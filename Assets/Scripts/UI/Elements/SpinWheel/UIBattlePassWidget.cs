using Cysharp.Threading.Tasks;
using GrillSort.BattlePass;
using GrillSort.Winstreak;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.ObjectPooling;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.EndlessTreasure
{
    public class UIBattlePassWidget : UIHomeWidget
    {
        [SerializeField] private Image icon;
        [SerializeField] private SonatFramework.Scripts.UIModule.UIElements.UITimeCounter timeCounter;
        [SerializeField] private GameObject noti;
        private readonly Service<BattlePassService> battlePassService = new();
        public override void Setup()
        {
            if (battlePassService.Instance.IsUnlocked() == false)
            {
                if (battlePassService.Instance.CanUnlock())
                {
                    battlePassService.Instance.Unlock();
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }

            // battlePassService.Instance.OnUpdateUI += UpdateUI;
            //UpdateUI();

            noti.SetActive(battlePassService.Instance.CanClaim());

        }

        private void UpdateUI(int idx = -1)
        {
            var themeData = battlePassService.Instance.GetCurrentThemeData();

            timeCounter.SetData(battlePassService.Instance.GetTimeToReset(), null);

            if (battlePassService.Instance.battlePassKey > 0)
            {
                Collect(battlePassService.Instance.battlePassKey, themeData.icon, () =>
                {
                    battlePassService.Instance.battlePassKey = 0;
                    noti.SetActive(battlePassService.Instance.CanClaim());
                }, 2);
            }
            else
            {
                noti.SetActive(battlePassService.Instance.CanClaim());
            }

            icon.sprite = themeData.icon;
        }

        private void OnDestroy()
        {
            // battlePassService.Instance.OnUpdateUI -= UpdateUI;
        }

        public override void OnFocus()
        {
            UpdateUI();
        }


        public override void OnLoseFocus()
        {

        }


        // Chỉ hiện ở phiên đầu tiên trong ngày
        public override async UniTask<bool> ProcessTask()
        {
            if (battlePassService.Instance.IsUnlocked() == false) return false;
            if (PlayerPrefs.HasKey($"{BattlePassService.DATA_KEY}_AppearBattlePass")) return false;
            PlayerPrefs.SetInt($"{BattlePassService.DATA_KEY}_AppearBattlePass", 1);

            var popup = battlePassService.Instance.OpenBattlePassTut(new UIData().Add("ShowButtonPlay", false));
            MySonatFramework.customTrackingService.OnShowPopup(gameObject.name.ToLogString(), "pop_up", "non_iap", "auto");
            await UniTask.WaitUntil(() => popup == null || !popup.gameObject.activeInHierarchy);
            await UniTask.Delay(100);
            //var popup2 = PanelManager.Instance.GetPanel<PopupBattlePass>();
            var popup2 = battlePassService.Instance.GetBattlePassPanel();
            await UniTask.WaitUntil(() => popup2 == null || !popup2.gameObject.activeInHierarchy);
            await UniTask.Delay(750);
            return true;
        }

        public void OnClick()
        {
            battlePassService.Instance.OpenBattlePass();
            //PanelManager.Instance.OpenForget<PopupBattlePass>();
            MySonatFramework.customTrackingService.OnShowPopup(gameObject.name.ToLogString(), "widget", "non_iap", "user");
        }

    }
}