using SonatFramework.Systems.ConfigManagement;
using SonatFramework.Systems.InventoryManagement.GameResources;
using UnityEngine;

namespace SonatFramework.Systems.AdsBreakManagement
{
    [CreateAssetMenu(fileName = "AdsBreakConfig", menuName = "Sonat Configs/AdsBreak Config")]
    public class AdsBreakConfig : ConfigSo
    {
        public int timeGap = 120;
        public int levelStartAdBreak = 5;
        public int stateStartAdBreak = 1;
        public RewardData reward;
    }
}
