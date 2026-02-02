using System;
using Sonat.FirebaseModule.RemoteConfig;
using UnityEngine;

namespace Sonat.FirebaseModule
{
    [Serializable]
    public class FirebaseDefaultSettings
    {
        [SerializeField] private PlayerPrefRemoteArrayInt logLevelStart = new PlayerPrefRemoteArrayInt(
            "log_level_start_array", new[]
            {
                1, 4, 8, 10, 20,30,40,50,60,70,80,90,100
            });

        public PlayerPrefRemoteArrayInt LogLevelStart => logLevelStart;
        
        [SerializeField] private PlayerPrefRemoteArrayInt logInterstitialAdsStart = new PlayerPrefRemoteArrayInt(
            "log_interstitial_ads_start_array", new[]
            {
                1, 4, 8, 10, 20,30,40,50,60,70,80,90,100
            });

        public PlayerPrefRemoteArrayInt LogInterstitialAdsStart => logInterstitialAdsStart;

        [SerializeField] private PlayerPrefRemoteArrayInt logPaidAd = new PlayerPrefRemoteArrayInt(
            "log_paid_ad_array", new[]
            {
                0
            });

        public PlayerPrefRemoteArrayInt LogPaidAd => logPaidAd;
    
    }
}