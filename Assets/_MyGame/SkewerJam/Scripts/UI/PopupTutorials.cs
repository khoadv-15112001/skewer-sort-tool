using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using UnityEngine;

public class PopupTutorials : Panel
{
    [SerializeField] private float delay;

    private bool readyClose = false;

    public override void Open(UIData uiData)
    {
        base.Open(uiData);
        readyClose = false;

        SonatUtils.DelayCall(delay, () => readyClose = true, this);
    }

    public void OnClickWaitingClose()
    {
        if (readyClose)
        {
            Close();
        }
    }
}
