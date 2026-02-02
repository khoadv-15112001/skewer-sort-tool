using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using SonatFramework.Scripts.UIModule;
using GrillSort.Services;
using DG.Tweening;
using GrillSort.RealTime;

namespace GrillSort.UI
{
    public class PopupVideoBar : Panel
    {
        [Header("UI References")]
        [SerializeField] private Slider progressBar;
        [SerializeField] private Button watchVideoButton;
        [SerializeField] private Button cooldownButton;
        [SerializeField] private TMP_Text cooldownText;
        [SerializeField] private VideoBarUIMilestone[] milestones;
        [SerializeField] private PreviewUIController previewUIController;
        private VideoBarService _videoBarService;
        private RealTimeService _realTimeService;

        private void Awake()
        {
            _videoBarService = MySonatFramework.GetService<VideoBarService>();
            _realTimeService = MySonatFramework.GetService<RealTimeService>();
        }

        public override void OnSetup()
        {
            base.OnSetup();

            progressBar.maxValue = 1;

            if (watchVideoButton != null)
                watchVideoButton.onClick.AddListener(OnWatchVideoClicked);

            if (cooldownButton != null)
                cooldownButton.onClick.AddListener(OnCooldownClicked);
            if (milestones != null)
            {
                int i = 0;
                foreach (var item in milestones)
                {
                    item.SetupUI(_videoBarService.Config.datas[i]);
                    i++;
                }
            }
            //event
            //_videoBarService.OnProgressChanged += RefreshUI;
            _videoBarService.OnCooldownChanged += UpdateCooldownText;
        }
        public override void Open(UIData uiData)
        {
            base.Open(uiData);
            RefreshUI();
        }
        public override void OnOpenCompleted()
        {
            watchVideoButton.interactable = true;

            base.OnOpenCompleted();
        }

        private void OnDestroy()
        {
            if (_videoBarService != null)
            {
                //_videoBarService.OnProgressChanged -= RefreshUI;
                _videoBarService.OnCooldownChanged -= UpdateCooldownText;
            }
        }

        private async void OnWatchVideoClicked()
        {
            watchVideoButton.interactable = false;

            bool success = await _videoBarService.WatchVideoAsync();

            watchVideoButton.interactable = true;
            if (success)
            {
                AnimateProgressFill().Forget();
            }
        }

        private void OnCooldownClicked()
        {
            PopupToast.Cretate("Comeback after", $" {cooldownText.text}");
            Debug.Log("[PopupVideoBar] Button cooldown clicked (feature locked until reset).");
        }

        private async UniTaskVoid AnimateProgressFill()
        {
            var target = (float)_videoBarService.CurrentCount / _videoBarService.MaxCount;

            await progressBar.DOValue(target, 0.4f);

            RefreshTickStates();

            RefreshUI();
        }

        private void RefreshUI()
        {
            if (_videoBarService == null || !_videoBarService.CanShow())
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            var current = _videoBarService.CurrentCount;
            var max = _videoBarService.MaxCount;

            progressBar.value = (float)current / max;

            RefreshTickStates();

            bool hasMore = current < max;
            watchVideoButton.interactable = hasMore;
            watchVideoButton.gameObject.SetActive(hasMore);
            cooldownButton.gameObject.SetActive(!hasMore);
            cooldownText.gameObject.SetActive(!hasMore);

            if (!hasMore)
                UpdateCooldownText();
        }

        private void RefreshTickStates()
        {
            var config = _videoBarService._config;
            var current = _videoBarService.CurrentCount;

            for (int i = 0; i < milestones.Length; i++)
            {
                bool reached = i < config.datas.Count && current >= config.datas[i].watchCount;
                milestones[i].Completed(reached);
            }
        }

        private void UpdateCooldownText()
        {
            long nextTime = _videoBarService.NextResetTime;
            long now = _realTimeService.GetCurrentTimeUnix();
            long remain = nextTime - now;

            if (remain <= 0)
            {
                cooldownText.text = "Ready!";
                this.DelayRefresh().Forget();
                return;
            }

            TimeSpan span = TimeSpan.FromSeconds(remain);
            cooldownText.text = $"{span.Hours:D2}:{span.Minutes:D2}:{span.Seconds:D2}";
        }

        private async UniTaskVoid DelayRefresh()
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            if (this != null && gameObject != null && gameObject.activeInHierarchy)
                RefreshUI();
        }

        //private float CalculateProgress()
        //{
        //    if (_videoBarService.CurrentCount == 0)
        //        return 0;
        //    var config = _videoBarService._config;
        //    var current = _videoBarService.CurrentCount;

        //    if (config.datas.Count == 0) return 0f;

        //    float fill = 0f;
        //    for (int i = 0; i < config.datas.Count; i++)
        //    {
        //        int target = config.datas[i].watchCount;
        //        if (current >= target)
        //            fill = (i + 1) / (float)config.datas.Count;
        //        else if (i == 0)
        //            fill = Mathf.Clamp01((float)current / target / config.datas.Count);
        //        else
        //        {
        //            int prev = config.datas[i - 1].watchCount;
        //            float segProgress = Mathf.InverseLerp(prev, target, current);
        //            fill = (i / (float)config.datas.Count) + segProgress / config.datas.Count;
        //            break;
        //        }
        //    }

        //    return Mathf.Clamp01(fill);
        //}

        public override void Close()
        {
            base.Close();
        }

        public void MilestoneClick(int id)
        {
            if (id < 0 || id >= _videoBarService.Config.datas.Count)
            {
                Debug.Log($"[PopupVideoBar] id={id} > datas count={_videoBarService.Config.datas.Count}");

                return;
            }
            var rewards = _videoBarService.Config.datas[id].rewards;
            if (rewards != null && rewards.resourceDatas.Count > 0)
            {
                previewUIController.ShowPreview(rewards);
            }
            else
            {
                Debug.Log("[PopupVideoBar] rewards = null or empty!");
            }
        }
    }
}
