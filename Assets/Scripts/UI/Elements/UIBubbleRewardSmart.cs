using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement.GameResources;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;

public class UIBubbleRewardSmart : MonoBehaviour
{
    [SerializeField] private GameObject pReward;
    private RewardData rewardData;
    private bool blockUnselectUpdate;

    private readonly Service<PoolingContainerService> poolingService = new();

    //public void Awake()
    //{
    //    if (gameObject.TryGetComponent<Canvas>(out var canvas))
    //    {
    //        canvas.overrideSorting = true;
    //        canvas.sortingLayerName = "UI_Top";
    //        canvas.sortingOrder = 100;
    //    }
    //}

    public void SetReward(RewardData rewardData)
    {
        this.rewardData = rewardData;

        poolingService.Instance.CleanContainer(this.transform);

        foreach (var reward in rewardData.resourceDatas)
        {
            var uiItem = poolingService.Instance.CreateObject<UIRewardItem>(this.transform);

            uiItem.Init(reward.resource, reward.quantity);
        }

        Unselect();
    }

    public void OnClick()
    {
        if (rewardData == null || rewardData.resourceDatas.Count == 0) return;

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
