using System.Collections;
using UnityEngine;
using GrillSort.RealTime;
using SonatFramework.Scripts.Helper;
using SonatFramework.Systems;
using SonatFramework.Scripts.UIModule.UIElements;
using UnityEngine.UI;
using Manager;
using Cysharp.Threading.Tasks;
using TMPro;
using System.Threading.Tasks;  // hoặc namespace thực RealTimeService

namespace GrillSort.PackServiceRegistry
{
    [RequireComponent(typeof(UIShopPack))]
    public class UIPackLevelScheduler : MonoBehaviour
    {
        [SerializeField] private Image bannerFrame;
        [SerializeField] private TMP_Text txtTimeBonus, txtSale;
        [SerializeField] private bool isShowTimeCounter = true;
        [SerializeField] private UITimeCounter timeCounter;

        private int maxPurchaseCount;
        private int appearanceDuration;

        private PackLevelManager levelManager;
        private PackSwitcherNew packSwitcher;

        private IntDataPref purchasedInTodayPref;
        private LongDataPref expiredTimePref;
        private readonly Service<RealTimeService> _realTimeService = new();
        private readonly Service<PackServiceRegistry> _packServiceRegistry = new();
        private BannerName bannerName;
        public void Initialize(PackLevelConfig cfg, PackLevelManager manager, PackSwitcherNew switcher)
        {
            bannerName = switcher.BannerName;
            maxPurchaseCount = cfg.purchaseLimit;
            // Bạn có thể lấy duration từ cfg nếu muốn, hoặc dùng quy tắc chung
            appearanceDuration = Mathf.Max(1, cfg.membershipDays) * 24 * 3600;
            levelManager = manager;
            packSwitcher = switcher;

            if (bannerName != BannerName.Revive)
                _ = bannerFrame.SetSpriteAsync(PathManager.BannerPackFrame(bannerName, levelManager.CurrentLevel));

            txtTimeBonus.text = $"+{cfg.timeBonus}s";
            txtSale.text = $"{cfg.saleOff}%";

            Debug.Log($"anhnt: Initialize {bannerName}");
        }

        private void OnEnable()
        {
            LoadPrefs();

            // đánh dấu rằng level này được hiển thị hôm nay
            levelManager?.MarkShownToday();

            if (maxPurchaseCount >= 0 && purchasedInTodayPref.Value >= maxPurchaseCount)
            {
                HideSelf();
                return;
            }

            long now = _realTimeService.Instance.GetCurrentTimeUnix();

            Debug.Log($"OnEnable {gameObject.name}: purchasedToday={purchasedInTodayPref.Value}, max={maxPurchaseCount}, expired={expiredTimePref.Value}, now={now}");

            if (expiredTimePref.Value < now)
            {
                expiredTimePref.Value = now + appearanceDuration;
            }

            long remaining = expiredTimePref.Value - now;
            if (remaining < 0) remaining = 0;

            if (isShowTimeCounter && timeCounter != null)
            {
                timeCounter.SetData(remaining, null);
            }

            //StartCoroutine(DisplayThenHide((int)remaining));
        }

        private void LoadPrefs()
        {
            string key = GetKeySuffix();
            purchasedInTodayPref = new IntDataPref($"PackLevel_{key}_PurchToday");
            expiredTimePref = new LongDataPref($"PackLevel_{key}_ExpiredTime");
        }

        private string GetKeySuffix()
        {
            return bannerName + "";
        }

        private IEnumerator DisplayThenHide(int seconds)
        {
            yield return new WaitForSeconds(seconds);
            expiredTimePref.Value = 0;
            HideSelf();
        }

        private void HideSelf()
        {
            //Debug.Log($"HideSelf called on {gameObject.name}");
            //gameObject.SetActive(false);
        }


        public void HandleBuy()
        {

            LoadPrefs();
            purchasedInTodayPref.Value += 1;
            expiredTimePref.Value = 0;
            HideSelf();

            levelManager?.OnPackBought();
            packSwitcher?.CheckActivePack(0);

            _packServiceRegistry.Instance.lastPackBought.Value = ((int)bannerName);
        }
    }
}
