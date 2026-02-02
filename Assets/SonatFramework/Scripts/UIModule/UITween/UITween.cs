using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace SonatFramework.Scripts.UIModule
{
    public static class UITween
    {
        public static async UniTask Play(TweenData tweenData)
        {
            tweenData.SetupData();
            if (tweenData.config == null) return;
            if (tweenData.target == null) return;

            var cancellationToken = tweenData.target.GetCancellationTokenOnDestroy();

            try
            {
                switch (tweenData.config.tweenType)
                {
                    case UITweenType.Scale:
                        await PlayTweenScale(tweenData, cancellationToken);
                        break;
                    case UITweenType.Fade:
                        await PlayTweenFade(tweenData, cancellationToken);
                        break;
                    case UITweenType.Move:
                        await PlayTweenMove(tweenData, cancellationToken);
                        break;
                    case UITweenType.LocalMove:
                        await PlayTweenLocalMove(tweenData, cancellationToken);
                        break;
                    case UITweenType.RectLocalMove:
                        await PlayTweenRectLocalMove(tweenData, cancellationToken);
                        break;
                    case UITweenType.FadeGroup:
                        await PlayTweenFadeGroup(tweenData, cancellationToken);
                        break;
                    case UITweenType.Active:
                        await PlayTweenActive(tweenData, cancellationToken);
                        break;
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private static async UniTask PlayTweenScale(TweenData tweenData, CancellationToken cancellationToken)
        {
            var target = tweenData.target;
            target.localScale = Vector3.one * tweenData.config.from;
            await target.DOScale(tweenData.config.to, tweenData.config.duration).SetEase(tweenData.config.curve)
                .SetDelay(tweenData.config.delay).OnComplete(() => { tweenData.OnCompleted?.Invoke(); })
                .ToUniTask(cancellationToken: cancellationToken);
        }

        private static async UniTask PlayTweenFade(TweenData tweenData, CancellationToken cancellationToken)
        {
            var target = tweenData.target.GetComponent<Graphic>();
            var color = target.color;
            color.a = tweenData.config.from;
            target.color = color;
            await target.DOFade(tweenData.config.to, tweenData.config.duration).SetEase(tweenData.config.curve)
                .SetDelay(tweenData.config.delay).OnComplete(() => { tweenData.OnCompleted?.Invoke(); })
                .ToUniTask(cancellationToken: cancellationToken);
        }

        private static async UniTask PlayTweenMove(TweenData tweenData, CancellationToken cancellationToken)
        {
            var target = tweenData.target;
            target.position = tweenData.config.mFrom;
            await target.DOMove(tweenData.config.mTo, tweenData.config.duration).SetEase(tweenData.config.curve)
                .SetDelay(tweenData.config.delay).OnComplete(() => { tweenData.OnCompleted?.Invoke(); })
                .ToUniTask(cancellationToken: cancellationToken);
        }

        private static async UniTask PlayTweenLocalMove(TweenData tweenData, CancellationToken cancellationToken)
        {
            var target = tweenData.target;
            target.localPosition = tweenData.config.mFrom;
            await target.DOLocalMove(tweenData.config.mTo, tweenData.config.duration).SetEase(tweenData.config.curve)
                .SetDelay(tweenData.config.delay).OnComplete(() => { tweenData.OnCompleted?.Invoke(); })
                .ToUniTask(cancellationToken: cancellationToken);
        }

        private static async UniTask PlayTweenRectLocalMove(TweenData tweenData, CancellationToken cancellationToken)
        {
            var target = (RectTransform)tweenData.target;
            target.anchoredPosition = tweenData.config.mFrom;
            await target.DOAnchorPos(tweenData.config.mTo, tweenData.config.duration).SetEase(tweenData.config.curve)
                .SetDelay(tweenData.config.delay).OnComplete(() => { tweenData.OnCompleted?.Invoke(); })
                .ToUniTask(cancellationToken: cancellationToken);
        }

        private static async UniTask PlayTweenFadeGroup(TweenData tweenData, CancellationToken cancellationToken)
        {
            if (tweenData?.target == null) return;

            if (!tweenData.target.TryGetComponent<CanvasGroup>(out var target)) return;

            if (target.Equals(null)) return;

            target.alpha = tweenData.config.from;

            await target.DOFade(tweenData.config.to, tweenData.config.duration)
                .SetEase(tweenData.config.curve)
                .SetDelay(tweenData.config.delay)
                .OnComplete(() => tweenData.OnCompleted?.Invoke())
                .ToUniTask(cancellationToken: cancellationToken);
        }

        private static async UniTask PlayTweenActive(TweenData tweenData, CancellationToken cancellationToken)
        {
            tweenData.target.gameObject.SetActive(false);
            await DOVirtual.DelayedCall(tweenData.config.delay, () =>
            {
                tweenData.target.gameObject.SetActive(true);
            }).ToUniTask(cancellationToken: cancellationToken);
        }
    }


    public enum UITweenType
    {
        None,
        Scale,
        Fade,
        Move,
        LocalMove,
        RectLocalMove,
        FadeGroup,
        Active
    }
}