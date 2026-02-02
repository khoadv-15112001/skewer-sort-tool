using Cysharp.Threading.Tasks;
using GrillSort.RealTime;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems;
using SonatUI.PageSlider;
using System;
using UnityEngine;

namespace GrillSort.PackServiceRegistry
{
    public class PackSwitcherNew : MonoBehaviour
    {
        [SerializeField] private UIPackLevelScheduler[] packs;  // index 0 = level1 UI, etc.
        [SerializeField] private GameObject hideableObject;
        [SerializeField] private BannerName bannerName;  // tên dòng (phải khớp với config.lineName)
        private LineConfig lineConfig;
        private PackLevelManager levelManager;
        private readonly Service<PackServiceRegistry> registry = new();
        private bool isCheck = false;
        public bool isShopPanel = false;

        private void Awake()
        {
            if (packs == null || packs.Length == 0)
            {
                Debug.LogError($"{nameof(PackSwitcherNew)} on {name} has no packs assigned!");
            }
            // Lấy manager từ service registry
            if (registry.Instance == null)
            {
                Debug.LogError("PackServiceRegistry not found");
            }
            levelManager = registry.Instance.GetManager(bannerName);

            lineConfig = registry.Instance.LineConfigs.Find(x => x.bannerName == bannerName);
            if (levelManager == null)
            {
                Debug.LogError($"No PackLevelManager for line {bannerName}");
                // fallback khởi mới nếu muốn
                //levelManager = new PackLevelManager(lineConfig);
            }
        }

        private void OnEnable()
        {
            if (!isShopPanel)
                PageSlider.OnPageVisible += CheckActivePack;

            CheckActivePack(registry.Instance.lastPackBought.Value);
        }
        private void OnDisable()
        {
            if (!isShopPanel)
                PageSlider.OnPageVisible -= CheckActivePack;
        }
        public void CheckActivePack(int pageId)
        {
            if (packs == null) return;  // hoặc chờ tới khi packs gán

            if (!isShopPanel && (isCheck || lineConfig.bannerName != PageOrderController.Instance.ConvertPageIDToPackLine(pageId)))
            {
                return;
            }
            isCheck = true;

            registry.Instance.HandleNextDayInternalIfNeeded();
            HideAll();

            levelManager.TryDowngrade();

            int cur = levelManager.CurrentLevel;
            var cfg = levelManager.GetCurrentLevelConfig();
            if (cfg == null)
            {
                Debug.LogWarning("Huge lineConfig has no cfg for level " + cur);
                hideableObject.SetActive(false);
                return;
            }

            if (levelManager.PurchasedCount >= cfg.purchaseLimit)
            {
                levelManager.OnPackBought();
                cur = levelManager.CurrentLevel;
                cfg = levelManager.GetCurrentLevelConfig();

                // Kiểm tra purchaseLimit sau khi mua
                while (cfg != null && cfg.purchaseLimit == 0 && cur < lineConfig.MaxLevel)
                {
                    levelManager.OnPackBought();
                    cur = levelManager.CurrentLevel;
                    cfg = levelManager.GetCurrentLevelConfig();
                }
            }
            else
            {
                // cur == maxLevel && mua hết → giữ cur, không ẩn
                Debug.Log("Huge at max level, purchasedCount >= limit, but keep showing max UI");
            }

            Debug.Log($"packs.Length = {packs.Length}");

            int idx = cur - 1;
            if (idx >= 0 && idx < packs.Length)
            {
                var ui = packs[idx];
                if (ui != null)
                {
                    ui.Initialize(cfg, levelManager, this);
                    ui.gameObject.SetActive(true);
                    levelManager.MarkShownToday();
                }
                else
                {
                    Debug.LogError($"PackSwitcherNew.CheckActivePack: packs[{idx}] is null for line {bannerName}");
                    hideableObject.SetActive(false);
                    return;
                }
            }
            else
            {
                Debug.LogError($"PackSwitcherNew.CheckActivePack: idx {idx} out of range (0..{packs.Length - 1})");
                hideableObject.SetActive(false);
                return;
            }

            levelManager.MarkShownToday();
            hideableObject.SetActive(true);

            Debug.Log($"anhnt: CheckActivePack {bannerName}, pageId={pageId}");

        }
        private void HideAll()
        {
            if (packs == null)
            {
                Debug.LogWarning("HideAll: packs array is null");
                return;
            }
            for (int i = 0; i < packs.Length; i++)
            {
                var p = packs[i];
                if (p != null)
                {
                    p.gameObject.SetActive(false);
                }
            }
        }

        internal int GetLinePriority()
        {
            return levelManager.GetCurrentLevelConfig().price;
        }

        public BannerName BannerName => bannerName;
    }
}
