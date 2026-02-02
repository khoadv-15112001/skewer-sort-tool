using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement;
using UnityEngine;

namespace GrillSort.BattlePass
{
    public class UIReward : MonoBehaviour
    {
        [SerializeField] private GameObject objNotStarted;
        [SerializeField] private GameObject objCanClaim;
        [SerializeField] private GameObject objCompleted;
        [SerializeField] private UIRewardGrid rewardGrid;
        [SerializeField] private UIGiftBox objRewardGiftBox;
        [SerializeField] private bool isPremium;
        [SerializeField] private BattlePassBase battlePassBase;
        private int idx;
        private RewardState state;
        private readonly Service<InventoryService> inventoryService = new();

        public void Init(int idx)
        {
            this.idx = idx;

            var reward = battlePassBase.config.GetReward(idx);
            if (reward.resourceDatas.Count == 1)
            {
                rewardGrid.gameObject.SetActive(true);
                rewardGrid.SetReward(reward);
                objRewardGiftBox.gameObject.SetActive(false);
            }
            else
            {
                objRewardGiftBox.gameObject.SetActive(true);
                objRewardGiftBox.SetData(reward);
                rewardGrid.gameObject.SetActive(false);
            }

        }

        public void UpdateUI()
        {
            if (idx <= MySonatFramework.GetService<BattlePassService>().CurrentMilestoneIdx && battlePassBase.IsUnlocked())
            {
                if (battlePassBase.CheckReceivedReward(idx))
                {
                    state = RewardState.Completed;
                }
                else
                {
                    state = RewardState.CanClaim;
                }
            }
            else if (idx > MySonatFramework.GetService<BattlePassService>().CurrentMilestoneIdx)
            {
                state = RewardState.NotStarted;
            }
            SetState(state);
        }

        public void SetState(RewardState state)
        {
            this.state = state;

            objNotStarted.SetActive(false);
            objCanClaim.SetActive(false);
            objCompleted.SetActive(false);

            switch (state)
            {
                case RewardState.NotStarted:
                    objNotStarted.SetActive(true);
                    break;
                case RewardState.CanClaim:
                    objCanClaim.SetActive(true);
                    break;
                case RewardState.Completed:
                    objCompleted.SetActive(true);
                    break;
            }
        }

        public void ClickGiftBox()
        {
            PopupBattlePass.OnShowPreview?.Invoke(battlePassBase.config.GetReward(idx), transform);
        }

        public void OnClick()
        {
            switch (state)
            {
                case RewardState.CanClaim:
                    battlePassBase.ReceiveReward(idx);
                    UpdateUI();

                    var reward = battlePassBase.config.GetReward(idx);

                    //inventory
                    var logData = new EarnResourceLogData
                    {
                        spendType = isPremium ? "battle_pass_premium" : "battle_pass",
                        spendId = isPremium ? "battle_pass_premium" : "battle_pass",
                        isFirstBuy = false,
                        source = isPremium ? "iap" : "non_iap"
                    };
                    inventoryService.Instance.AddReward(reward, logData);

                    // uidata
                    UIData uiData = new UIData();
                    uiData.Add("Title", "REWARD!");
                    uiData.Add("Reward", reward);
                    uiData.Add("x2", false);
                    PanelManager.Instance.OpenPanel<PopupReward>(uiData);

                    break;
                case RewardState.NotStarted:
                    Debug.Log("Locked");
                    break;
                case RewardState.Completed:
                    Debug.Log("Completed");
                    break;
            }

        }
    }
}

public enum RewardState
{
    NotStarted,
    CanClaim,
    Completed
}
