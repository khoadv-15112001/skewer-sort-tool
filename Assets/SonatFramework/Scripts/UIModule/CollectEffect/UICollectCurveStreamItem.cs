using DG.Tweening;
using Sonat.Enums;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;

namespace SonatFramework.Scripts.UIModule.CollectEffect
{
    public class UICollectCurveStreamItem : MonoBehaviour, IPoolingObject
    {
        [SerializeField] private UIResourceItem uiResourceItem;
        public Transform Transform => transform;

        public void Setup()
        {
        }

        public void OnCreateObj(params object[] args)
        {
            transform.DOKill();
            uiResourceItem.SetData((GameResource)args[0]);
        }

        public void OnReturnObj()
        {
        }
    }
}