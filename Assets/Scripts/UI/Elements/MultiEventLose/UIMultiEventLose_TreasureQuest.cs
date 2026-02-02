using GrillSort.LavaQuest;
using SonatFramework.Systems.UserData;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMultiEventLose_TreasureQuest : UIMultiEventLoseBase
{
    [SerializeField] private UIAvatarLavaQuest uiAvatar;

    [SerializeField] private Image tagImg;
    [SerializeField] private TMP_Text valueTxt;

    [SerializeField] private Sprite tagEasy;
    [SerializeField] private Sprite tagMedium;
    [SerializeField] private Sprite tagHard;

    public override bool Setup()
    {
        if (!LavaQuestService.Instance.isJoined.BoolValue) return false;

        uiAvatar.Init(LavaQuestService.Instance.GetXPlayersLive(1)[0]);

        tagImg.sprite = GetTagSprite();

        valueTxt.text = $"{LavaQuestService.Instance.step.Value}/{LavaQuestService.Instance.Config.maxStep}";

        return true;
    }

    private Sprite GetTagSprite()
    {
        var level = MySonatFramework.GetService<UserDataService>().GetLevel();
        var levelDifficulty = MySonatFramework.GetLevelDifficulty(level);

        switch (levelDifficulty)
        {
            case Sonat.Enums.LevelDifficulty.Easy: return tagEasy;
            case Sonat.Enums.LevelDifficulty.Hard: return tagMedium;
            case Sonat.Enums.LevelDifficulty.SuperHard: return tagHard;
            default: return tagEasy;
        }
    }

}
