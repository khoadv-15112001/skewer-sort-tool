using System.Collections.Generic;
using UnityEngine;

namespace SonatFramework.Systems.InventoryManagement.GameResources
{
    [CreateAssetMenu(menuName = "Sonat/ListRewardData")]
    public class ListRewardData : ScriptableObject
    {
        public List<RewardData> rewards;
    }
}