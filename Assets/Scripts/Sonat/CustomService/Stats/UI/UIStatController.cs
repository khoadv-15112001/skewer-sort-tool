using GrillSort.OnlineService;
using SonatFramework.Systems;
using SonatFramework.Systems.ObjectPooling;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrillSort.Stats
{
    public class UIStatController : MonoBehaviour
    {
        [SerializeField] private Transform container;

        private PoolingContainerService poolingService => SonatSystem.GetService<PoolingContainerService>();
        private StatsService statsService => SonatSystem.GetService<StatsService>();

        public void BindSelf()
        {
            poolingService.CleanContainer(container);

            foreach (var stat in statsService.Stats)
            {
                var item = poolingService.CreateObject<UIStat>(container);

                item.BindData(stat.StatType);
            }
        }

        public void Bind(GeneralStats generalStats)
        {
            poolingService.CleanContainer(container);

            if (generalStats == null)
                generalStats = new();

            var statDict = ToDictionary(generalStats);

            foreach (var kvp in statDict)
            {
                var item = poolingService.CreateObject<UIStat>(container);
                item.BindData(kvp.Key, kvp.Value);
            }
        }

        public static Dictionary<StatsConfig.StatType, int> ToDictionary(GeneralStats stats)
        {
            return new Dictionary<StatsConfig.StatType, int>
            {
                { StatsConfig.StatType.FirstTryWin, stats.firstTryWin },
                { StatsConfig.StatType.LongestStreak, stats.longestStreak },
                { StatsConfig.StatType.TreasureQuestWins, stats.treasureQuestWins },
                { StatsConfig.StatType.CollectionsCompleted, stats.collectionsCompleted },
                { StatsConfig.StatType.SetsCompleted, stats.setsCompleted },
            };
        }
    }
}