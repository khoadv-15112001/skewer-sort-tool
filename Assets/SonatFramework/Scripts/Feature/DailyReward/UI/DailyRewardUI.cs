using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;
using UnityEngine.UI;

namespace SonatFramework.Scripts.Feature.DailyReward.UI
{
    public class DailyRewardUI : MonoBehaviour
    {
        [SerializeField] private UIProgressBar progressBar;
        [SerializeField] private ScrollRect scroll;
        [SerializeField] private Transform container;

        [SerializeField] private readonly Service<DailyRewardManager> dailyRewardManager = new();
        [SerializeField] private readonly Service<PoolingContainerService> poolingService = new();

        private DailyRewardConfig config;
        private DailyRewardData data;

        private void Setup()
        {
            config = dailyRewardManager.Instance.configs;
            data = dailyRewardManager.Instance.data;
        }

        public void OnOpenCompleted()
        {
            scroll.enabled = true;
        }

        public void Open(UIData uiData)
        {
            scroll.enabled = false;
            progressBar.SetData(data.currentStreak, config.MaxDay());
            SetupUI();
        }

        protected virtual void SetupUI()
        {
            foreach (var dailyReward in config.rewards)
            {
                var item = poolingService.Instance.CreateObject<DailyRewardItem>(container);
                item.SetData(dailyReward, data.currentStreak);
            }
        }
    }
}