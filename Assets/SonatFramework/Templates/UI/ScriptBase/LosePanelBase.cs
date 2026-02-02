using System;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using UnityEngine;

public class LosePanelBase : Panel
{
    public class Data : UIData
    {
        public Action onRetryClick;
    }
    protected Data data;
    protected bool clicked = false;
    [SerializeField] protected bool showNative;
    protected bool nativeHided;

    public override void Open(UIData uiData)
    {
        base.Open(uiData);
        data = (Data)uiData;
        if (showNative)
        {
            SonatSDKAdapter.ShowNativeAds();
        }
    }

    public override void OnFocus()
    {
        base.OnFocus();
        if (showNative && nativeHided)
        {
            SonatSDKAdapter.ShowNativeAds();
        }
    }
    
    public override void OnFocusLost()
    {
        base.OnFocusLost();
        if (!showNative) return;
        nativeHided = true;
        SonatSDKAdapter.HideNavtiveAds();
    }

    public virtual void RetryClick()
    {
        if(clicked) return;
        SonatSDKAdapter.ShowInterAds("lose", Retry);
    }

    protected virtual void Retry()
    {
        if(clicked) return;
        clicked = true;
        Close();
        data?.onRetryClick?.Invoke();
    }
    
}
