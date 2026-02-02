using System.Collections.Generic;
using SonatFramework.Systems;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;

namespace MyGame.Modules.CardCollection
{
    public class UIStarGroup : MonoBehaviour
    {
        [SerializeField] private Transform container;
        private readonly Service<PoolingContainerService> _poolingService = new();
        private List<UIStar> stars = new();

        public void Setup(int star)
        {
            _poolingService.Instance.CleanContainer(container);
            stars.Clear();
            for (int i = 0; i < star; i++)
            {
                var starObj = _poolingService.Instance.CreateObject<UIStar>(container);
                stars.Add(starObj);
            }
        }

        public void SetData(bool isOn)
        {
            for (int i = 0; i < stars.Count; i++)
            {
                stars[i].SetData(isOn);
            }
        }
    }
}