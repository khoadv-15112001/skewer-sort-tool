using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UINotificationToggle : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private GameObject objOn;
    [SerializeField] private GameObject objOff;

    private bool isOn { get => PlayerPrefs.GetInt("Notification_isOn", 0) == 1; set => PlayerPrefs.SetInt("Notification_isOn", value ? 1 : 0); }

    void OnEnable()
    {
        // slider.onValueChanged.AddListener(OnSliderValueChanged);
        SetValue(isOn);
    }

    void OnDisable()
    {
        // slider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }

    private void OnSliderValueChanged(float value)
    {

        var isOn = value >= 0.5f;
        SetValue(isOn);
    }

    public void OnClick()
    {
        isOn = !isOn;
        SetValue(isOn);
    }

    public void SetValue(bool isOn)
    {
        slider.value = isOn ? 1 : 0;
        objOn.SetActive(isOn);
        objOff.SetActive(!isOn);
    }
}
