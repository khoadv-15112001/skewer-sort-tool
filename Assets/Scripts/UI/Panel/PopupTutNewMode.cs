using Gameplay;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using UnityEngine;

public class PopupTutNewMode : Panel
{
    [SerializeField] private float delayClose = 5f;
    public override void Open(UIData uiData)
    {
        base.Open(uiData);
        SonatUtils.DelayCall(delayClose, Close, this);
        MySonatFramework.customTrackingService.LogTutorialBegin(gameObject.name.ToLogString(), 0, 0);
    }

    public override void Close()
    {
        MySonatFramework.customTrackingService.LogTutorialComplete(gameObject.name.ToLogString(), 0, 0);
        base.Close();
        UIFlowController.isShowedTut = false;
    }
}
