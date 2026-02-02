using UnityEngine;

public class NotifyFetchDataPanel : NotifyPanelBase
{
    public static string LOG_TAG = "[NotifyFetchDataPanel]";
    public static string DATA_KEY = "NotifyFetchDataPanel";
    public static bool CanFetchData
    {
        get => PlayerPrefs.GetInt($"{DATA_KEY}_CanFetchData", 0) == 1;
        private set => PlayerPrefs.SetInt($"{DATA_KEY}_CanFetchData", value ? 1 : 0);
    }

    public void OnCancelFetch()
    {
        OnCloseCompleted();
        CanFetchData = false;
    }
}