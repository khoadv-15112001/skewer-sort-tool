using SonatFramework.Scripts.Feature.Shop.UI;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.InventoryManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.DailyMission
{
    public class PopupDailyMissionX2 : Panel
    {
        [SerializeField] private TMP_Text timeTxt;
        [SerializeField] private Button btn;
        [SerializeField] private TMP_Text priceTxt;

        private DailyMissionConfig.X2Data _data;

        public override void OnSetup()
        {
            base.OnSetup();

            _data = DailyMissionService.Instance.GetCurrentX2Data();

            priceTxt.text = _data.CoinPrice.ToString();
            timeTxt.text = SonatUtils.GetTimeByFormat(_data.TimeDuration_Second);
            btn.onClick.AddListener(OnClickBuy);

            DailyMissionService.OnChangeMission += Close;
        }

        private void OnDestroy()
        {
            DailyMissionService.OnChangeMission -= Close;
        }

        private void OnClickBuy()
        {
            if (MySonatFramework.inventoryService.CanReduce(Sonat.Enums.GameResource.Coin, _data.CoinPrice))
            {
                var log = new SpendResourceLogData()
                {
                    earnType = "feature",
                    earnId = "dm",
                };

                MySonatFramework.inventoryService.ReduceResource(Sonat.Enums.GameResource.Coin, _data.CoinPrice,
                    log);

                DailyMissionService.Instance.BuyX2();
                Close();
            }
            else
            {
                PopupToast.Cretate("Not enough coin!");
                PanelManager.Instance.OpenPanelByName<ShopPanelBase>("ShopPanel");
            }
        }
    }
}