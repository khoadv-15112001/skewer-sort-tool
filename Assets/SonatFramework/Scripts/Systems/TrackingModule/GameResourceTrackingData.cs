using System.Collections.Generic;
using Sonat.Enums;
using SonatFramework.Scripts.Utils;
using UnityEngine;

namespace SonatFramework.Systems.TrackingModule
{
    [CreateAssetMenu(menuName = "Sonat Services/Tracking Services/Game Resource Tracking Data", fileName = "GameResourceTrackingData")]
    public class GameResourceTrackingData : ScriptableObject
    {
        public List<GameResourceTrackingConfig> gameResourceTrackingConfigs;
    }

    [System.Serializable]
    public class GameResourceTrackingConfig
    {
        public string gameResourceName;
        public string trackingName;
        public int exchange;

        public string GetTrackingName()
        {
            if (!string.IsNullOrEmpty(trackingName)) return trackingName;
            return gameResourceName.ToLogString();
        }

        public int GetValueCurrency(int quantity)
        {
            return exchange * quantity;
        }
    }
}