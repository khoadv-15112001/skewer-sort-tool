using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gameplay.Entities;
using Gameplay.LevelData;
using GrillSort.QuestEvent;
using PrototypeTest;
using Sonat.Enums;
using SonatFramework.Scripts.Helper;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.EventBus;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayScreen : MonoBehaviour
{
    [SerializeField] private GameplayController gameplayController;
    [SerializeField] private TextMeshProUGUI txtLevel;
    [SerializeField] private TextMeshProUGUI txtTime;
    [SerializeField] private TextMeshProUGUI txtMove;
    [SerializeField] private TextMeshProUGUI txtProgress;
    [SerializeField] private Slider progressBar;
    [SerializeField] private GameObject iceObj;
    [SerializeField] private Image warningTime;
    [SerializeField] private UIQuestItemInGame uiQuestItemInGame;
    [SerializeField] private UIBooster[] uiBooster;
    [SerializeField] private Image cmBgr;
    [SerializeField] private Sprite[] cmsSprites;
    [SerializeField] private Image bgrImage;
    [SerializeField] private Sprite[] bgrSprites;
    [SerializeField] private Transform queuePos;
    [SerializeField] private Transform progressBoard;
    [SerializeField] private Transform targetBoard;
    [SerializeField] private Transform comboTextContainer;

    private int maxOrder;
    private int currentOrder;
    private bool isWarning;
    private bool suggestFreeze;

    private EventBinding<LevelStartedEvent> onStartLevel;
    private EventBinding<AddTimeBonusEvent> onAddTimeBonus;
    public Transform ComboTextContainer { get => comboTextContainer; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCheckSuggestMagnet();
        MoveUIToQueuePos();
    }

    private void MoveUIToQueuePos()
    {
        HideAllBoards();

        switch (gameplayController.levelGenerator.LevelMode)
        {
            case LevelMode.Target:
                targetBoard.gameObject.SetActive(true);
                break;
            case LevelMode.All:
                progressBoard.gameObject.SetActive(true);
                break;
        }

        Vector3 pos = progressBoard.position;
        pos.y = queuePos.position.y;
        progressBoard.position = pos;
        pos = targetBoard.position;
        pos.y = queuePos.position.y;
        targetBoard.position = pos;
    }

    private void HideAllBoards()
    {
        progressBoard.gameObject.SetActive(false);
        targetBoard.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        GameplayController.OnLoadLevel += OnLevelLoad;
        GameplayController.OnCollectItem += OnCollectItem;
        gameplayController.timeManager.OnFreeze += SetFreezeState;
        gameplayController.timeManager.OnTimeUpdate += UpdateTime;
        gameplayController.moveManager.OnMoveUpdate += UpdateMove;

        onAddTimeBonus = new EventBinding<AddTimeBonusEvent>(OnAddTimeBonus);

        onStartLevel = new EventBinding<LevelStartedEvent>(OnStartLevel);
    }

    private void OnDisable()
    {
        GameplayController.OnLoadLevel -= OnLevelLoad;
        GameplayController.OnCollectItem -= OnCollectItem;
        gameplayController.timeManager.OnFreeze -= SetFreezeState;
        gameplayController.timeManager.OnTimeUpdate -= UpdateTime;
        gameplayController.moveManager.OnMoveUpdate -= UpdateMove;

        EventBus<LevelStartedEvent>.Deregister(onStartLevel);
        EventBus<AddTimeBonusEvent>.Deregister(onAddTimeBonus);
    }

    private void OnLevelLoad(int level)
    {
        LevelData levelData = gameplayController.levelGenerator.LevelData;
        LevelDifficulty levelDifficulty = MySonatFramework.GetLevelDifficulty(level);

        txtLevel.text = $"{level}";
        txtLevel.SetMaterial(levelDifficulty);
        txtTime.SetMaterial(levelDifficulty);
        txtMove.SetMaterial(levelDifficulty);

        maxOrder = gameplayController.levelGenerator.MaxOrder;
        currentOrder = 0;
        //UpdateProgress(0);

        // Check level mode để hiển thị time hoặc move
        // if (levelData.levelMode == LevelMode.Target)
        // {
        //     txtTime.transform.gameObject.SetActive(false);
        //     txtMove.transform.gameObject.SetActive(true);
        //     UpdateMove(levelData.move);
        // }
        // else
        // {
            txtTime.transform.gameObject.SetActive(true);
            txtMove.transform.gameObject.SetActive(false);
            UpdateTime(levelData.time);
       // }

        if (cmsSprites?.Length > (int)levelDifficulty)
            cmBgr.sprite = cmsSprites[(int)levelDifficulty];

        var questEventService = SonatFramework.Systems.SonatSystem.GetService<QuestEventService>();
        if (questEventService.config.Active == false) return;
        var config = questEventService.config;
        if (questEventService.IsUnlocked() && questEventService.CheckCompleteAllQuest() == false)
        {
            uiQuestItemInGame.gameObject.SetActive(true);
        }
        else
        {
            uiQuestItemInGame.gameObject.SetActive(false);
        }

        bgrImage.sprite = levelData.levelType is LevelType.Food ? bgrSprites[0] : bgrSprites[1];
        UpdateProgress(0);

        // kiểm tra xem có thể hiện booster không
        foreach (var booster in uiBooster)
        {
            booster.gameObject.SetActive(booster.IsForcedToAppear || gameplayController.CheckAppearBooster(booster.boosterType));
        }

        checkMatch = level == 6;

    }


    private void OnStartLevel(LevelStartedEvent eventData)
    {
        // LevelDifficulty levelDifficulty = MySonatFramework.GetLevelDifficulty(eventData.level);
        // if (levelDifficulty != LevelDifficulty.Normal)
        // {
        //     UIFlowController.isShowedPopupWarningLevel = true;
        //     SonatUtils.DelayCall(0.2f, () =>
        //     {
        //         var data = new UIData().Add("levelDifficulty", levelDifficulty);
        //         data.Add("Target", cmBgr.transform);
        //         PanelManager.Instance.OpenPanel<PopupWarningLevel>(data);
        //     });
        // }
    }


    public void ClearLevel()
    {
        SetFreezeState(false);
        StopWaitingTime();
        lastTimeCollectItem = Time.time;
        suggestType = 0;
        SuggestBooster(GameResource.BoosterMagnet, false);
        suggestFreeze = false;
        SuggestBooster(GameResource.BoosterFreeze, false);
    }


    public void UpdateTime(float time)
    {
        int seconds = (int)time;
        txtTime.text = SonatUtils.FormatTimeFromSec(seconds);
        if (!isWarning)
        {
            if (seconds == 30 || seconds == 15 || seconds == 10)
            {
                WarningTimeOneShot();
                if (seconds <= 10)
                {
                    StartWarningTime();
                }

                if (!suggestFreeze)
                {
                    suggestFreeze = true;
                    SuggestBooster(GameResource.BoosterFreeze, true);
                }
            }
        }
        else if (seconds > 30)
        {
            StopWaitingTime();
            if (suggestFreeze)
            {
                suggestFreeze = false;
                SuggestBooster(GameResource.BoosterFreeze, false);
            }
        }
    }

    public void UpdateMove(int moves)
    {
        txtMove.text = moves.ToString();
    }

    private void OnCollectItem(int id)
    {
        currentOrder++;
        UpdateProgress(currentOrder);

        lastTimeCollectItem = Time.time;
    }

    private void UpdateProgress(int progress)
    {
        txtProgress.text = $"{progress}/{gameplayController.levelGenerator.MaxOrder}";
        //progressBar.DOValue(progress * 1.0f / maxOrder, 0.15f);
    }

    public void SetFreezeState(bool state)
    {
        if (state)
            SonatUtils.DelayCall(2, () => { iceObj.SetActive(true); }, this);
        else
        {
            iceObj.SetActive(false);
        }

        if (state && suggestFreeze)
        {
            suggestFreeze = false;
            SuggestBooster(GameResource.BoosterFreeze, false);
        }
    }

    public void StartWarningTime()
    {
        isWarning = true;
        warningTime.DOKill();
        warningTime.gameObject.SetActive(true);

        warningTime.SetAlpha(1);
        warningTime.DOFade(0.3f, 0.75f).SetLoops(-1, LoopType.Yoyo);

        PlayLoopSound().Forget();
    }

    private async UniTask PlayLoopSound()
    {
        int time = 10;
        while (time >= 0)
        {
            MySonatFramework.audioService.PlaySound(AudioId.Time_Count);
            await UniTask.Delay(1000);
            time--;
        }

        warningTime.DOKill();
        warningTime.gameObject.SetActive(false);
    }

    public void WarningTimeOneShot()
    {
        isWarning = true;
        warningTime.gameObject.SetActive(true);
        warningTime.SetAlpha(0);
        MySonatFramework.audioService.PlaySound(AudioId.Time_Warning);
        MySonatFramework.audioService.PlaySound(AudioId.Time_Count);
        warningTime.DOFade(1, 0.75f).SetLoops(2, LoopType.Yoyo).OnComplete(() =>
        {
            warningTime.gameObject.SetActive(false);
            isWarning = false;
        });
    }

    private void StopWaitingTime()
    {
        if (!isWarning) return;
        isWarning = false;
        warningTime.DOKill();
        warningTime.gameObject.SetActive(false);
        //uiWarningTime.StopWarning();
    }

    private void StartCheckSuggestMagnet()
    {
        lastTimeCollectItem = Time.time;
        StartCoroutine(WaitSuggestMagnet());
    }

    public void ForceCheckSuggest()
    {
        if (suggestType == 2) return;

        if (suggestType == 3)
        {
            suggestType = 0;
            SuggestBooster(GameResource.BoosterMagnet, false);
        }

        if (lastTimeCollectItem > Time.time - 10)
            lastTimeCollectItem = Time.time - 15;
    }

    private float lastTimeCollectItem;
    private int suggestType = 0; // 0: không suggest, 1: suggest items, 2: suggest lock ads, 3: suggest magnet
    private float timeStartCheckSuggest = 0;
    private List<Item> suggestItems;
    private bool checkMatch;

    IEnumerator WaitSuggestMagnet()
    {
        while (true)
        {
            if (gameplayController.gameState == GameState.Playing)
            {
                if (checkMatch)
                {
                    if (suggestType == 2)
                    {
                        if (gameplayController.levelGenerator.CheckHasMatch() || Time.time - lastTimeSuggestLockAds > 3)
                        {
                            suggestType = 0;
                            PanelManager.Instance.ClosePanel<PopupSuggest>();
                        }
                        else
                        {
                            yield return null;
                            continue;
                        }
                    }
                }

                if (Time.time - lastTimeCollectItem > 10)
                {
                    if (suggestType == 0)
                    {
                        if (!SuggestItems())
                        {
                            // chờ 2s rồi mới check xem có cái nào lock không
                            yield return new WaitForSeconds(1f);

                            // nếu out of move thì suggest lock ads trước
                            if (gameplayController.levelGenerator.CheckOutOfMove() && SuggestLockAds() == true)
                            {
                            }
                            else
                            {
                                SuggestBooster(GameResource.BoosterMagnet, true);
                            }
                        }

                        timeStartCheckSuggest = Time.time;
                    }
                    else if (suggestType == 1)
                    {
                        // check mỗi 5s
                        if (Time.time - timeStartCheckSuggest > 5)
                        {
                            // Nếu đang suggest mà bị out of move thì suggest magnet hoặc lock ads
                            if (gameplayController.levelGenerator.CheckOutOfMove())
                            {
                                ClearSuggestItems();
                                if (SuggestLockAds() == false)
                                {
                                    SuggestBooster(GameResource.BoosterMagnet, true);
                                }
                            }

                            timeStartCheckSuggest = Time.time;
                        }
                    }
                }
                else
                {
                    if (suggestType != 0)
                    {
                        suggestType = 0;
                        SuggestBooster(GameResource.BoosterMagnet, false);
                        ClearSuggestItems();
                        PanelManager.Instance.ClosePanel<PopupSuggest>();
                    }
                }
            }
            else if (suggestType == 2)
            {
                // tắt suggest khi mở popup khác <=> pause
                suggestType = 0;
                PanelManager.Instance.ClosePanel<PopupSuggest>();
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    private float lastTimeSuggestLockAds;

    public bool SuggestLockAds()
    {
        if (suggestType == 2 || Time.time - lastTimeSuggestLockAds < 20) return true;
        var lockAds = gameplayController.levelGenerator.GetLockAds();
        if (lockAds != null && lockAds.IsLock)
        {
            suggestType = 2;
            lastTimeSuggestLockAds = Time.time;
            var uiData = new UIData().Add("targetPosition", lockAds.transform.position);
            PanelManager.Instance.OpenPanel<PopupSuggest>(uiData);
            return true;
        }

        return false;
    }

    private bool SuggestItems()
    {
        suggestItems = gameplayController.levelGenerator.GetSuggestItems();
        if (suggestItems == null) return false;

        suggestType = 1;
        foreach (var item in suggestItems)
        {
            item.SetSuggest(true);
        }

        return true;
    }

    private void ClearSuggestItems()
    {
        if (suggestItems == null) return;
        foreach (var item in suggestItems)
        {
            item.SetSuggest(false);
        }

        suggestItems = null;
    }

    public void SuggestBooster(GameResource booster, bool state)
    {
        Debug.Log($"SuggestBooster: {booster} - {state}");
        switch (booster)
        {
            case GameResource.BoosterMagnet:
                uiBooster[0].Suggest(state);
                if (state == true) suggestType = 3;
                break;
            case GameResource.BoosterFreeze:
                uiBooster[1].Suggest(state);
                break;
            case GameResource.BoosterShuffle:
                uiBooster[2].Suggest(state);
                break;
        }
    }

    private void OnAddTimeBonus(AddTimeBonusEvent eventData)
    {
        var effect = MySonatFramework.poolingService.Create<EffectPoolBase>("BonusTimeEffect", PanelManager.Instance.transform);
        effect.transform.position = eventData.position;
        effect.transform.DOJump(txtTime.transform.position, 1, 1, 1.2f).SetEase(Ease.InOutSine).SetDelay(0.5f).OnComplete(() =>
        {
            AddTimeBonus();
            MySonatFramework.poolingService.ReturnObj(effect);
        });
    }

    private void AddTimeBonus()
    {
        GameplayController.instance.timeManager.AddTime(5);
    }
}