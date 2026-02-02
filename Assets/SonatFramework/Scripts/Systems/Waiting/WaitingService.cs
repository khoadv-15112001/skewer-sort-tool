#if sonat_sdk_v2
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sonat.AdsModule;
using Sonat.IapModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(WaitingService), menuName = "Sonat Services/Waiting Service")]
public class WaitingService : SonatServiceSo, IServiceInitialize
{
    [SerializeField] private string popupWaitingAdsName = "PopupWaiting";
    private PopupWaitingBase popupWaitingBase;

    public void Initialize()
    {
        SonatAds.OnFullScreenAd += OnFullScreenAds;
    }

    private void OnFullScreenAds(bool isFullScreen)
    {
        if (isFullScreen)
        {
            ShowPopupWaiting(popupWaitingAdsName).Forget();
        }
        else
        {
            if (popupWaitingBase != null)
            {
                popupWaitingBase.Close();
            }
        }
    }

    private async UniTaskVoid ShowPopupWaiting(string popupWaitingName)
    {
        popupWaitingBase = await PanelManager.Instance.OpenPanelByNameAsync<PopupWaitingBase>(popupWaitingName);
    }
}
#endif