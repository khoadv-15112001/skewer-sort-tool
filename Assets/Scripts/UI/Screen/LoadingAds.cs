using System;
using Sonat.AdsModule;
using UnityEngine;

public class LoadingAds : MonoBehaviour
{
    [SerializeField] private GameObject panel;

    private void Start()
    {
        SonatAds.OnFullScreenAd += SonatAds_OnFullScreenAd;
    }

    private void SonatAds_OnFullScreenAd(bool value)
    {
        panel.SetActive(value);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        panel.SetActive(false);
    }
}
