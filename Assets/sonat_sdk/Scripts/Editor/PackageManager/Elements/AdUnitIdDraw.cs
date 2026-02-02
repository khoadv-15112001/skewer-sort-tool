using System;
using Sonat.AdsModule;
using UnityEditor;
using UnityEngine;

namespace Sonat.Editor.PackageManager.Elements
{
    public class AdUnitIdDraw
    {
        private AdUnitId adUnitId;
        private AdsConfigDraw adsConfigDraw;

        public AdUnitIdDraw(AdUnitId adUnitId, AdsConfigDraw adsConfigDraw)
        {
            this.adUnitId = adUnitId;
            this.adsConfigDraw = adsConfigDraw;
        }


        public void Show()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Ad Unit Id:", GUILayout.Width(60));
            adUnitId.id = EditorGUILayout.TextField(adUnitId.id, GUILayout.ExpandWidth(true), GUILayout.MaxWidth(300));
            GUILayout.Space(10);
            adUnitId.placement = (AdPlacement)EditorGUILayout.EnumPopup(adUnitId.placement, GUILayout.Width(100));
            GUILayout.Space(5);
            switch (adUnitId.placement)
            {
                case AdPlacement.Banner:
                    if (!CheckAdTypeBanner(adUnitId.adType))
                    {
                        adUnitId.adType = AdType.Banner;
                    }

                    adUnitId.adType = (AdType)EditorGUILayout.EnumPopup(new GUIContent(), adUnitId.adType, CheckAdTypeBanner, false, GUILayout.Width(100));
                    break;

                case AdPlacement.Native:
                    if (!CheckAdTypeNative(adUnitId.adType))
                    {
                        adUnitId.adType = AdType.NativeAds;
                    }

                    adUnitId.adType = (AdType)EditorGUILayout.EnumPopup(new GUIContent(), adUnitId.adType, CheckAdTypeNative, false, GUILayout.Width(100));
                    break;
                case AdPlacement.OnFocus:
                    if (!CheckAdTypeOnFocus(adUnitId.adType))
                    {
                        adUnitId.adType = AdType.AppOpenAd;
                    }

                    adUnitId.adType = (AdType)EditorGUILayout.EnumPopup(new GUIContent(), adUnitId.adType, CheckAdTypeOnFocus, false, GUILayout.Width(100));
                    break;
                case AdPlacement.AppOpen:
                    if (!CheckAdTypeAppOpen(adUnitId.adType))
                    {
                        adUnitId.adType = AdType.AppOpenAd;
                    }

                    adUnitId.adType = (AdType)EditorGUILayout.EnumPopup(new GUIContent(), adUnitId.adType, CheckAdTypeAppOpen, false, GUILayout.Width(100));
                    break;
                case AdPlacement.Interstitial:
                    if (!CheckAdTypeInterstitial(adUnitId.adType))
                    {
                        adUnitId.adType = AdType.Interstitial;
                    }

                    adUnitId.adType =
                        (AdType)EditorGUILayout.EnumPopup(new GUIContent(), adUnitId.adType, CheckAdTypeInterstitial, false, GUILayout.Width(100));
                    break;
                case AdPlacement.Rewarded:
                    if (!CheckAdTypeRewarded(adUnitId.adType))
                    {
                        adUnitId.adType = AdType.Rewarded;
                    }

                    adUnitId.adType = (AdType)EditorGUILayout.EnumPopup(new GUIContent(), adUnitId.adType, CheckAdTypeRewarded, false, GUILayout.Width(100));
                    break;
                case AdPlacement.AdsBreak:
                    if (!CheckAdTypeAdsBreak(adUnitId.adType))
                    {
                        adUnitId.adType = AdType.Interstitial;
                    }

                    adUnitId.adType =
                        (AdType)EditorGUILayout.EnumPopup(new GUIContent(), adUnitId.adType, CheckAdTypeAdsBreak, false, GUILayout.Width(100));
                    break;
            }

            GUILayout.Space(5);

            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                adsConfigDraw.RemoveItem(adUnitId);
            }

            EditorGUILayout.EndHorizontal();
        }

        private bool CheckAdTypeInterstitial(Enum adType)
        {
            switch (adType)
            {
                case AdType.Interstitial:
#if using_aps
                case AdType.InterstitialVideo:
#endif
                    return true;
                default:
                    return false;
            }
        }

        private bool CheckAdTypeAdsBreak(Enum adType)
        {
            switch (adType)
            {
                case AdType.Interstitial:
                case AdType.AppOpenAd:
#if using_aps
                case AdType.InterstitialVideo:
#endif
                    return true;
                default:
                    return false;
            }
        }

        private bool CheckAdTypeRewarded(Enum adType)
        {
            switch (adType)
            {
                case AdType.Rewarded:
                    return true;
                default:
                    return false;
            }
        }

        private bool CheckAdTypeBanner(Enum adType)
        {
            switch (adType)
            {
                case AdType.Banner:
                case AdType.CollapsibleBanner:
#if using_aps
                case AdType.LargeBanner:
#endif
                    return true;
                default:
                    return false;
            }
        }

        private bool CheckAdTypeAppOpen(Enum adType)
        {
            switch (adType)
            {
                case AdType.Interstitial:
                case AdType.AppOpenAd:
                    return true;
                default:
                    return false;
            }
        }

        private bool CheckAdTypeOnFocus(Enum adType)
        {
            switch (adType)
            {
                case AdType.Interstitial:
                case AdType.AppOpenAd:
                    return true;
                default:
                    return false;
            }
        }

        private bool CheckAdTypeNative(Enum adType)
        {
            switch (adType)
            {
                case AdType.NativeAds:
                case AdType.LargeBanner:
                case AdType.MREC:
                    return true;
                default:
                    return false;
            }
        }
    }
}