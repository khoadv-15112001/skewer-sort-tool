using System;
using System.Collections;
using System.Collections.Generic;
using Sonat.Enums;
using TMPro;
using UnityEngine;

public class DropdownLevelMode : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;

    private void OnEnable()
    {
        Init();
    }

    public void Init()
    {
        dropdown.ClearOptions();
        List<TMP_Dropdown.OptionData> opts = new List<TMP_Dropdown.OptionData>();
        for (LevelMode i = LevelMode.All; i < LevelMode.MAX; i++)
        {
            if (!Enum.IsDefined(typeof(LevelMode), i)) continue;
            TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData()
            {
                text = i.ToString()
            };
            opts.Add(data);
        }

        dropdown.ClearOptions();
        dropdown.options = opts;
        dropdown.onValueChanged.RemoveAllListeners();
        dropdown.onValueChanged.AddListener(OnLevelModeOptChange);
        OnLevelModeOptChange(0);
    }

    private void OnLevelModeOptChange(int value)
    {
        var levelMode = GetLevelModeOpt();
        switch (levelMode)
        {
            case LevelMode.All:
                break;
            case LevelMode.Target:
                break;
        }
    }

    public LevelMode GetLevelModeOpt()
    {
        LevelMode levelMode = (LevelMode)dropdown.value;
        return levelMode;
    }

    public void SetLevelModeOpt(LevelMode levelMode)
    {
        dropdown.value = (int)levelMode;
    }
}
