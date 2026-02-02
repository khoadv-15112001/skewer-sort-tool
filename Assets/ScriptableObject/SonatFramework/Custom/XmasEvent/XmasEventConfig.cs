using System;
using System.Collections.Generic;
using UnityEngine;
using Sonat.Enums;
using Sonat.IapModule;

namespace GrillSort.XmasEvent
{
    [CreateAssetMenu(fileName = "XmasEventConfig", menuName = "Sonat Configs Custom/XmasEventConfig")]
    [Serializable]
    public class XmasEventConfig : ScriptableObject
    {
        [Header("Event Time Range")]
        public string startDate = "2025-12-01";
        public string endDate = "2025-12-31";

        [Header("Segment Packs")]
        public List<XmasSegmentData> segmentData;
    }

    [Serializable]
    public class XmasSegmentData
    {
        public UserGroup segmentType; // non_payer, minnow, ...

        public List<XmasPackTier> packTiers = new(); // 3 pack nhỏ
    }

    [Serializable]
    public class XmasPackTier
    {
        [Header("Pack Info")]
        public ShopItemKey shopItemKey; // key pack IAP - rewards lấy từ ShopConfig
        
        [Header("Discount")]
        public int discountPercent = 50; // 50%, 70%, etc.
      
    }
}
