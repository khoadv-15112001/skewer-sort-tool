using SonatFramework.Systems;
using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using SonatFramework.Scripts.Helper;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Systems.EventBus;
using Sonat.Data;

namespace GrillSort.LoseRwdService
{
    [CreateAssetMenu(fileName = "LoseRwdService", menuName = "My Services/Lose Rwd Service")]
    public class LoseRwdService : SonatServiceSo, IServiceInitialize
    {
        public string Prefix = "lose_rwd";
        public LoseRwdConfig _config;

        private IntDataPref _currentStage;
        private bool _featureActive;

        public LoseRwdConfig Config => _config;
        public int CurrentStage => _currentStage.Value;

        public void Initialize()
        {
            _config = SonatSDKAdapter.GetRemoteConfig<LoseRwdConfig>($"{Prefix}_config", _config);
            Debug.Log("anhnt: [LoseRwdService] _config active = " + _config.active);
            _currentStage = new IntDataPref($"{Prefix}_currentStage", 1);

            new EventBinding<LevelEndedEvent>(OnLevelEnded);

            ResetData();
        }
        private void ResetData()
        {
            _currentStage.Value = 1;

            _featureActive = _config.active;
        }
        private void OnLevelEnded(LevelEndedEvent @event)
        {
            ResetData();
        }

        public bool CanShowLoseRwd()
        {
            if (!RwdGlobalHelper.RwdGlobalEnable) return false;

            return _featureActive && _currentStage.Value <= _config.maxStage;
        }
        public LoseRwdData GetCurrentStageData()
        {
            if (_currentStage.Value - 1 < _config.datas.Count)
                return _config.datas[_currentStage.Value - 1];
            return null;
        }

        /// <summary>
        /// Gọi khi user đã xem xong ads qua popup continue (OnReviveWithAds)
        /// </summary>
        public void OnReviveSuccess()
        {
            Debug.Log("anhnt: [LoseRwdService] _config loop = " + _config.loopByLevel);

            if (_featureActive && _config.loopByLevel && _currentStage.Value == _config.maxStage)
            {
                ResetData();
                return;
            }

            if (!CanShowLoseRwd()) return;
            _currentStage.Value++;
        }
    }
}
