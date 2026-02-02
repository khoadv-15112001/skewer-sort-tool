using System;
using I2.Loc;
using Sirenix.OdinInspector;
using Sonat.Enums;
using SonatFramework.Systems.UserData;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class ChangeTMPMaterial : MonoBehaviour
{
    public TMPMaterialSO materialSO;
    public bool useFillColor;
    private TMP_Text text;
    private Localize localize;
    private void Awake()
    {
        text = GetComponent<TMP_Text>();
        localize = GetComponent<Localize>();
    }

    private void OnEnable()
    {
        var level = MySonatFramework.GetService<UserDataService>().GetLevel();
        var difficulty = MySonatFramework.GetLevelDifficulty(level);
        SetMaterial(difficulty);
    }

    public void SetMaterial(LevelDifficulty difficulty)
    {
        if (useFillColor)
        {
            // Chỉ thay đổi màu text
            switch (difficulty)
            {
                case LevelDifficulty.Easy:
                    text.color = materialSO.normal;
                    break;
                case LevelDifficulty.Hard:
                    text.color = materialSO.hard;
                    break;
                case LevelDifficulty.SuperHard:
                    text.color = materialSO.veryHard;
                    break;
                case LevelDifficulty.NightmarishlyHard:
                    text.color = materialSO.normal;
                    break;
            }
        }
        else
        {
            // Logic cũ - thay đổi material
            switch (difficulty)
            {
                case LevelDifficulty.Easy:
                    text.fontMaterial = materialSO.materialNormal;
                    if (localize != null)
                    {
                        localize.SecondaryTerm = materialSO.materialNormalTerm;
                        localize.OnLocalize(true);
                    }
                    break;
                case LevelDifficulty.Hard:
                    text.fontMaterial = materialSO.materialHard;
                    if (localize != null)
                    {
                        localize.SecondaryTerm = materialSO.materialHardTerm;
                        localize.OnLocalize(true);
                    }
                    break;
                case LevelDifficulty.SuperHard:
                    text.fontMaterial = materialSO.materialSuperHard;
                    if (localize != null)
                    {
                        localize.SecondaryTerm = materialSO.materialSuperHardTerm;
                        localize.OnLocalize(true);
                    }
                    break;

                default:
                    text.fontMaterial = materialSO.materialNormal;
                    if (localize != null)
                    {
                        localize.SecondaryTerm = materialSO.materialNormalTerm;
                        localize.OnLocalize(true);
                    }
                    break;
            }
        }
    }
    public LevelDifficulty levelDifficultyTest;
    [Button("Test")]
    public void Test()
    {
        SetMaterial(levelDifficultyTest);
    }
}

public static class TMP_TextExtensions
{
    public static void SetMaterial(this TMP_Text text, LevelDifficulty difficulty)
    {
        if (text.transform.TryGetComponent<ChangeTMPMaterial>(out var changeTMPMaterial))
        {
            changeTMPMaterial.SetMaterial(difficulty);
        }
    }
}