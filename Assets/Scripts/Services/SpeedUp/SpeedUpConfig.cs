using System;
using System.Collections.Generic;
using Sonat.Enums;
using UnityEngine;

namespace GrillSort.SpeedUp
{
    [CreateAssetMenu(fileName = "SpeedUpConfig", menuName = "My Configs/SpeedUpConfig")]
    public class SpeedUpConfig : ScriptableObject
    {
        [Header("Pricing Settings")]
        [Tooltip("Default coin price if remote config is not available")]
        public int defaultCoinPrice = 100;
        
        [Tooltip("Remote config key for segment-based pricing")]
        public string remoteConfigKey = "speed_up_price_by_segment";
        
        [Header("Reward Ad Settings")]
        [Tooltip("Enable/disable reward ads option")]
        public bool enableRewardAds = true;
        
        [Tooltip("Tracking item type for ads")]
        public string adsItemType = "booster";
        
        [Tooltip("Tracking item ID for ads")]
        public string adsItemId = "speed_up";
        
        [Header("Logging Settings")]
        [Tooltip("Earn type for coin tracking")]
        public string coinEarnType = "booster";
        
        [Tooltip("Earn ID for coin tracking")]
        public string coinEarnId = "speed_up";
        
        [Header("Speed Up Effects")]
        [Tooltip("Speed multiplier when speed up is active")]
        [Range(1.5f, 5f)]
        public float speedMultiplier = 2f;
        
        [Tooltip("Duration of speed up effect in seconds (0 = instant)")]
        public float duration = 0f;
        
        [Header("Bubble Show Conditions")]
        [Tooltip("Minimum gameplay seconds before bubble can appear")]
        public float minSecondsToShow = 5f;
        
        [Tooltip("Maximum gameplay seconds for bubble to appear (after this, bubble won't show)")]
        public float maxSecondsToShow = 30f;
        
        [Header("Campaign Segment Prices (For Editor Preview)")]
        [Tooltip("These are for reference only. Actual values come from Firebase Remote Config")]
        public List<SegmentPrice> segmentPrices = new List<SegmentPrice>
        {
            new SegmentPrice { segment = "IAP", price = 80 },
            new SegmentPrice { segment = "IAA", price = 100 },
            new SegmentPrice { segment = "Hybrid", price = 90 },
            new SegmentPrice { segment = "Organic", price = 100 }
        };
    }
    
    [Serializable]
    public class SegmentPrice
    {
        public string segment;
        public int price;
    }
}

