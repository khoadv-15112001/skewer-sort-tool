using System.Collections;
using System.Collections.Generic;
using Sonat.Data;
using Sonat.Enums;
using SonatFramework.Scripts.Helper;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using UnityEngine;

public class CheckRemoteByLevelCounterHLW
{
    private string key;
    private int defaultValue;
    private IntDataPref lastLevel;
    private IntDataPref lastStartCount;
    private string counterKey;

    public CheckRemoteByLevelCounterHLW(string key, int defaultValue)
    {
        this.key = key;
        this.defaultValue = defaultValue;
        lastLevel = new($"LastLevel_{this.key}", 0);
        lastStartCount = new($"LastStartCount_{this.key}", 0);
        counterKey = $"counter_{key}_level";
    }

    public CheckRemoteByLevelCounterHLW(string key, string counterKeyCustom, int defaultValue)
    {
        this.key = key;
        this.defaultValue = defaultValue;
        lastLevel = new($"LastLevel_{this.key}", 0);
        lastStartCount = new($"LastStartCount_{this.key}", 0);
        counterKey = $"counter_{counterKeyCustom}_{key}_level";
    }

    public int GetCurrentValue()
    {
        int currentLevel = MySonatFramework.userDataService.GetLevel(GameMode.SkewerJam);
        int currentStartCount = MySonatFramework.customTrackingService.HLWTrackingService.startCount;
        if (currentLevel != lastLevel.Value || currentStartCount != lastStartCount.Value)
        {
            lastLevel.Value = currentLevel;
            lastStartCount.Value = currentStartCount;
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
        int currentLevel = MySonatFramework.userDataService.GetLevel();
        int condition = SonatSDKAdapter.GetValueByLevelSegment(key, UserData.UserCampaignSegment.Value, currentLevel, defaultValue);
        int currentValue = GetCurrentValue();
        return currentValue < condition;
    }
}