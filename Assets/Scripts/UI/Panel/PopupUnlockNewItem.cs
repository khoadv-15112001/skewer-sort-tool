using System.Collections;
using System.Collections.Generic;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using UnityEngine;

public class PopupUnlockNewItem : Panel
{
    [SerializeField] private FixedImageRatio fixedImageRatio;

    public override void Open(UIData uiData)
    {
        base.Open(uiData);
        Sprite sprite = uiData.Get<Sprite>("Icon");
        fixedImageRatio.SetSprite(sprite);
    }
}
