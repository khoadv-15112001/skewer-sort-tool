using Cysharp.Threading.Tasks;
using GrillSort.PackServiceRegistry;
using SonatFramework.Systems;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PageOrderController : SingletonSimple<PageOrderController>
{
    [Serializable]
    public class PageLine
    {
        public BannerName bannerName;
        public Transform page;
    }

    [Header("Pages (in content)")]
    [SerializeField] private List<PageLine> pageLines = new();

    private readonly Service<PackServiceRegistry> registry = new();

    private void Start()
    {
        MoveLinePageToFront(registry.Instance.GetLinePageToFront()).Forget();
    }

    /// <summary>
    /// Gọi khi pack của dòng bannerName được mua → reorder pages
    /// </summary>
    public async UniTaskVoid MoveLinePageToFront(BannerName purchasedLine)
    {
        await UniTask.WaitForSeconds(0.1f);
        // Sort các PageLine trong list
        pageLines.Sort((a, b) =>
        {
            // Nếu a là purchasedLine, nó về trước
            if (a.bannerName == purchasedLine && b.bannerName != purchasedLine)
                return -1;
            if (b.bannerName == purchasedLine && a.bannerName != purchasedLine)
                return +1;

            // Cả hai không phải purchasedLine → sắp theo priority giảm
            int pa = GetLinePriority(a.bannerName);
            int pb = GetLinePriority(b.bannerName);
            // pa - pb để sắp tăng
            return pa.CompareTo(pb);
        });

        // Đưa thứ tự trong hierarchy content theo list mới
        for (int i = 0; i < pageLines.Count; i++)
        {
            var pl = pageLines[i];
            pl.page.SetSiblingIndex(i);
        }
    }
    private int GetLinePriority(BannerName line)
    {
        // Ví dụ giả sử bạn có service / registry để lấy priority (giá) từ config:
        var mgr = registry.Instance.GetManager(line);
        if (mgr != null)
        {
            var cfg = mgr.GetCurrentLevelConfig();
            if (cfg != null)
            {
                return cfg.price;
            }
        }
        return 0;
    }
    public BannerName ConvertPageIDToPackLine(int id)
    {
        if (id >= pageLines.Count)
        {
            Debug.Log($"anhnt: convert page id = {id} failed, fallback to 0");
            id = 0;

        }
        return pageLines[id].bannerName;
    }
}
