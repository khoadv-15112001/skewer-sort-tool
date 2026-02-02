using System;
using System.Collections;
using System.Collections.Generic;
using Sonat.Enums;
using SonatFramework.Scripts.Utils;
using TMPro;
using UnityEngine;

public class DropdownLevelDifficultyValue : MonoBehaviour
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
        for (int i = 0; i < 5; i++)
        {
            TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData()
            {
                text = i.ToString()
            };
            opts.Add(data);
        }

        dropdown.ClearOptions();
        dropdown.options = opts;
        dropdown.onValueChanged.RemoveAllListeners();
        dropdown.onValueChanged.AddListener(OnDifficultyOptChange);
        OnDifficultyOptChange(0);
    }

    private void OnDifficultyOptChange(int value)
    {
        var levelDifficultyValue = GetDifficultyValueOpt();
        // switch (levelDifficulty)
        // {
        //     case LevelDifficulty.Easy:
        //         break;
        //     case LevelDifficulty.Hard:
        //         break;
        //     case LevelDifficulty.SuperHard:
        //         break;
        // }
    }

    public int GetDifficultyValueOpt()
    {
        return dropdown.value;
    }

    public void SetDifficultyValueOpt(int levelDifficultyValue)
    {
        dropdown.value = levelDifficultyValue;
    }
}
