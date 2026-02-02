using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GrillSort.DailyMission
{
    [CreateAssetMenu(fileName = "DailyMission_VeggieHunt", menuName = "My Services/DailyMissionService/DailyMission_VeggieHunt")]
    public class DailyMission_VeggieHunt : DailyMissionBase
    {
        public override DailyMissionConfig.EMission MissionType => DailyMissionConfig.EMission.Veggie_Hunt;

        [SerializeField] private List<int> veggieIds = new();

        public override void Activate()
        {
            GameplayController.OnCollectItemCount += OnCollectItem;
        }

        public override void Deactivate()
        {
            GameplayController.OnCollectItemCount -= OnCollectItem;
        }

        private void OnCollectItem(int id, int count)
        {
            if (!veggieIds.Contains(id)) return;

            AddTempItem(count);
        }

#if UNITY_EDITOR
        [Button]
        protected void AssignData(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                veggieIds.Clear();
                Debug.LogWarning("AssignData: input string is empty.");
                return;
            }

            veggieIds = data.Split(',')
                .Select(s => s.Trim())                // loại bỏ khoảng trắng
                .Where(s => int.TryParse(s, out _))   // chỉ giữ các phần tử có thể parse được
                .Select(int.Parse)                    // parse sang int
                .ToList();

            Debug.Log($"AssignData: assigned {veggieIds.Count} veggieIds.");
        }

#endif
    }
}