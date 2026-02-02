using Gameplay.Entities;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;

namespace PrototypeTest
{
    public class TimeBonusVisual : MonoBehaviour, IPoolingObject
    {
        [SerializeField] private Transform visual;

        public void SetItem(Item item)
        {
        }

        public void OnBonusTime()
        {
            EventBus<AddTimeBonusEvent>.Raise(new() { position = visual.position });
        }

        public void Setup()
        {
        }

        public void OnCreateObj(params object[] args)
        {
        }

        public void OnReturnObj()
        {
        }
    }

    public struct AddTimeBonusEvent : IEvent
    {
        public Vector3 position;
    }
}