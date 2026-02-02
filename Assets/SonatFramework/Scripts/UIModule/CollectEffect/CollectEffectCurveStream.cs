using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sonat.Enums;
using SonatFramework.Scripts.Helper;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;

namespace SonatFramework.Scripts.UIModule.CollectEffect
{
    [CreateAssetMenu(menuName = "Sonat Anims/CollectEffectCurveStream")]
    public class CollectEffectCurveStream : CollectEffectControllerBase
    {
        [SerializeField] private float duration;
        [SerializeField] private float timeGap = 0.15f;
        [SerializeField] private AnimationCurve xCurve, yCurve;
        [SerializeField]private Service<PoolingService> poolingService = new();

        public async UniTaskVoid CreateEffect(Vector3 position, GameResource gameResource, int number,
            Action callback = null)
        {
            if (gameResource.ResourceType() == GameResourceType.Currency)
            {
                var uiCurrency = UIResourceBar.Main.GetUICurrency(gameResource);
                if (uiCurrency != null)
                {
                    var durationCounter = (number - 1) * timeGap;
                    uiCurrency.UpdateValue(durationCounter, duration);
                    for (var i = 0; i < number; i++)
                    {
                        var effectItem = poolingService.Instance.Create<UICollectCurveStreamItem>(
                            "UICollectCurveStreamItem", position,
                            PanelManager.Instance.transform, gameResource);

                        effectItem.transform.DOMoveX(uiCurrency.icon.transform.position.x, duration).SetEase(xCurve);
                        effectItem.transform.DOMoveY(uiCurrency.icon.transform.position.y, duration).SetEase(yCurve)
                                .onComplete =
                            () =>
                            {
                                uiCurrency.PlayCollectEffect();
                                poolingService.Instance.ReturnObj(effectItem);
                            };
                        await UniTask.Delay(TimeSpan.FromSeconds(timeGap));
                    }

                    await UniTask.Delay(TimeSpan.FromSeconds(duration));
                    Service<InventoryService>.Get().NotiUpdateResource(gameResource);
                    callback?.Invoke();
                }
            }
        }
    }
}