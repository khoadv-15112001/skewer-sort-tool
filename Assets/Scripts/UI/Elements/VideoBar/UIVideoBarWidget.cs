using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using GrillSort.Services;
using SonatFramework.Systems;
using GrillSort.RealTime;
using GrillSort.UI;
using SonatFramework.Scripts.UIModule;
using GrillSort.LavaQuest;
using SonatFramework.Systems.AudioManagement;
using SonatFramework.Systems.UserData;

public class UIVideoBarWidget : UIHomeWidget
{
    [Header("UI Elements")]
    [SerializeField] private Button openButton;
    [SerializeField] private TMP_Text progressText;     // Hiển thị dạng "x / y"
    [SerializeField] private GameObject notiDot;        // Chấm đỏ thông báo còn lượt xem

    private VideoBarService _videoBarService;
    private RealTimeService _realTimeService;

    private void Awake()
    {
        _videoBarService = MySonatFramework.GetService<VideoBarService>();
        _realTimeService = MySonatFramework.GetService<RealTimeService>();
    }

    public override void Setup()
    {
        base.Setup();

        if (openButton != null)
            openButton.onClick.AddListener(OnClickOpen);

        // Đăng ký sự kiện service
        if (_videoBarService != null)
        {
            _videoBarService.OnProgressChanged += RefreshUI;
            _videoBarService.OnCooldownChanged += RefreshUI;
        }

        RefreshUI();
    }

    private void OnDestroy()
    {
        if (_videoBarService != null)
        {
            _videoBarService.OnProgressChanged -= RefreshUI;
            _videoBarService.OnCooldownChanged -= RefreshUI;
        }

        if (openButton != null)
            openButton.onClick.RemoveListener(OnClickOpen);
    }

    private void RefreshUI()
    {
        if (_videoBarService == null || !_videoBarService.CanShow() || !_videoBarService.Config.showWidget)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        int current = _videoBarService.CurrentCount;
        int max = _videoBarService.MaxCount;
        bool hasMore = current < max;

        if (progressText != null)
            progressText.text = $"{current}/{max}";

        if (notiDot != null)
            notiDot.SetActive(hasMore);
    }

    private void OnClickOpen()
    {
        if (_videoBarService == null || !_videoBarService.CanShow())
            return;

        PanelManager.Instance.OpenPanel<PopupVideoBar>();
    }

    protected override bool CheckActive()
    {
        if (_videoBarService == null) return false;
        return _videoBarService.CanShow();
    }

    public override async UniTask<bool> ProcessTask()
    {
        if (_videoBarService == null || !_videoBarService.CanShow())
            return false;

        if (!_videoBarService.TutShown && _videoBarService.Config.levelStart == MySonatFramework.userDataService.GetLevel())
        {
            _videoBarService.TutShown = true;

            MySonatFramework.GetService<AudioService>().PlaySound(Sonat.Enums.AudioId.ButtonClick);
            var popup = PanelManager.Instance.OpenPanel<PopupVideoBar>();

            await UniTask.WaitUntil(() => !popup.gameObject.activeSelf);

            await UniTask.WaitForSeconds(0.75f);

            return true;
        }
        return false;
    }
}
