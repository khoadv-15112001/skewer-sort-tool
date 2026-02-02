using System;
using DG.Tweening;
using Sonat.Enums;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;

namespace SkewerJam.Utils.Effects
{
    public class UICollectResouceText : MonoBehaviour, IPoolingObject
    {
        [SerializeField] protected UIResourceItem uiResourceItem;
        [SerializeField] protected AnimationCurve moveXCurve, moveYCurve;
        [SerializeField] protected float delayMove;
        [SerializeField] protected float duration;
        [SerializeField] protected float height;

        private Vector3 startPosition;
        private Vector3 targetPosition;

        public virtual void OnCreateObj(params object[] args)
        {
            transform.DOKill();
            GameResource resource = (GameResource)args[0];
            int quantity = (int)args[1];
            startPosition = (Vector3)args[2];
            targetPosition = startPosition + new Vector3(0, height, 0);

            transform.position = startPosition;
            uiResourceItem.SetData(resource, quantity);

            if (TryGetComponent<Canvas>(out Canvas canvas))
            {
                canvas.overrideSorting = true;
                canvas.sortingLayerName = "UI_Top";
                canvas.sortingOrder = 1000;
            }
            DOEffect();
        }

        private void DOEffect()
        {
            transform.DOMoveY(targetPosition.y, duration).SetEase(moveYCurve).SetDelay(delayMove);
            transform.DOMoveX(targetPosition.x, duration).SetEase(moveXCurve).SetDelay(delayMove).OnComplete(() =>
            {
                Remove();
            });
        }

        private void Remove()
        {
            // gameObject.SetActive(false);
            SonatSystem.GetService<PoolingServiceAsync>().ReturnObj(this);
        }

        public void OnReturnObj()
        {
        }

        public void Setup()
        {
        }
    }
}
