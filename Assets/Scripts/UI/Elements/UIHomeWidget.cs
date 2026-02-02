using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SonatFramework.Systems.ObjectPooling;
using SonatFramework.Systems;
using UI.Elements;
using UnityEngine;
using System;
using System.Threading;
using DG.Tweening;
using Sonat.Enums;

public class UIHomeWidget : MonoBehaviour, IHomeProcess
{
    public bool forceOpen = false;
    [SerializeField] protected HomeWidgetManager manager;

    [SerializeField] protected bool active = true;
    [SerializeField] private int levelShow = 5;
    [SerializeField] private int DayShow = 3;

    public virtual void Setup()
    {

    }

    public virtual void OnFocus()
    {
    }

    public virtual void OnLoseFocus()
    {
    }

    public virtual async UniTask<bool> ProcessTask()
    {
        return false;
    }

    public virtual async UniTask ProcessOnFocus()
    {
        return;
    }

    protected virtual bool CheckActive()
    {
        int level = MySonatFramework.userDataService.GetLevel();
        int day = MySonatFramework.userDataService.UserDay;
        return level >= levelShow && day >= DayShow;
    }

    protected virtual void Collect(int value, Sprite sprite, Action callback = null, float delay = 2, Vector2 size = default)
    {
        var effect = SonatSystem.GetService<PoolingService>().Create<UIAddItemFeature>
            ("UIAddItemFeature", transform.position, transform);

        effect.Setup(value, sprite, transform.position, () =>
        {
            callback?.Invoke();
            transform.DOPunchScale(Vector3.one * 0.1f, 0.4f);
            MySonatFramework.audioService.PlaySound(AudioId.Items_Collected_Grill_sort);
        }, delay, size);
    }
}