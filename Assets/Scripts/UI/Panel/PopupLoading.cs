using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;

public class PopupLoading : Panel
{
    private float time = 1;
    [SerializeField] private Slider slider;
    
    public override void Open(UIData uiData)
    {
        base.Open(uiData);
        if (uiData != null && uiData.TryGet("Time", out time))
        {
            SonatUtils.DelayCall(time, Close);
        }
        slider.value = 0;
    }

    public override void OnOpenCompleted()
    {
        base.OnOpenCompleted();
        slider.DOValue(1, time);
    }
}
