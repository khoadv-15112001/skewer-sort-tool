using System;
using System.Collections.Generic;
using Gameplay.Entities;
using Sirenix.OdinInspector;
using SonatFramework.Systems.InventoryManagement.GameResources;
using UnityEngine;

namespace MyGame.Modules.CardCollection
{
    [CreateAssetMenu(fileName = "StarExchangeConfig", menuName = "Sonat Configs/CardCollection/StarExchangeConfig")]
    public class StarExchangeConfig : ScriptableObject
    {
        [SerializeField]
        public List<StarExchangeReward> milestones;

#if UNITY_EDITOR
        void OnValidate()
        {
            int idx = 0;
            foreach (var item in milestones)
            {
                item.index = idx++;
            }
        }
#endif
    }

    [Serializable]
    public class StarExchangeReward
    {
        [GUIColor(0.2f, 1f, 0.2f)]
        [SerializeField, ReadOnly] public int index;
        public int star;
        public RewardData reward;
    }
}


