using DG.Tweening;
using GrillSort.QuestEvent;
using Sonat.Enums;
using SonatFramework.Scripts.Feature.Shop;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement.GameResources;

public class PopupWeekendDeal : PopupPack
{
    private readonly Service<QuestEventService> questEventService = new();
    protected readonly Service<ShopService> shopService = new();
    public override void OnSetup()
    {
        base.OnSetup();

        shopItemKey = ShopItemKey.WeekendDeal;
    }
    public override void OnBuySuccess()
    {
        base.OnBuySuccess();

        RewardData rewardData = shopService.Instance.GetPackData(shopItemKey).rewardData;

        if (rewardData.resourceDatas.Find(x => x.resource == GameResource.X2ItemQuestEvent) != null)
        {
            questEventService.Instance.CheckActiveX2Item();
        }
    }
}
