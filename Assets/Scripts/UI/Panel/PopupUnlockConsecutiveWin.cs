using System.Collections;
using System.Collections.Generic;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;

public class PopupUnlockConsecutiveWin : Panel
{
    public class Data : UIData
    {
        public Transform consecutiveWinTransform;
    }

    [SerializeField] private Transform arrowTransform;
    [SerializeField] private Transform highlightTransform;

    public override void Open(UIData uiData)
    {
        base.Open(uiData);
        Data data = (Data)uiData;
        arrowTransform.position = data.consecutiveWinTransform.position;
        highlightTransform.position = data.consecutiveWinTransform.position;

    }


    public void OpenTutConsecutiveWin()
    {
        Close();
        PanelManager.Instance.OpenPanelByName<BasePanel>("PopupTutConsecutiveWin");
    }
}
