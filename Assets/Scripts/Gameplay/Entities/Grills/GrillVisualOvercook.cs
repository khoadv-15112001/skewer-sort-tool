using Gameplay.Entities.GrillScripts;
using SonatFramework.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using Gameplay;

public class GrillVisualOvercook : GrillVisual
{
    [SerializeField]
    private Slider timeSlider;
    [SerializeField]
    private TMP_Text timeTMP;
    [SerializeField]
    private Sprite orangeSlider;
    [SerializeField]
    private Sprite redSlider;
    [SerializeField]
    private Image sliderImage;

    private float lastSliderValue;

    public void SetUpTimeSlider(int maxTime)
    {
        ResetParam();
        timeSlider.maxValue = maxTime;
        timeSlider.value = maxTime;
        timeTMP.text = SonatUtils.FormatTimeFromSec((int)maxTime);
    }

    private void ResetParam()
    {
        timeSlider.gameObject.SetActive(true);
        sliderImage.sprite = orangeSlider;
    }

    public void UpdateSliderValue(float value)
    {
        lastSliderValue = timeSlider.value;
        timeSlider.DOValue(value, 1f).From(lastSliderValue).SetEase(Ease.Linear);
        SetSliderSprite(value);
        timeTMP.text = SonatUtils.FormatTimeFromSec((int)value);
    }

    private void SetSliderSprite(float value)
    {
        if (value <= (timeSlider.maxValue / 3))
        {
            sliderImage.sprite = redSlider;
        }
        else
        {
            sliderImage.sprite = orangeSlider;
        }
    }

    public void OnComplete()
    {
        timeSlider.value = timeSlider.maxValue;
        timeTMP.text = "";
        CloseGrill();
        timeSlider.gameObject.SetActive(false);
    }

    public override void CloseGrill(bool doEffect = true)
    {
        lid.transform.DOKill();
        lid.gameObject.SetActive(true);
        if (doEffect)
        {
            lid.transform.localScale = Vector3.one * 0.8f;
            lid.transform.localPosition = new Vector3(0, 2.5f, 0);
            lid.SetAlpha(0);
            lid.transform.DOScale(0.85f, GameDefine.grillLidAnim);
            lid.transform.DOLocalMoveY(defaultLidPos, GameDefine.grillLidAnim).SetEase(Ease.InQuad);
            lid.DOFade(1, GameDefine.grillLidAnim).SetEase(Ease.OutQuad);
        }
        else
        {
            lid.transform.localScale = Vector3.one;
            lid.transform.SetLocalPositionY(defaultLidPos);
            lid.SetAlpha(1);
        }
    }
}
