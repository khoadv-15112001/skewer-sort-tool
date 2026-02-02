using I2.Loc;
using SonatFramework.Scripts.UIModule;
using UnityEngine;
using UnityEngine.UI;

public class PopupLanguage : Panel
{
    [SerializeField] UIButtonLanguage[] uiButtonLanguages;


    public override void Open(UIData uiData)
    {
        base.Open(uiData);
        foreach (var btn in uiButtonLanguages)
        {
            if (languageName[(int)btn.language] == LocalizationManager.CurrentLanguage)
            {
                btn.Selected();
            }
            else
            {
                btn.Unselected();
            }
        }
    }

    public void OnClickButtonLanguage(ELanguage language)
    {
        foreach (var btn in uiButtonLanguages)
        {
            if (btn.language != language)
            {
                btn.Unselected();
            }
        }
    }

#if UNITY_EDITOR
    public static string[] txtLanguages = {
            "English",
            "한국어",
            "日本語",
            "中国人",
            "Português",
            "Español",
            "Bahasa Indonesia",
            "Türkçe",
            "Deutsch",
            "Français",
            "Tiếng Việt",
            "Pilipino",
            "Italiano",
            "Português",
            "Русский",
            "ไทย",
            "Arabic",
            "हिन्दी",
        };
#endif

    public static string[] languageName = {
        "English",
        "Korean",
        "Japanese",
        "Chinese",
        "Portuguese",
        "Spanish",
        "Indonesian",
        "Turkish",
        "German",
        "French",
        "Vietnamese",
        "Pilipino",
        "Italiano",
        "Portuguese (Brazil)",
        "Russian",
        "Thai",
        "Arabic",
        "Hindi",
    };
}


public enum ELanguage
{
    English,
    Korean,
    Japanese,
    Chinese,
    Portuguese,
    Spanish,
    Indonesian,
    Turkish,
    German,
    French,
    Vietnamese,
    Philippines,
    Italy,
    Brazil,
    Russian,
    Thai,
    Arabic,
    Hindi,
}
