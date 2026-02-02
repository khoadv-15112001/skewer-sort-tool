using SonatFramework.Scripts.UIModule;
using UnityEngine;

public class PopupGoldenSlice : Panel
{
    [SerializeField] private GameObject btnPlay;
    [SerializeField] private GameObject btnContinue;
    [SerializeField] private GameObject btnClose;

    public override void Open(UIData uiData)
    {
        base.Open(uiData);

        if (uiData != null && uiData.Get<bool>("hideButtonPlay") == true)
        {
            btnPlay.SetActive(false);
            btnContinue.SetActive(true);
            btnClose.SetActive(false);
        }
        else
        {
            btnPlay.SetActive(true);
            btnContinue.SetActive(false);
            btnClose.SetActive(true);
        }



        if (PanelManager.Instance.GetPanel<PopupQuestEvent>() != null)
        {
            PanelManager.Instance.ClosePanel<PopupQuestEvent>();
        }
    }

    public void OnClickContinue()
    {
        PanelManager.Instance.OpenPanelByName<BasePanel>("PopupTutQuestEvent");
        Close();
    }
}
