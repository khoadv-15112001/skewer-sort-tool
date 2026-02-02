using System.Collections;
using System.Collections.Generic;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using UnityEngine;

public class PopupTutIceLock : PopupTutNewMode
{
    public override void Close()
    {
        base.Close();

        //PanelManager.Instance.OpenPanelByName<PopupTutNewMode>("PopupTutIceLock1");
    }
}
