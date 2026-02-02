using System.Collections;
using System.Collections.Generic;
using Sonat.Data;
using SonatFramework.Scripts.Helper;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using UnityEngine;

public class CheckRemoteByDayCounter
{
    private string key;
    private int defaultValue;
    private IntDataPref lastDay;
    private string counterKey;


    public CheckRemoteByDayCounter(string key, int defaultValue)
    {
        this.key = key;
        this.defaultValue = defaultValue;
        lastDay = new($"LastDay_{this.key}", 0);
        counterKey = $"counter_{key}";
    }
    
    public CheckRemoteByDayCounter(string key, string customKeyCounter, int defaultValue)
    {
        this.key = key;
        this.defaultValue = defaultValue;
        lastDay = new($"LastDay_{this.key}", 0);
        counterKey = $"counter_{customKeyCounter}_{key}";
    }

    public int GetCurrentValue()
    {
        int currentDay = MySonatFramework.userDataService.UserDay;
        if (currentDay != lastDay.Value)
        {
            lastDay.Value = currentDay;
            PlayerPrefs.SetInt(counterKey, 0);
            return 0;
        }

        return PlayerPrefs.GetInt(counterKey, 0);
    }

    public void AddValue(int add = 1)
    {
        int value = PlayerPrefs.GetInt(counterKey, 0);
        value += add;
        PlayerPrefs.SetInt(counterKey, value);
    }

    public bool CheckCounter()
    {
        if (!RwdGlobalHelper.RwdGlobalEnable) 
        {
            Debug.Log($"anhnt: [UserCampaignSegment] CheckRemoteByDayCounter: RwdGlobalEnable is false");
            return false; 
        }

        int currentDay = MySonatFramework.userDataService.UserDay;
        //int condition = SonatSDKAdapter.GetValueByLevel($"{UserData.UserCampaignSegment.Value}_{key}", currentDay, defaultValue);
        int condition = SonatSDKAdapter.GetValueByLevelSegment(key, UserData.UserCampaignSegment.Value, currentDay, defaultValue);
        int currentValue = GetCurrentValue();
        return currentValue < condition;
    }
}