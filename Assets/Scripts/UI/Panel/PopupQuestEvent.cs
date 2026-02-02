using SonatFramework.Scripts.UIModule;
using UnityEngine;

public class PopupQuestEvent : Panel
{
    public GameObject finishedObj;
    public override void Open(UIData uiData)
    {
        finishedObj.SetActive(false);

        base.Open(uiData);
    }

    public void SetFinished()
    {
        finishedObj.SetActive(true);
    }
}