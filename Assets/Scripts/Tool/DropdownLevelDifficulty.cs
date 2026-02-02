using System;
using System.Collections;
using System.Collections.Generic;
using Sonat.Enums;
using SonatFramework.Scripts.Utils;
using TMPro;
using UnityEngine;

public class DropdownLevelDifficulty : MonoBehaviour
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
        for (LevelDifficulty i = LevelDifficulty.Easy; i < LevelDifficulty.MAX; i++)
        {
            if (!Enum.IsDefined(typeof(LevelDifficulty), i)) continue;
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
        var levelDifficulty = GetDifficultyOpt();
        switch (levelDifficulty)
        {
            case LevelDifficulty.Easy:
                break;
            case LevelDifficulty.Medium:
                break;
            case LevelDifficulty.Hard:
                break;
            case LevelDifficulty.SuperHard:
                break;
        }
    }

    public LevelDifficulty GetDifficultyOpt()
    {
        LevelDifficulty levelDifficulty = dropdown.options[dropdown.value].text.ToEnum<LevelDifficulty>();
        return levelDifficulty;
    }

    public void SetDifficultyOpt(LevelDifficulty levelDifficulty)
    {
        dropdown.value = (int)levelDifficulty;
    }
}
