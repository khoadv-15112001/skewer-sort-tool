using DG.Tweening;
using SonatFramework.Scripts.UIModule;
using UnityEngine;

public class PopupPreWin : Panel
{
    public float delay = 1.5f;
    public override void OnOpenCompleted()
    {
        base.OnOpenCompleted();
        DOVirtual.DelayedCall(delay, Close);
    }
}
