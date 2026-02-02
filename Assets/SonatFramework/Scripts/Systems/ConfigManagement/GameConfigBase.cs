using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sonat.Enums;
using SonatFramework.Systems.InventoryManagement.GameResources;

namespace SonatFramework.Systems.ConfigManagement
{
    public abstract class GameConfigBase : ConfigSo
    {
        public ResourceData winReward;
        public ResourceData playOnPrice;

        [ShowInInspector] public Dictionary<GameMode, int> totalLevel = new();
    }
}