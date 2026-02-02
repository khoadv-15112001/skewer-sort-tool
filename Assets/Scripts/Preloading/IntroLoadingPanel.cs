using System;
using System.Collections;
using System.Collections.Generic;
using Base.Singleton;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class IntroLoadingPanel : MonoBehaviour
{
    public static IntroLoadingPanel instance;

    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] private Slider slider;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        slider.value = 0;
        DOSlider(0.9f, 5);
    }

    public void DOSlider(float value, float duration)
    {
        slider.DOKill();
        slider.DOValue(value, duration);
    }

    public void FadeOut()
    {
        slider.DOKill(true);
        canvasGroup.DOFade(0, 0.5f).OnComplete(() => { Destroy(gameObject); });
    }
}