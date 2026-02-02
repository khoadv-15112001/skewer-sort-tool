using System;
using UnityEngine;

public class PackLevelManager
{
    private LineConfig config;
    private string prefsKeyPrefix;

    // các biến internal (private)
    private int currentLevel;
    private int purchasedCount;
    private int daysShown;

    private bool wasShownToday;

    public int CurrentLevel => currentLevel;
    public int PurchasedCount => purchasedCount;
    public int DaysShown => daysShown;
    public bool WasShownToday { get => wasShownToday; }

    public PackLevelManager(LineConfig cfg)
    {
        config = cfg;
        prefsKeyPrefix = $"{cfg.bannerName}";
        LoadState();
    }

    private void LoadState()
    {
        int max = config.MaxLevel;
        currentLevel = PlayerPrefs.GetInt(prefsKeyPrefix + "_CurrentLevel", 1);
        currentLevel = Mathf.Clamp(currentLevel, 1, max);

        purchasedCount = PlayerPrefs.GetInt(prefsKeyPrefix + "_PurchasedCount", 0);
        daysShown = PlayerPrefs.GetInt(prefsKeyPrefix + "_DaysShown", 0);
        wasShownToday = PlayerPrefs.GetInt(prefsKeyPrefix + "_WasShownToday", 0) == 1;
    }

    private void SaveState()
    {
        PlayerPrefs.SetInt(prefsKeyPrefix + "_CurrentLevel", currentLevel);
        PlayerPrefs.SetInt(prefsKeyPrefix + "_PurchasedCount", purchasedCount);
        PlayerPrefs.SetInt(prefsKeyPrefix + "_DaysShown", daysShown);
        PlayerPrefs.SetInt(prefsKeyPrefix + "_WasShownToday", wasShownToday ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void OnNewDay()
    {
        if (wasShownToday)
        {
            daysShown += 1;
            Debug.Log($"anhnt: OnNewDay {prefsKeyPrefix} daysShown ={daysShown}");

        }
        wasShownToday = false;

        TryDowngrade();  // hạ cấp nếu cần

        SaveState();
    }

    public void MarkShownToday()
    {
        wasShownToday = true;
        SaveState();
    }

    public void OnPackBought()
    {
        purchasedCount += 1;
        daysShown = 0;

        var cfg = config.GetConfigForLevel(currentLevel);
        if (cfg != null && purchasedCount >= cfg.purchaseLimit)
        {
            if (currentLevel < config.MaxLevel)
            {
                currentLevel += 1;
            }
            else
            {
                // nếu đang max, quay về level 1
                currentLevel = 1;
            }
            // reset lại state ở cấp mới
            purchasedCount = 0;
            daysShown = 0;
        }

        SaveState();
    }

    /// <summary>
    /// Kiểm tra hạ cấp do daysShown >= membershipDays và chưa mua trong level đó
    /// </summary>
    public void TryDowngrade()
    {
        var cfg = config.GetConfigForLevel(currentLevel);
        if (cfg == null) return;

        if (daysShown >= cfg.membershipDays && currentLevel > 1)
        {
            currentLevel -= 1;
            purchasedCount = 0;
            daysShown = 0;
        }
    }

    public PackLevelConfig GetCurrentLevelConfig()
    {
        return config.GetConfigForLevel(currentLevel);
    }

    public void DowngradeToPreviousLevel()
    {
        if (currentLevel > 1)
        {
            currentLevel -= 1;
            purchasedCount = 0;
            daysShown = 0;
            SaveState();
        }
    }

    internal object GetConfigForLevel(int i)
    {
        throw new NotImplementedException();
    }
}
