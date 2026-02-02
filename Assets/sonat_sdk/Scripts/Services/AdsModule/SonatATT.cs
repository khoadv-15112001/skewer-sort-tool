#if UNITY_IOS
using System.Collections;
using System.Runtime.InteropServices;
using Sonat.Debugger;
using Sonat;
using Sonat.TrackingModule;
using Unity.Advertisement.IosSupport;
using UnityEngine;

namespace Sonat.AdsModule
{
    public class SonatATT
    {
        public void CheckRequest()
        {
            AskAtt();
            bool trackingEnabled;
            if ((int)ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == 3)
                trackingEnabled = true; //If==3, App is AUTHORIZED in settings
            else trackingEnabled = false; //DENIED, RESTRICTED or NOT DETERMINED (==2,1,0)

            AdSettings.SetAdvertiserTrackingEnabled(trackingEnabled);

            SonatSdkManager.instance.StartCoroutine(WaitCheckAtt());
        }

        private static void AskAtt()
        {
            CheckShowLogAtt("CALL", "call");
#if using_max
            return;
#endif

            if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() ==
                ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
            {
                ATTrackingStatusBinding.RequestAuthorizationTracking();
            }
        }

        private static void CheckShowLogAtt(string action, string status)
        {
            if (!PlayerPrefs.HasKey($"SONAT_LOG_ATT_{action}"))
            {
                new SonatLogATT()
                {
                    status = status
                }.Post();
            }
        }

        private IEnumerator WaitCheckAtt()
        {
            yield return new WaitForSeconds(1);
            string status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus().ToString();
            SonatDebugType.Ads.Log($"Att Status: {status}");
            CheckShowLogAtt("STATUS", status);
        }
    }


    public static class AdSettings
    {
        [DllImport("__Internal")]
        private static extern void FBAdSettingsBridgeSetAdvertiserTrackingEnabled(bool advertiserTrackingEnabled);

        public static void SetAdvertiserTrackingEnabled(bool advertiserTrackingEnabled)
        {
#if !UNITY_EDITOR
            FBAdSettingsBridgeSetAdvertiserTrackingEnabled(advertiserTrackingEnabled);
#endif
        }
    }
}
#endif