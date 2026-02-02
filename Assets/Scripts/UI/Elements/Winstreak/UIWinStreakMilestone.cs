using DG.Tweening;
using MyGame.Modules.CardCollection;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.Winstreak
{
    public class UIWinStreakMilestone : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform;

        [SerializeField] private Transform /*lineContainer,*/ previewScaleObj;

        [SerializeField] private GameObject previewObj, flareObj, decoChestObj;

        [SerializeField] private Image chest, board;

        [SerializeField] private UIRewardGrid _rewardGrid;

        private readonly Service<InventoryService> inventoryService = new();

        private readonly Service<WinStreakManager> _winStreakManager = new();

        private int milestone;

        public int targetLevel;

        private bool claimed;

        private bool isInited = false;

        private WinStreakMilestone milestoneData;

        private UIMilestoneState uIMilestoneState;

        public int Milestone { get => milestone; set => milestone = value; }

        public void SetData(int milestone, int targetLevel, WinStreakMilestone milestoneData, UIMilestoneState uIMilestoneState)
        {
            this.milestone = milestone;
            this.targetLevel = targetLevel;
            this.milestoneData = milestoneData;
            this.uIMilestoneState = uIMilestoneState;

            claimed = _winStreakManager.Instance.CheckMilestoneClaimed(milestone);

            //previewScaleObj.localScale = Vector3.one * (milestoneData.reward.resourceDatas.Count == 1 ? 1.5f : 1);

            uIMilestoneState.AddOnClick(ClaimClick);

            Init(milestoneData);

            UpdateUI(false);

            isInited = true;
        }
        internal void SetPos(float posY)
        {
            Vector2 pos = rectTransform.anchoredPosition;
            pos.y = posY;
            rectTransform.anchoredPosition = pos;
        }
        //internal void CreateLine(float lineGap, int lineCount)
        //{
        //    for (int i = 0; i < lineContainer.childCount; i++)
        //        lineContainer.GetChild(i).gameObject.SetActive(false);

        //    if (lineCount <= 0) return;

        //    lineVertical.spacing = lineGap;

        //    for (int i = 0; i < lineCount - 1; i++)
        //    {
        //        _ = poolingService.Instance.CreateObject<GameObject>(lineContainer);
        //    }
        //}
        void Init(WinStreakMilestone milestoneData)
        {
            if (isInited) return;

            chest.sprite = _winStreakManager.Instance.config.GetChestSpite(milestone);

            board.sprite = _winStreakManager.Instance.config.GetSpriteBoard(milestone);

            flareObj.SetActive(milestone == _winStreakManager.Instance.config.rewardDatas.Count - 1);
            decoChestObj.SetActive(flareObj.activeSelf);

            _rewardGrid.SetReward(milestoneData.GetRewardData());
        }
        public void UpdateUI(bool showEffect)
        {
            var complete = milestoneData.levelWinRequired <= _winStreakManager.Instance.winningInARow.Value;

            uIMilestoneState.UpdateState(complete, claimed, showEffect);

        }

        public void ClaimClick()
        {
            if (claimed || _winStreakManager.Instance.winningInARow.Value < targetLevel)
            {
                if (previewObj && !previewObj.activeSelf)
                {
                    previewObj.SetActive(true);
                }
                return;
            }

            claimed = true;

            _winStreakManager.Instance.ClaimReward(milestone);

            uIMilestoneState.UpdateState(true, true);

            //inventory
            //RewardData rewardData = _rewardData;
            var logData = new EarnResourceLogData
            {
                spendType = "winstreak",
                spendId = "winstreak",
                isFirstBuy = false,
                source = "non_iap"
            };
            var rewardData = milestoneData.GetRewardData();
            inventoryService.Instance.AddReward(rewardData, logData);

            // uidata
            UIData uiData = new UIData();
            uiData.Add("Reward", rewardData);
            //uiData.Add("x2", true);

            if (_winStreakManager.Instance.config.GetChestType(milestone) == ChestType.chest_3)
                PanelManager.Instance.OpenPanel<PopupRewardWinstreak>(uiData);
            else
                PanelManager.Instance.OpenPanel<PopupReward>(uiData);

            PanelManager.Instance.ClosePanel<PopupDailyGift>();
        }
    }
}