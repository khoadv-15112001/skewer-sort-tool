using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GrillSort.PiggyBank;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;

public class UIReceivePiggyPoint : MonoBehaviour
{
    [SerializeField] private UIRewardItem rewardItem;
    [SerializeField] private GameObject pMain;
    private readonly Service<PiggyBankService> piggyBankService = new();

    private int points;
    private bool isClaimed = false;
    private bool isFull = false;
    public Action onCollect;

    private void OnEnable()
    {
        if (piggyBankService.Instance.IsUnlocked() == false && piggyBankService.Instance.CanUnlock() == false)
        {
            gameObject.SetActive(false);
            return;
        }

        piggyBankService.Instance.Unlock();

        points = piggyBankService.Instance.PiggyBankConfig.reward;
        rewardItem.Init(GameResource.PiggyPoint, points);
        isClaimed = false;

        isFull = piggyBankService.Instance.CanUpdatePiggyTier();

        rewardItem.gameObject.SetActive(!isFull);

        SonatUtils.DelayCall(2f, OnClaimClick);
    }

    private void OnDisable()
    {

    }

    [SerializeField] private float delaySoundTime = 0.5f;
    public async void OnClaimClick()
    {
        if (isClaimed) return;
        isClaimed = true;
        if (!isFull)
        {
            var addPoints = SonatSystem.GetService<PiggyBankService>().GetAddedReward();
            var log = new EarnResourceLogData()
            {
                spendId = "win",
                spendType = "win"
            };
            MySonatFramework.audioService.PlaySound(AudioId.Piggy_Coins_Appear);
            SonatUtils.DelayCall(delaySoundTime, () =>
            {
                MySonatFramework.audioService.PlaySound(AudioId.Piggy_Coins_Drop);
            });


            //MySonatFramework.GetService<InventoryService>().AddResource(GameResource.PiggyPoint, addPoints, log, false);
            SonatSystem.GetService<PoolingService>().Create<EffectPoolBase>("PigybankDropEffect", this.transform.position, this.transform);
            onCollect?.Invoke();
            int spawnCount = addPoints;
            for (int i = 0; i < spawnCount; i++)
            {

                var collectEffect = new CollectEffectSingle() { collectEffectName = "CollectResourceSingleItemPiggyCoin" };
                EventBus<AddItemEvent>.Raise(new AddItemEvent()
                {
                    resource = GameResource.PiggyPoint,
                    quantity = 1,
                    position = rewardItem.transform.position,
                    collectEffect = collectEffect
                });
                if (i < spawnCount - 1)
                {
                    await UniTask.Delay(200);
                }

            }
            rewardItem.gameObject.SetActive(false);

        }
        else
        {
            //pigIcon.DOShakePosition(0.2f, 1, 5).SetLoops(-1, LoopType.Yoyo).SetDelay(1);
        }

        // SonatUtils.DelayCall(2.25f, () =>
        // {
        //     //gameObject.SetActive(false);
        //     pMain.transform.DOLocalMoveX(-350f, 0.5f).SetEase(Ease.InBack);
        // }, this);

    }
}
