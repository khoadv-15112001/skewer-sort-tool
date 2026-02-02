
using System.Collections.Generic;
using DG.Tweening;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.QuestEvent
{
    public class UIQuestContainer : MonoBehaviour
    {
        public PopupQuestEvent popupMain;
        [SerializeField] private Transform container;
        [SerializeField] private ScrollRect scrollView;
        [SerializeField] private UITimeCounter timeCounter;

        private readonly Service<QuestEventService> questEventService = new();
        private readonly Service<PoolingContainerService> poolingService = new();
        private List<UIQuest> _uiQuests = new();
        private static bool playTween = true;
        private static int currentQuestIdx = -1;

        private void OnEnable()
        {
            SpawnUIQuest();

            var data = questEventService.Instance.Data;
            ScrollToIndex(data.currentQuestIdx);

            UpdateUI();
            questEventService.Instance.OnDataChanged += UpdateUI;
        }

        private void OnDestroy()
        {
            questEventService.Instance.OnDataChanged -= UpdateUI;
        }

        private void SpawnUIQuest()
        {
            poolingService.Instance.CleanContainer(container);
            _uiQuests.Clear();

            var config = questEventService.Instance.config;
            for (int i = config.questDatas.Count - 1; i >= 0; i--)
            {
                var uiQuest = poolingService.Instance.CreateObject<UIQuest>(container);
                uiQuest.Init(i, config.questDatas[i].GetRewardData());

                _uiQuests.Add(uiQuest);
            }
        }

        public void ScrollToIndex(int index)
        {
            // Dừng tween cũ nếu có
            DOTween.Kill(scrollView);
            var percentage = Mathf.Max(1.0f / (_uiQuests.Count - 2) * (index - 1), 0);
            // Tween vị trí scroll mượt mà
            DOTween.To(
                () => scrollView.verticalNormalizedPosition,
                value => scrollView.verticalNormalizedPosition = value,
                percentage,
                0.3f
            ).SetEase(Ease.OutCubic).SetTarget(scrollView);
        }

        public void UpdateUI()
        {
            var data = questEventService.Instance.Data;
            for (int i = _uiQuests.Count - 1; i >= 0; i--)
            {
                var uiQuest = _uiQuests[i];
                if (data.currentQuestIdx == uiQuest.Index)
                {
                    if (data.canClaim)
                    {
                        uiQuest.SetQuestState(UIQuest.QuestState.CanClaim, false);

                        // widget quest event --> check nhận reward thì mở popup
                        // mở popup có thể nhận quà thì nhận luôn
                        SonatUtils.DelayCall(1f, ()=>{
                            uiQuest.OnClick();
                        }, this);
                    }
                    else
                    {
                        uiQuest.SetQuestState(UIQuest.QuestState.InProgress, playTween);
                    }

                    var idx = _uiQuests.Count - data.currentQuestIdx;
                    if (idx >= 0 && idx < _uiQuests.Count)
                    {
                        var preUIQuest = _uiQuests[idx];
                        if (playTween)
                        {
                            preUIQuest.uiElementProgress.PlayTween(0.5f, 1f, 0.2f);
                        }
                        else
                        {
                            preUIQuest.uiElementProgress.SetValue(1f);
                        }
                    }
                }
                else if (data.currentQuestIdx > uiQuest.Index)
                {
                    uiQuest.SetQuestState(UIQuest.QuestState.Completed);
                }
                else
                {
                    uiQuest.SetQuestState(UIQuest.QuestState.NotStarted);
                }
            }

            playTween = false;
            if (currentQuestIdx != data.currentQuestIdx)
            {
                playTween = true;
                currentQuestIdx = data.currentQuestIdx;
            }

            var remainTime = questEventService.Instance.GetRemainTime();
            timeCounter.SetData(remainTime, null);

            if (remainTime <= 0)
            {
                popupMain.SetFinished();
            }    
        }
    }
}

