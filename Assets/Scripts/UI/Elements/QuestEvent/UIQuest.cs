using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.InventoryManagement.GameResources;
using TMPro;
using UnityEngine;

namespace GrillSort.QuestEvent
{
    public class UIQuest : MonoBehaviour
    {
        [SerializeField] private TMP_Text textQuest;
        [SerializeField] private UIRewardGrid _rewardGrid;

        [Header("State")]
        [SerializeField] private GameObject objCompleted;
        [SerializeField] private GameObject objInProgress;
        [SerializeField] private GameObject objNotStarted;
        [SerializeField] private GameObject objCanClaim;

        [Header("Progress")]
        [SerializeField] public UIElementProgress uiElementProgress;
        [SerializeField] private GameObject objNormalPoint;
        [SerializeField] private GameObject objCurrentPoint;
        [SerializeField] private GameObject objClaimedPoint;

        private readonly Service<InventoryService> inventoryService = new();


        public int Index { get; private set; }

        public void Init(int index, RewardData reward)
        {
            Index = index;
            textQuest.text = $"{index + 1}";
            
            _rewardGrid.SetReward(reward);
            _rewardGrid.transform.localScale = Vector3.one * (reward.resourceDatas.Count > 2 ? (reward.resourceDatas.Count > 3? 0.65f : 0.8f) : 1);

            var questEventService = SonatSystem.GetService<QuestEventService>();
            if (index == 0)
            {
                uiElementProgress.HideProgressBefore();
            }
            else if (index == questEventService.config.GetQuestCount() - 1)
            {
                uiElementProgress.HideProgressAfter();
            }
        }

        public void SetQuestState(QuestState state, bool playTween = true)
        {
            // Debug.Log($"[UIQuest] SetQuestState: {state}");
            objCompleted.SetActive(false);
            objInProgress.SetActive(false);
            objNotStarted.SetActive(false);
            objCanClaim.SetActive(false);

            objNormalPoint.SetActive(false);
            objCurrentPoint.SetActive(false);
            objClaimedPoint.SetActive(false);
            switch (state)
            {
                case QuestState.Completed:
                    objCompleted.SetActive(true);
                    objClaimedPoint.SetActive(true);
                    uiElementProgress.SetValue(1);
                    break;
                case QuestState.InProgress:
                    objInProgress.SetActive(true);
                    objCurrentPoint.SetActive(true);
                    if (playTween)
                    {
                        uiElementProgress.PlayTween(0f, 0.5f, 0.45f);
                    }
                    else
                    {
                        uiElementProgress.SetValue(0.5f);
                    }
                    break;
                case QuestState.NotStarted:
                    objNotStarted.SetActive(true);
                    objNormalPoint.SetActive(true);
                    uiElementProgress.SetValue(0);
                    break;
                case QuestState.CanClaim:
                    objCanClaim.SetActive(true);
                    objClaimedPoint.SetActive(true);
                    uiElementProgress.PlayTween(0f, 0.5f, 0.45f);
                    break;
            }
        }

        public void OnClick()
        {
            var questEventService = SonatSystem.GetService<QuestEventService>();
            // receive reward
            if (questEventService.Data.canClaim)
            {
                Debug.Log("Can claim reward");
                var config = questEventService.config;
                var questData = config.questDatas[questEventService.Data.currentQuestIdx];

                //inventory
                RewardData rewardData = questData.GetRewardData();
                var logData = new EarnResourceLogData
                {
                    spendType = "quest_event",
                    spendId = "quest_event",
                    isFirstBuy = false,
                    source = "non_iap"
                };
                inventoryService.Instance.AddReward(rewardData, logData);

                // uidata
                PanelManager.Instance.ClosePanel<PopupQuestEvent>();

                UIData uiData = new UIData();
                uiData.Add("Title", "REWARD!");
                uiData.Add("Reward", rewardData);
                uiData.Add("x2", false);
                PanelManager.Instance.OpenPanel<PopupReward>(uiData);

                questEventService.ReceiveReward();
            }
            else
            {
                Debug.Log("Can't claim reward");
            }
        }

        public enum QuestState
        {
            Completed,
            InProgress,
            NotStarted,
            CanClaim
        }
    }
}

