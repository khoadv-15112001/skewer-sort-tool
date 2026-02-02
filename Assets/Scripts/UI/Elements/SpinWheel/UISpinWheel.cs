using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using GrillSort.EndlessTreasure;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.InventoryManagement.GameResources;
using UnityEngine;

namespace GrillSort.LuckySpin
{
    public class UISpinWheel : MonoBehaviour
    {
        [SerializeField] private float spinDuration = 2f;
        [SerializeField] private RectTransform wheelTransform;
        [SerializeField] private int totalReward = 8;
        [SerializeField] private int minSpin = 10;
        [SerializeField] private int maxSpin = 15;
        [SerializeField] private AnimationCurve spinCurve;

        [Header("UI")]
        [SerializeField] private List<UIRewardItem> rewardItems;

        public bool IsSpinning => isSpinning;
        private bool isSpinning = false;
        private Dictionary<UIRewardItem, float> rewardWeights = new();
        private int currentRewardIndex = 0;
        private List<SpinRewardData> rewards = new();

        public Action<ResourceData> onSpinEnd;

        public void SetData(List<SpinRewardData> rewards)
        {
            this.rewards = rewards;

            rewardWeights.Clear();
            float totalWeight = 0;
            for (int i = 0; i < rewardItems.Count; i++)
            {
                totalWeight += rewards[i].weight;
            }

            for (int i = 0; i < rewardItems.Count; i++)
            {
                rewardItems[i].Init(rewards[i].reward.resource, rewards[i].reward.quantity);
                rewardWeights.Add(rewardItems[i], rewards[i].weight / totalWeight);
            }
        }

        // Gọi khi nhấn nút "Spin"
        public void StartSpin()
        {
            if (isSpinning) return;

            isSpinning = true;
            MySonatFramework.audioService.PlaySound(AudioId.Spin_Rolling_Grill_sort);

            // float targetAngle = Random.Range(360 * minSpin, 360 * maxSpin);
            // int rewardIndex = (int)(targetAngle / (360.0f / totalReward));
            // 1. Chọn phần thưởng theo xác suất
            currentRewardIndex = GetRandomRewardByWeight();

            // 2. Tính góc cần xoay đến phần thưởng đó
            float anglePerReward = 360f / totalReward;
            // float rewardAngle = Random.Range((currentRewardIndex - 0.5f) * anglePerReward, (currentRewardIndex + 0.5f) * anglePerReward);
            float rewardAngle = currentRewardIndex * anglePerReward;

            // 3. Quay nhiều vòng và kết thúc tại phần thưởng
            int fullRounds = UnityEngine.Random.Range(minSpin, maxSpin + 1);
            float totalAngle = fullRounds * 360f + rewardAngle;
            // Quay nhiều vòng rồi dừng tại phần thưởng

            wheelTransform
                .DORotate(new Vector3(0, 0, -totalAngle), spinDuration, RotateMode.FastBeyond360)
                .SetEase(spinCurve) // làm chậm dần
                .OnComplete(() =>
                {
                    isSpinning = false;
                    onSpinEnd?.Invoke(GetReward());

                });
        }

        private int GetRandomRewardByWeight()
        {
            float rand = UnityEngine.Random.value; // random từ 0 đến 1
            float cumulative = 0f;
            currentRewardIndex = 0;
            foreach (var kvp in rewardWeights)
            {
                cumulative += kvp.Value;
                if (rand <= cumulative)
                {
                    return rewardItems.IndexOf(kvp.Key);
                }
            }

            return 0;
        }

        public ResourceData GetReward()
        {
            return new ResourceData()
            {
                resource = rewards[currentRewardIndex].reward.resource,
                quantity = rewards[currentRewardIndex].reward.quantity
            };
        }
    }

}
