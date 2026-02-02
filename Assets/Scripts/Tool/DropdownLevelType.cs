using System;
using System.Collections;
using System.Collections.Generic;
using Sonat.Enums;
using SonatFramework.Scripts.Utils;
using TMPro;
using UnityEngine;

public class DropdownLevelType : MonoBehaviour
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
        for (LevelType i = LevelType.Food; i < LevelType.MAX; i++)
        {
            if (!Enum.IsDefined(typeof(LevelType), i)) continue;
            TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData()
            {
                text = i.ToString()
            };
            opts.Add(data);
        }

        dropdown.ClearOptions();
        dropdown.options = opts;
        dropdown.onValueChanged.RemoveAllListeners();
        dropdown.onValueChanged.AddListener(OnLevelTypeOptChange);
        OnLevelTypeOptChange(0);
    }

    private void OnLevelTypeOptChange(int value)
    {
        var levelType = GetLevelTypeOpt();
        switch (levelType)
        {
            case LevelType.Food:
                break;
            case LevelType.Fruit:
            case LevelType.Cake:
                break;
        }
    }

    public LevelType GetLevelTypeOpt()
    {
        LevelType levelType = dropdown.options[dropdown.value].text.ToEnum<LevelType>();
        return levelType;
    }

    public void SetLevelTypeOpt(LevelType levelType)
    {
        dropdown.value = (int)levelType;
    }
}