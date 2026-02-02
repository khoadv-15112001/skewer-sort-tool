using GrillSort.Winstreak;
using I2.Loc;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMultiEventLose_Winstreak : UIMultiEventLoseBase
{
    [SerializeField] private UIMultiEventLoseController.EVerbType lastVerbType;

    [ValueDropdown(nameof(GetAllTerms))]
    [SerializeField] private string lastTerm;

    [SerializeField] private Image icon;
    [SerializeField] private List<Sprite> sprites;

    private int value;

    public override bool Setup()
    {
        var service = MySonatFramework.GetService<WinStreakManager>();

        if (!service.EventActive.BoolValue) return false;

        icon.sprite = GetIconSprite();

        return true;
    }

    private Sprite GetIconSprite()
    {
        var service = MySonatFramework.GetService<WinStreakManager>();
        value = service.lives.Value;

        return sprites[Mathf.Clamp(value, 0, sprites.Count - 1)];
    }

    public override UIMultiEventLoseController.EVerbType GetVerbType()
    {
        if (value == 0) return lastVerbType;

        return base.GetVerbType();
    }

    public override string GetName()
    {
        if (value == 0) return LocalizationManager.GetTranslation(lastTerm);

        else return base.GetName();
    }
}
