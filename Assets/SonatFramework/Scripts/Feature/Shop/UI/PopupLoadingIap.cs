using System;
using System.Collections;
using System.Collections.Generic;
using SonatFramework.Scripts.UIModule;
using UnityEngine;

public class PopupLoadingIap : Panel
{
    private float timeAutoClose = 10;

    public override void Open(UIData uiData)
    {
        base.Open(uiData);
        if (uiData != null && uiData.TryGet("time_to_close", out timeAutoClose))
        {
            
        }
        else
        {
            timeAutoClose = 10;
        }
    }

    public override void OnOpenCompleted()
    {
        base.OnOpenCompleted();
        StartCoroutine(IEWaitToClose());
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            Close();
        }
    }

    IEnumerator IEWaitToClose()
    {
        yield return new WaitForSeconds(timeAutoClose);
        Close();
    }
}
