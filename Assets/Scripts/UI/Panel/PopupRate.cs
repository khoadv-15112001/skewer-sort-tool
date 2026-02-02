using System;
using System.Collections;
using System.Collections.Generic;
using Manager;
using Sonat;
using SonatFramework.Scripts.UIModule;
using UnityEngine;
using UnityEngine.UI;

public class PopupRate : Panel
{
    // private const string LAST_OPEN_POPUP_RATE_KEY = "last_open_popup_rate";
    // public const string DAILY_OPEN_POPUP_RATE_KEY = "daily_open_popup_rate";


    [SerializeField] private Transform starContainer;
    private UIStarRate[] stars;
    private int starSelected = 5;

    public override void OnSetup()
    {
        base.OnSetup();
        stars = starContainer.GetComponentsInChildren<UIStarRate>();
    }

    public override void Open(UIData uiData)
    {
        base.Open(uiData);
        AnimStar();
    }

    private void AnimStar()
    {
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].Setup(i, this);
        }
    }

    public void OnSelectStar(int star)
    {
        starSelected = star + 1;
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].SetStar(i <= star);
        }
    }

    public void RateClick()
    {
#if UNITY_ANDROID
        if (starSelected >= GameRemoteConfigValue.starRate)
            Application.OpenURL("https://play.google.com/store/apps/details?id=com.grill.sorting.food.match.puzzle&pcampaignid=web_share");
#elif UNITY_IOS
            Application.OpenURL($"https://apps.apple.com/us/app/id{SonatSdkManager.Settings.appID_IOS}");
#endif
        Close();
    }


    // public void TrackingOpenPopupRate()
    // {
    //     string today = DateTime.Now.ToString("yyyy-MM-dd");
    //     string lastOpenDate = PlayerPrefs.GetString(LAST_OPEN_POPUP_RATE_KEY, "");

    //     if (lastOpenDate == today)
    //     {
    //         // Cùng ngày => tăng số lần mở
    //         int count = PlayerPrefs.GetInt(DAILY_OPEN_POPUP_RATE_KEY, 0);
    //         count++;
    //         PlayerPrefs.SetInt(DAILY_OPEN_POPUP_RATE_KEY, count);
    //     }
    //     else
    //     {
    //         // Ngày mới => reset lại đếm
    //         PlayerPrefs.SetString(LAST_OPEN_POPUP_RATE_KEY, today);
    //         PlayerPrefs.SetInt(DAILY_OPEN_POPUP_RATE_KEY, 1);
    //     }
    //     PlayerPrefs.Save(); // Đảm bảo lưu

    // }
}
