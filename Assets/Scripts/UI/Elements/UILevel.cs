using System;
using Gameplay.LevelData;
using I2.Loc;
using Sonat.Enums;
using SonatFramework.Systems.UserData;
using UnityEngine;

public class UILevel : MonoBehaviour
{
    [Serializable]
    public class ObjectAndDifficultyLevel
    {
        public GameObject obj;
        public LevelDifficulty difficulty;
    }

    [SerializeField] private ObjectAndDifficultyLevel[] objDifficultyLevel;
    [SerializeField] private LocalizationParamsManager[] txtLevel;

    private void OnEnable()
    {
        var level = MySonatFramework.GetService<UserDataService>().GetLevel();
        var levelDifficulty = MySonatFramework.GetLevelDifficulty(level);
        SetObjectDifficultyLevel(levelDifficulty);

        SetTextLevel(level);
    }

    private void SetObjectDifficultyLevel(LevelDifficulty levelDifficulty)
    {
        foreach (var p in objDifficultyLevel)
        {
            p.obj.SetActive(p.difficulty == levelDifficulty);
        }
    }

    private void SetTextLevel(int level)
    {
        for (int i = 0; i < txtLevel.Length; i++)
        {
            txtLevel[i].SetParameterValue("LEVEL", level.ToString());
        }
    }
}
