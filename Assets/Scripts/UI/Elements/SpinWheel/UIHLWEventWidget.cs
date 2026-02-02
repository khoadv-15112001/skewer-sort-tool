using System;
using Cysharp.Threading.Tasks;
using MyGame.SkewerJam.Scripts.Service;
using SkewerJam.Utils.Effects;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.InventoryManagement;
using Spine.Unity;
using UnityEngine;

namespace GrillSort.DailyGift
{
    public class UIHLWEventWidget : UIHomeWidget
    {
        [SerializeField] private UITimeCounter timeCounter;
        [SerializeField] private GameObject finishObj;
        [SerializeField] private GameObject notiObj;

        [SerializeField] private SkeletonGraphic skeletonGraphic;
        [SerializeField] private Transform spawnPoint_UICollectEffect;



        private readonly Service<HLWEventService> hlwEventService = new();

        public override void Setup()
        {
            base.Setup();

            if (hlwEventService.Instance.IsUnlocked() == false)
            {

                if (hlwEventService.Instance.CanUnlock())
                {
                    hlwEventService.Instance.Unlock();
                }
                else
                {
                    gameObject.SetActive(false);
                    return;
                }

            }

            // Nếu đã kết thúc thì hiện noti để nhận quà xong
            var remainTime = hlwEventService.Instance.GetRemainTime();
            if (remainTime <= 0)
            {
                if (hlwEventService.Instance.CheckReceiveEventReward)
                {
                    HideEvent();
                }
                else
                {
                    OnFinishEvent();
                    hlwEventService.Instance.OnReceiveEventReward += HideEvent;
                }
            }
            else
            {
                notiObj.SetActive(false);
                finishObj.SetActive(false);
                //timeCounter.gameObject.SetActive(true);
                //timeCounter.SetData(remainTime, null);
                hlwEventService.Instance.OnFinishEvent += OnFinishEvent;

                if (CheckShowNoti())
                {
                    notiObj.SetActive(true);
                }
            }
        }


        public override void OnFocus()
        {
            base.OnFocus();
            var remainTime = hlwEventService.Instance.GetRemainTime();
            if (remainTime > 0)
            {
                timeCounter.gameObject.SetActive(true);
                timeCounter.SetData(remainTime, null);
            }
        }


        private bool CheckShowNoti()
        {
            // lần đầu tiên
            if (PlayerPrefs.GetInt("ShowNotiHLWEvent", 0) == 0)
            {
                PlayerPrefs.SetInt("ShowNotiHLWEvent", 1);
                return true;
            }
            return false;
        }

        private void OnFinishEvent()
        {
            // gameObject.SetActive(false);
            timeCounter.gameObject.SetActive(false);
            finishObj.SetActive(true);
            notiObj.SetActive(true);
        }

        private void HideEvent()
        {
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            hlwEventService.Instance.OnFinishEvent -= OnFinishEvent;
            hlwEventService.Instance.OnReceiveEventReward -= HideEvent;
        }

        public override async UniTask<bool> ProcessTask()
        {
            // if (hlwEventService.Instance.IsUnlocked() && PlayerPrefs.GetInt("ShowTutHLWEvent", 0) == 0)
            // {
            //     PlayerPrefs.SetInt("ShowTutHLWEvent", 1);
            //     var popup = PanelManager.Instance.OpenPanelByName<PopupTutorials>("PopupTut_SkewerJam");
            //     await UniTask.WaitUntil(() => popup == null || !popup.gameObject.activeInHierarchy);
            //     await UniTask.Delay(1000);
            // }
            // else
            // {
            // await UniTask.Delay(1000);
            // }
            if (hlwEventService.Instance.IsUnlocked() && !hlwEventService.Instance.CheckFinishEvent())
            {
                await CollectEffect(GameResource.Energy, () =>
                {
                    ShakeIconPumpkin();
                });

                if (!hlwEventService.Instance.CheckFinishEvent() && PlayerPrefs.GetInt("ShowPopupStart_HLW", 0) == 0)
                {
                    PlayerPrefs.SetInt("ShowPopupStart_HLW", 1);
                    var popup = PanelManager.Instance.OpenPanelByName<BasePanel>("PopupStart_HLW");
                    await UniTask.WaitUntil(() => popup == null || !popup.gameObject.activeInHierarchy);
                    await UniTask.Delay(1000);
                    return true;

                }
            }

            return false;
        }

        public void OnClick()
        {
            notiObj.SetActive(false);
            PanelManager.Instance.OpenPanel<PanelLeaderboard_SkewerJam>();
        }

        private async UniTask CollectEffect(GameResource resource, Action onComplete = null)
        {
            var lastValue = MySonatFramework.GetService<InventoryService>().GetResourceView(resource);
            var value = MySonatFramework.GetService<InventoryService>().GetResource(resource);
            var diff = value - lastValue;
            if (diff > 0)
            {
                await UniTask.Delay(1000);
                EventBus<AddItemEvent>.Raise(new AddItemEvent()
                {
                    resource = resource,
                    quantity = diff,
                    position = spawnPoint_UICollectEffect.position,
                    collectEffect = new CollectEffectMultipleAtHome()
                    {
                        collectEffectName = "UICollectResouceEffectAtHome"
                    }
                });
                onComplete?.Invoke();

            }
        }

        private async UniTask ShakeIconPumpkin()
        {
            var token = this.GetCancellationTokenOnDestroy();

            try
            {
                await UniTask.Delay(2000, cancellationToken: token);
                skeletonGraphic.AnimationState.ClearTrack(0);
                skeletonGraphic.AnimationState.SetAnimation(0, "Idle2", true);

                await UniTask.Delay(1000, cancellationToken: token);
                skeletonGraphic.AnimationState.ClearTrack(0);
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("ShakeIconPumpkin was canceled because the object is being destroyed.");
            }
        }

#if UNITY_EDITOR
        [Header("Test Effect")]
        [SerializeField] private float radiusX = 1f;
        [SerializeField] private float radiusY = 0.25f;
        [SerializeField] private int maxCount = 5;
        [SerializeField] private float delaySpawn = 0.1f;
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                EventBus<AddItemEvent>.Raise(new AddItemEvent()
                {
                    resource = GameResource.Pumpkin,
                    quantity = 20,
                    position = spawnPoint_UICollectEffect.position,
                    collectEffect = new CollectEffectMultipleAtHome()
                    {
                        collectEffectName = "UICollectResouceEffectAtHome",
                        radiusX = radiusX,
                        radiusY = radiusY,
                        delaySpawn = delaySpawn,
                        maxCount = maxCount
                    }
                });
                ShakeIconPumpkin().Forget();
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                EventBus<AddItemEvent>.Raise(new AddItemEvent()
                {
                    resource = GameResource.Energy,
                    quantity = 50,
                    position = spawnPoint_UICollectEffect.position,
                    collectEffect = new CollectEffectMultipleAtHome()
                    {
                        collectEffectName = "UICollectResouceEffectAtHome",
                        radiusX = radiusX,
                        radiusY = radiusY,
                        delaySpawn = delaySpawn,
                        maxCount = maxCount
                    }
                });
            }
        }
#endif
    }
}