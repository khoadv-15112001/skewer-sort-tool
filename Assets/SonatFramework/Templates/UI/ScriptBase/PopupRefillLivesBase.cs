using System;
using System.Collections;
using Sonat;
using Sonat.Enums;
using SonatFramework.Scripts.Feature.CheckInternet;
using SonatFramework.Scripts.Feature.Lives;
using SonatFramework.Scripts.Feature.Lives.UI;
using SonatFramework.Scripts.Feature.Shop.UI;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.NetworkManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SonatFramework.Templates.UI.ScriptBase
{
    public class PopupRefillLivesBase : Panel
    {
        [SerializeField] protected TMP_Text refillTimer, unlimitedTimer;
        //public UILivesList uiLiveList;
        [SerializeField] protected UIResourceValue liveValue;
        [SerializeField] protected GameObject closeButton;
        public TMP_Text txtRefillPrice;
        public GameObject fullLivesObj;
        public GameObject nonFullLivesObj;
        public GameObject unlimitedLivesObj;
        public GameObject noInternetObj;

        public Button refillFreeBtn;

        //public Button refillWithCoinBtn;
        public GameObject refillNonFree;
        public Button refillWithAdsBtn;
        public GameObject noInternetBuyLiveCoinBtn, noInternetFreeLiveBtn;
        public TMP_Text txtNoInternetCoinPrice;
        protected readonly Service<LivesService> liveService = new();
        protected readonly Service<InventoryService> inventory = new();
        //protected readonly Service<CheckInternetService> checkInternetService = new();

        public override void OnSetup()
        {
            base.OnSetup();
            //uiLiveList.Setup();
        }

        public override void Open(UIData uiData)
        {
            base.Open(uiData);
            CheckLive();
        }

        protected virtual void OnEnable()
        {
            liveService.Instance.onLivesUpdate += CheckLive;
        }

        protected virtual void OnDisable()
        {
            liveService.Instance.onLivesUpdate -= CheckLive;
        }

        public override void OnOpenCompleted()
        {
            base.OnOpenCompleted();
        }

        protected override void OnCloseCompleted()
        {
            base.OnCloseCompleted();
            //SetGameStateBefore();
        }

        public virtual void CheckLive()
        {
            if (gameObject == null) return;

            if (!liveService.Instance.isUnlimitedLive.BoolValue)
            {
                StopCoroutine(nameof(IeCountdownUnlimited));

                unlimitedLivesObj.SetActive(false);
                //uiLiveList.gameObject.SetActive(true);
                liveValue.gameObject.SetActive(true);

                int live = inventory.Instance.GetResource(GameResource.Lives);

                if (live >= liveService.Instance.config.maxLives)
                {
                    StopCoroutine(nameof(IeCountdownRefill));

                    fullLivesObj.SetActive(true);
                    nonFullLivesObj.SetActive(false);
                    noInternetObj.SetActive(false);
                }
                else
                {
                    bool isInternetConnect = !liveService.Instance.forceInternet || SonatSdkManager.IsInternetConnection();
                    noInternetObj.SetActive(!isInternetConnect);
                    long timeRefillRemain = liveService.Instance.GetTimeRefillRemain();
                    if (timeRefillRemain <= 0)
                    {
                        fullLivesObj.SetActive(false);
                        nonFullLivesObj.SetActive(false);

                        if (!isInternetConnect)
                        {
                            bool canRefillFree = liveService.Instance.CanRefillFree();
                            noInternetBuyLiveCoinBtn.SetActive(!canRefillFree);
                            noInternetFreeLiveBtn.SetActive(canRefillFree);
                            if (!canRefillFree)
                            {
                                txtNoInternetCoinPrice.text = liveService.Instance.GetRefillPrice().ToString();
                                // txtNoInternetCoinPrice.text =
                                //     $"<sprite name=\"ico_Coin\"> {liveService.Instance.GetRefillPrice()}";
                            }
                        }
                        else
                        {
                            Close();
                        }
                    }
                    else
                    {
                        if (!isInternetConnect)
                        {
                            fullLivesObj.SetActive(false);
                            nonFullLivesObj.SetActive(false);

                            bool canRefillFree = liveService.Instance.CanRefillFree();
                            noInternetBuyLiveCoinBtn.SetActive(!canRefillFree);
                            noInternetFreeLiveBtn.SetActive(canRefillFree);
                            if (!canRefillFree)
                            {
                                txtNoInternetCoinPrice.text = liveService.Instance.GetRefillPrice().ToString();
                                // txtNoInternetCoinPrice.text =
                                //     $"<sprite name=\"ico_Coin\"> {liveService.Instance.GetRefillPrice()}";
                            }
                        }
                        else
                        {
                            fullLivesObj.SetActive(false);
                            nonFullLivesObj.SetActive(true);

                            StartCoroutine(nameof(IeCountdownRefill));

                            bool canRefillFree = liveService.Instance.CanRefillFree();
                            refillFreeBtn.gameObject.SetActive(canRefillFree);
                            refillNonFree.SetActive(!canRefillFree);
                            if (!canRefillFree)
                            {
                                txtRefillPrice.text = liveService.Instance.GetRefillPrice().ToString();
                                // txtRefillPrice.text =
                                //     $"<sprite name=\"ico_Coin\"> {liveService.Instance.GetRefillPrice()}";
                            }
                        }
                    }
                }
            }
            else
            {
                StopCoroutine(nameof(IeCountdownRefill));

                nonFullLivesObj.SetActive(false);
                unlimitedLivesObj.SetActive(true);
                //uiLiveList.gameObject.SetActive(false);
                liveValue.gameObject.SetActive(false);
                fullLivesObj.SetActive(false);
                nonFullLivesObj.SetActive(false);

                StartCoroutine(nameof(IeCountdownUnlimited));
            }
        }

        protected virtual IEnumerator IeCountdownRefill()
        {
            long timeRemain = liveService.Instance.GetTimeRefillRemain();

            while (timeRemain > 0)
            {
                UpdateTimer(refillTimer, timeRemain);

                yield return new WaitForSecondsRealtime(1);

                timeRemain = liveService.Instance.GetTimeRefillRemain();
            }

            CheckLive();
        }

        protected virtual IEnumerator IeCountdownUnlimited()
        {
            long timeRemain = liveService.Instance.GetUnlimitedLiveRemain();

            while (timeRemain > 0)
            {
                UpdateTimer(unlimitedTimer, timeRemain);

                yield return new WaitForSecondsRealtime(1);

                timeRemain = liveService.Instance.GetUnlimitedLiveRemain();
            }

            CheckLive();
        }

        protected virtual void UpdateTimer(TMP_Text timer, long sec)
        {
            TimeSpan time = TimeSpan.FromSeconds(sec);

            if (time.Hours > 0)
            {
                timer.text = $"{(int)(time.TotalHours):D2}:{time.Minutes:D2}:{time.Seconds:D2}";
            }
            else
                timer.text = $"{(int)(time.TotalMinutes):D2}:{time.Seconds:D2}";
        }

        public virtual void RefillFree()
        {
            liveService.Instance.RefillFullLive(new EarnResourceLogData() { spendId = "free", spendType = "free" });
            liveService.Instance.refillFreeCount.Value++;
            CheckLive();
        }

        public virtual void RefillWithAds()
        {
            SonatSDKAdapter.ShowRewardAds(OnRefillWithAds, "live", "live");
        }

        protected virtual void OnRefillWithAds()
        {
            liveService.Instance.RefillOneLive(new EarnResourceLogData()
            {
                spendId = "rw_ads",
                spendType = "rw_ads",
            });
            CheckLive();
        }

        public virtual void RefillWithCoin()
        {
            int price = liveService.Instance.GetRefillPrice();
            if (!inventory.Instance.CanReduce(liveService.Instance.config.refillPriceCurrency, price))
            {
                // HomeScreen homeScreen = PanelManager.Instance.GetPanel<HomeScreen>();
                //
                //          if (homeScreen != null && homeScreen.gameObject.activeSelf)
                // {
                // 	homeScreen.SelectPanel(HomeBtnType.Shop);
                // }
                // else
                // {
                //              PanelManager.Instance.OpenPanelImmediately<ShopPanel>();
                //          }
                //
                //          return;
                PopupToast.Cretate("Not enough coin!");
                PanelManager.Instance.OpenPanelByName<ShopPanelBase>("ShopPanel");
                return;
            }

            inventory.Instance.ReduceResource(liveService.Instance.config.refillPriceCurrency, price,
                new SpendResourceLogData() { earnType = "Currency", earnId = "Lives" });
            liveService.Instance.RefillFullLive(new EarnResourceLogData()
            {
                spendId = liveService.Instance.config.refillPriceCurrency.ToString(),
                spendType = GameResourceType.Currency.ToString(),
                price = price,
            });
            CheckLive();
        }
    }
}