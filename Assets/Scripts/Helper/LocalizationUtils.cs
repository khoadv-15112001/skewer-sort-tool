using I2.Loc;
using Manager;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LocalizationUtils
{
    private const string KEY_JAPAN = "localize_japan";

    public static bool ForceJapan { get => PlayerPrefs.GetInt("force_japan", 0) == 1; set => PlayerPrefs.SetInt("force_japan", value ? 1 : 0); }
    //
    public static bool IsJapanese()
    {
        return ForceJapan ? true : GameRemoteConfigValue.localizeJapan;
    }

    public static bool IsJapaneseDeviceLanguage()
    {
        var lang = Application.systemLanguage;
        return lang == SystemLanguage.Japanese;
    }

    public static bool IsLocalizeJapanLastFetch()
    {
        if (!PlayerPrefs.HasKey($"{KEY_JAPAN}"))
            return false;

        return PlayerPrefs.GetInt($"{KEY_JAPAN}") == 1;
    }

    public static bool IsLocalizeJapanSplash()
    {
        if (ForceJapan) return true;

        if (PlayerPrefs.HasKey(KEY_JAPAN))
        {
            return IsLocalizeJapanLastFetch();
        }

        if (IsJapaneseDeviceLanguage())
        {
            return true;
        }

        return false;
    }

    public static bool IsLanguage(string languageCode)
    {
        return LocalizationManager.CurrentLanguageCode.Equals(languageCode);
    }

    public static IEnumerable<string> GetAllTerms()
    {
#if UNITY_EDITOR
        var sourcePath = LocalizationManager.GlobalSources[0];
        var asset = Resources.Load<LanguageSourceAsset>(sourcePath);
        if (asset == null || asset.mSource == null)
            return new[] { "(source not found or invalid)" };

        return asset.mSource
            .GetTermsList()
            .Distinct()
            .OrderBy(term => term);
#endif
        return null;
    }
}
