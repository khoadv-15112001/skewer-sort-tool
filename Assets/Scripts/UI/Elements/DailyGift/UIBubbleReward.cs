using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems.InventoryManagement.GameResources;
using UnityEngine;

public class UIBubbleReward : MonoBehaviour
{
    [SerializeField] private GameObject pReward;
    [SerializeField] private UIRewardGrid rewardGrid;
    private RewardData reward;
    private bool blockUnselectUpdate;

    public void Awake()
    {
        if (gameObject.TryGetComponent<Canvas>(out var canvas))
        {
            canvas.overrideSorting = true;
            canvas.sortingLayerName = "UI_Top";
            canvas.sortingOrder = 100;
        }
    }

    public void SetReward(RewardData reward)
    {
        this.reward = reward;
        rewardGrid.SetReward(reward);
        Unselect();
    }

    public void OnClick()
    {
        if (reward == null || reward.resourceDatas.Count == 0) return;

        if (pReward.activeSelf)
        {
            Unselect();
        }
        else
        {
            Select();
        }
    }

    public void Select()
    {
        pReward.SetActive(true);
    }

    public void Unselect()
    {
        pReward.SetActive(false);
    }

    public void SetBlockUpdate()
    {
        blockUnselectUpdate = true;
    }

    private void Update()
    {
        if (blockUnselectUpdate) return;

        if (Input.GetMouseButtonDown(0))
        {
            Unselect();
        }
    }
}
