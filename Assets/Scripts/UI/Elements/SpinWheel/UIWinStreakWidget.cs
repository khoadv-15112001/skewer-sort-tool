using Cysharp.Threading.Tasks;
using DG.Tweening;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace GrillSort.Winstreak
{
    public class UIWinStreakWidget : UIHomeWidget
    {
        [SerializeField] private UITimeCounter timeCounter;
        [SerializeField] private TMP_Text textValue;
        [SerializeField] private GameObject notiObj, finishedObj;
        [SerializeField] private Image icon;
        [SerializeField] private Slider slider;
        //private bool showPopup = false;

        private readonly Service<WinStreakManager> _winstreakManager = new();

        private void Start()
        {
            _winstreakManager.Instance.OnChangeData += UpdateUI;
        }
        private void OnDestroy()
        {
            _winstreakManager.Instance.OnChangeData -= UpdateUI;
        }
        public override void Setup()
        {
            base.Setup();
            active = active && _winstreakManager.Instance.EventActive.BoolValue;
            gameObject.SetActive(active);
            //Debug.Log($"anhnt:{active} {_winstreakManager.Instance.CanUnlock()}");
            if (!active) return;

            icon.sprite = _winstreakManager.Instance.config.iconWidget;
            //showPopup = CheckShowPopup();
            timeCounter.SetData(_winstreakManager.Instance.GetTimeToReset(), FinishEvent);

            UpdateUI();

            UpdateWinningInARow();
        }

        private void FinishEvent()
        {
            DOVirtual.DelayedCall(0.1f, () =>
            {
                _winstreakManager.Instance.CheckFinishEvent();

                gameObject.SetActive(false);
            });
        }

        private void UpdateUI()
        {
            if (!_winstreakManager.Instance.EventActive.BoolValue || _winstreakManager.Instance.CheckCompletedEvent())
            {
                gameObject.SetActive(false);

                return;
            }
            //timeCounter.SetData(_winstreakManager.Instance.GetTimeToReset(), null);
            notiObj.SetActive(_winstreakManager.Instance.CheckHasMilestoneClaimed());
        }


        // Tính tiến độ trong đoạn milestone hiện tại
        // Trả về: cur (tiến độ trong đoạn), max (độ dài đoạn), segIndex (chỉ số mốc hiện tại),
        //         prevReq (mốc trước), currReq (mốc hiện tại)
        private (int cur, int max, int segIndex, int prevReq, int currReq) GetProgress(int winLevel)
        {
            var levels = _winstreakManager.Instance.config.rewardDatas
                .Select(m => m.levelWinRequired)
                .OrderBy(x => x)
                .ToList();

            if (levels.Count == 0) return (0, 1, 0, 0, 1);

            if (winLevel <= 0)
                return (0, levels[0], 0, 0, levels[0]);

            // Nếu >= mốc cuối: kẹp ở đoạn cuối
            if (winLevel >= levels[^1])
            {
                int seg = levels.Count - 1;
                int prev = seg == 0 ? 0 : levels[seg - 1];
                int curr = levels[seg];
                int max = Mathf.Max(1, curr - prev);
                int cur = Mathf.Clamp(winLevel - prev, 0, max);
                return (cur, max, seg, prev, curr);
            }

            // Tìm đoạn: first j sao cho L[j] >= winLevel
            int i = levels.FindIndex(v => v >= winLevel);
            if (i < 0) i = levels.Count - 1;

            int prevReq = (i == 0) ? 0 : levels[i - 1];
            int currReq = levels[i];
            int maxSeg = Mathf.Max(1, currReq - prevReq);
            int curSeg = Mathf.Clamp(winLevel - prevReq, 0, maxSeg);

            return (curSeg, maxSeg, i, prevReq, currReq);
        }

        private void UpdateWinningInARow()
        {
            if (_winstreakManager.Instance.IsMax())
            {
                finishedObj.SetActive(true);
                slider.value = 1;
                return;
            }

            finishedObj.SetActive(false);

            int targetWins = 0;

            if (_winstreakManager.Instance.HasNoti())
            {
                targetWins = _winstreakManager.Instance.winningInARow.Value -  1;
                _winstreakManager.Instance.ItemCollect = 0;

                Collect(1, _winstreakManager.Instance.config.iconItem, () =>
                {
                    UpdateVisual();
                }, 1.5f);
            }
            else
            {
                targetWins = _winstreakManager.Instance.winningInARow.Value;
            }

            var (cur, max, segIndex, prevReq, currReq) = GetProgress(targetWins);

            textValue.text = $"{cur}/{max}";
            slider.value = (float)cur / max;

            //Debug.Log($"anhnt: start cur={cur} ,max={max}, segIndex={segIndex}, prevReq={prevReq},currReq={currReq}");

            if (cur == max)
            {
                var nextProg = GetProgress(targetWins + 1);
                textValue.text = $"0/{nextProg.max}";
                slider.value = 0f;
                Debug.Log($"anhnt: completed segment, reset to next max={nextProg.max}");
            }
        }

        internal void UpdateVisual()
        {
            int lastWins = _winstreakManager.Instance.winningInARow.Value - 1;

            int targetWins = _winstreakManager.Instance.winningInARow.Value;

            var lastProg = GetProgress(lastWins);

            var newProg = GetProgress(targetWins);

            if (newProg.segIndex == lastProg.segIndex && lastProg.cur != lastProg.max)
            {
                textValue.text = $"{newProg.cur}/{newProg.max}";
                slider.DOValue((float)newProg.cur / newProg.max, 0.3f)
                    .OnComplete(() =>
                    {
                        if (newProg.cur == newProg.max)
                        {
                            var nextProg = GetProgress(targetWins + 1);
                            textValue.text = $"0/{nextProg.max}";
                            slider.value = 0f;
                        }
                    });
            }
            else
            {
                textValue.text = $"{newProg.cur}/{newProg.max}";
                slider.DOValue((float)newProg.cur / newProg.max, 0.25f);
            }
        }
        public void CheatCollect()
        {
            Collect(1, _winstreakManager.Instance.config.iconItem, () =>
            {
                textValue.text = _winstreakManager.Instance.winningInARow.Value.ToString();
                transform.DOPunchScale(Vector3.one * 0.1f, 0.4f);
            });
        }

        public override void OnFocus()
        {

        }

        public override void OnLoseFocus()
        {

        }

        // Chỉ hiện ở phiên đầu tiên trong ngày
        //public override async UniTask<bool> ProcessTask()
        //{
        //    if (showPopup)
        //    {
        //        var popup = PanelManager.Instance.OpenPanelByName<BasePanel>("PopupWinstreak");
        //        MySonatFramework.customTrackingService.OnShowPopup(gameObject.name.ToLogString(), "widget", "non_iap", "auto");
        //        await UniTask.WaitUntil(() => popup == null || !popup.gameObject.activeInHierarchy);
        //        await UniTask.Delay(750);
        //        return true;
        //    }
        //    return false;
        //}

        public void OnClick()
        {
            PanelManager.Instance.OpenForget<PopupWinstreak>();
            MySonatFramework.customTrackingService.OnShowPopup(gameObject.name.ToLogString(), "widget", "non_iap", "user");
        }
    }
}

