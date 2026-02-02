using System;
using DG.Tweening;
using Gameplay.LevelData;
using I2.Loc;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.SceneManagement;
using SonatFramework.Systems.UserData;
using UnityEngine;
using Sirenix.OdinInspector;
using GrillSort.ConsecutiveWin;
using SonatFramework.Systems.EventBus;
using GrillSort.Pinata;

public class UIButtonPlay : MonoBehaviour
{
    [SerializeField] private bool isHome = true;
    [SerializeField, ShowIf("isHome")] private GameObject[] pLevelDifficulty;
    [SerializeField, ShowIf("isHome")] private LocalizationParamsManager[] listTxtLevelPlay;

    private GameObject uiButtonPlay;

    private void OnEnable()
    {
        if (isHome)
        {
            var level = MySonatFramework.GetService<UserDataService>().GetLevel();
            SetButtonPlay(level);

            PinataService.OnActive += OnUpdatePinata;
            PinataService.OnInactive += OnUpdatePinata;
            PinataService.OnCompleteStage += OnUpdatePinata;
        }
    }

    private void OnDisable()
    {
        if (isHome)
        {
            PinataService.OnActive -= OnUpdatePinata;
            PinataService.OnInactive -= OnUpdatePinata;
            PinataService.OnCompleteStage -= OnUpdatePinata;
        }
    }

    public void SetButtonPlay(int level)
    {
        SetTextLevel(level);

        var levelDifficulty = MySonatFramework.GetLevelDifficulty(level);
        foreach (var p in pLevelDifficulty)
        {
            p.SetActive(false);
        }

        uiButtonPlay = pLevelDifficulty[(int)levelDifficulty];

        uiButtonPlay.SetActive(true);

        OnUpdatePinata();
    }

    private void OnUpdatePinata()
    {
        var rect = uiButtonPlay.GetComponent<RectTransform>();

        if (PinataService.Instance != null &&
            PinataService.Instance.IsActive.BoolValue &&
            PinataService.Instance.CurrentStage.Value == PinataService.Instance.Config.MaxStage)
        {
            rect.sizeDelta = new Vector2(570f, rect.sizeDelta.y);
        }
        else
        {
            rect.sizeDelta = new Vector2(525f, rect.sizeDelta.y);
        }
    }

    private void SetTextLevel(int level)
    {
        foreach (var txt in listTxtLevelPlay)
        {
            txt.SetParameterValue("LEVEL", level.ToString());
        }
    }

    private void CheckLives()
    {
        if (MySonatFramework.livesService.CanPlay())
        {
            var consecutiveWinService = MySonatFramework.GetService<ConsecutiveWinService>();
            var level = MySonatFramework.GetService<UserDataService>().GetLevel();
            if (consecutiveWinService.CheckStart(level))
            {
                UIData uiData = new UIData();
                uiData.Add("OnPlay", (Action)(() => SonatUtils.DelayCall(0.1f, PlayGame)));
                PanelManager.Instance.OpenPanel<PopupPrePlay>(uiData);
            }
            else
            {
                PlayGame();
            }
        }
        else
        {
            PanelManager.Instance.OpenPanel<PopupRefillLives>();
            PopupToast.Cretate("No more lives left!");
        }
    }

    private void PlayGame()
    {
        LoadingScreenInstance.Instance.Show(3.5f);
        SonatUtils.DelayCall(0.5f, () => { MySonatFramework.GetService<SceneService>().SwitchScene(GamePlacement.Gameplay); });
    }

    public void PlayClick()
    {
        CheckLives();
    }
}