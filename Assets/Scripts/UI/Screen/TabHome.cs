using DG.Tweening;
using GrillSort.Story;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.UI;

public class TabHome : UITabBase
{
    [SerializeField] private HomeWidgetManager homeWidgetManager;
    private bool firstTime = true;

    [SerializeField] private RectTransform topPanel;
    [SerializeField] private RectTransform leftPanel;
    [SerializeField] private RectTransform rightPanel;
    [SerializeField] private RectTransform botPanel;
    [SerializeField] private RectTransform playBtn;

    [SerializeField] private float slideDistance = 800f;
    [SerializeField] private float slideDuration = 0.5f;
    [SerializeField] private Ease slideEase = Ease.OutCubic;

    private Vector2 topStartPos;
    private Vector2 leftStartPos;
    private Vector2 rightStartPos;
    private Vector2 botStartPos;
    private Vector2 playStartPos;

    private bool isOut = false;

    public static Action OnSlideIn;
    public static Action OnSlideOut;

    protected override void Awake()
    {
        base.Awake();

        if (topPanel) topStartPos = topPanel.anchoredPosition;
        if (leftPanel) leftStartPos = leftPanel.anchoredPosition;
        if (rightPanel) rightStartPos = rightPanel.anchoredPosition;
        if (botPanel) botStartPos = botPanel.anchoredPosition;
        if (playBtn) playStartPos = playBtn.anchoredPosition;
    }

    protected override void Start()
    {
        base.Start();

        int bgm = UnityEngine.Random.Range(0, 2); // random [0;1]

        // MySonatFramework.audioService.PlayMusic(GameplayController.GetBackgroundMusic(), true, 0.5f);
        if (LocalizationUtils.IsJapanese())
        {

            if (bgm == 0)
            {
                MySonatFramework.audioService.PlayMusic(Sonat.Enums.AudioId.BGM_Home_Japan_01_Grill_sort, true, 0.5f);
            }
            else
            {
                MySonatFramework.audioService.PlayMusic(Sonat.Enums.AudioId.BGM_Home_Japan_02_Grill_sort, true, 0.5f);
            }
        }
        else
        {
            if (bgm == 0)
                MySonatFramework.audioService.PlayMusic(Sonat.Enums.AudioId.BGM_Home_summer_Grill_sort, true, 0.5f);
            else
                MySonatFramework.audioService.PlayMusic(Sonat.Enums.AudioId.BGM_Home_summer_Grill_sort_01, true, 0.5f);
        }

        //StoryService.Instance.PlayBackgroundHomeMusic();
        homeWidgetManager.Setup();
    }

    private void OnEnable()
    {
        OnSlideIn += SlideIn;
        OnSlideOut += SlideOut;
    }

    private void OnDisable()
    {
        OnSlideIn -= SlideIn;
        OnSlideOut -= SlideOut;
    }

    public override void OnShow()
    {
        base.OnShow();
        homeWidgetManager.OnFocus();
    }

    public override void OnHide()
    {
        base.OnHide();
        homeWidgetManager.OnLoseFocus();
    }

    public override void FadeIn()
    {
        if (firstTime)
        {
            firstTime = false;
            return;
        }

        base.FadeIn();
    }

    [Button]
    public void SlideOut()
    {
        if (isOut) return;
        isOut = true;

        if (topPanel)
            topPanel.DOAnchorPosY(topStartPos.y + slideDistance, slideDuration).SetEase(slideEase);
        if (botPanel)
            botPanel.DOAnchorPosY(botStartPos.y - slideDistance, slideDuration).SetEase(slideEase);
        if (leftPanel)
            leftPanel.DOAnchorPosX(leftStartPos.x - slideDistance, slideDuration).SetEase(slideEase);
        if (rightPanel)
            rightPanel.DOAnchorPosX(rightStartPos.x + slideDistance, slideDuration).SetEase(slideEase);

        if (playBtn)
            playBtn.DOAnchorPosY(rightStartPos.y - slideDistance, slideDuration).SetEase(slideEase);
    }

    [Button]
    public void SlideIn()
    {
        if (!isOut) return;
        isOut = false;

        if (topPanel)
            topPanel.DOAnchorPos(topStartPos, slideDuration).SetEase(slideEase);
        if (botPanel)
            botPanel.DOAnchorPos(botStartPos, slideDuration).SetEase(slideEase);
        if (leftPanel)
            leftPanel.DOAnchorPos(leftStartPos, slideDuration).SetEase(slideEase);
        if (rightPanel)
            rightPanel.DOAnchorPos(rightStartPos, slideDuration).SetEase(slideEase);

        if (playBtn)
            playBtn.DOAnchorPos(playStartPos, slideDuration).SetEase(slideEase);
    }
}