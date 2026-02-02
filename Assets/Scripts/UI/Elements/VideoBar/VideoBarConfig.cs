using System;
using System.Collections.Generic;
using UnityEngine;
using SonatFramework.Systems.InventoryManagement.GameResources;
using Newtonsoft.Json;
using Sirenix.OdinInspector;

namespace GrillSort.Services
{
    [CreateAssetMenu(fileName = "VideoBarConfig", menuName = "Sonat Configs Custom/VideoBarConfig")]
    [Serializable]
    public class VideoBarConfig : ScriptableObject
    {
        [Header("General Settings")]
        public bool active = true;
        public int levelStart = 5;
        public int retentionDay = 1;
        public int maxWatchPerDay = 5;
        public bool showWidget = true;

        [Header("Rewards by milestone")]
        public List<VideoBarRewardData> datas = new();

        [Button("Print JSON Config")]
        private void PrintJsonConfig()
        {
            Debug.Log(JsonConvert.SerializeObject(this));
        }
    }

    [Serializable]
    public class VideoBarRewardData
    {
        public int watchCount;
        public RewardData rewards = new();
    }
}
