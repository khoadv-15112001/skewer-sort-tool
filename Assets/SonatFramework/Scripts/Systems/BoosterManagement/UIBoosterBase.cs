using System;
using DG.Tweening;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Templates.UI.ScriptBase;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace SonatFramework.Systems.BoosterManagement
{
    public class UIBoosterBase : MonoBehaviour
    {
        public GameResource boosterType;
        public GameObject[] lockObj;
        public GameObject[] unlockObj;
        public TMP_Text txtQuantity;
        public TMP_Text txtLevelUnlock;
        public GameObject priceObj;
        public TMP_Text txtPrice;

        [SerializeField] protected readonly Service<BoosterService> boosterService = new();
        protected BoosterData boosterData;
        protected BoosterConfig config;

        private bool inited;
        protected bool unlocked;
        protected EventBinding<AddItemEvent> collectItemEvent;
        protected EventBinding<ReduceItemEvent> reduceItemEvent;
        protected bool usingBooster;

        protected virtual void OnEnable()
        {
            Init();
            UpdateData();
            boosterService.Instance.onUnlockBooster += OnBoosterUnlocked;
            collectItemEvent = new EventBinding<AddItemEvent>(OnCollectResource);
            reduceItemEvent = new EventBinding<ReduceItemEvent>(OnReduceBooster);
        }

        protected virtual void OnDisable()
        {
            boosterService.Instance.onUnlockBooster -= OnBoosterUnlocked;
            EventBus<AddItemEvent>.Deregister(collectItemEvent);
            EventBus<ReduceItemEvent>.Deregister(reduceItemEvent);
            collectItemEvent = null;
        }

        protected virtual void Init()
        {
            if (inited) return;
            inited = true;

            config = boosterService.Instance.GetBoosterConfig(boosterType);
            boosterData = boosterService.Instance.GetBoosterData(boosterType);
            txtPrice.text = config.price.ToString();
            UpdateLockVisual(true);
        }

        private void OnBoosterUnlocked(GameResource resources)
        {
            if (boosterType != resources && resources != GameResource.MAX) return;
            UpdateData();
            //SetLockState();
            UnlockBooster();
        }

        protected virtual void OnCollectResource(AddItemEvent eventData)
        {
            if(eventData.resource != this.boosterType) return;
            if (eventData.collectEffect != null)
            {
                eventData.collectEffect.Collect(this.boosterType, eventData.quantity, eventData.position, transform.position, () =>
                {
                    float defaultScale = transform.localScale.x;
                    transform.DOScale(defaultScale * 1.1f, 0.075f).SetLoops(2, LoopType.Yoyo);
                    UpdateData();
                });
            }
            else
            {
                UpdateData();
            }
        }

        protected virtual void OnReduceBooster(ReduceItemEvent eventData)
        {
            if (eventData.resource != this.boosterType) return;
            SonatUtils.DelayCall(0.1f, UpdateData, this);
        }

        protected virtual void UpdateData()
        {
            txtQuantity.text = boosterData.quantity.ToString();
            priceObj.SetActive(boosterData.quantity <= 0);
        }

        protected virtual void UpdateLockVisual(bool force = false)
        {
            if (!force && unlocked) return;
            unlocked = boosterData.unlocked;
            if (lockObj != null)
                for (var i = 0; i < lockObj.Length; i++)
                    lockObj[i].SetActive(!unlocked);

            if (unlockObj != null)
                for (var i = 0; i < unlockObj.Length; i++)
                    unlockObj[i].SetActive(unlocked);

            if (!unlocked) txtLevelUnlock.text = $"Lv.{config.levelUnlock}";
        }

        protected virtual void UnlockBooster()
        {
            UpdateLockVisual();
        }


        public virtual void ClickBooster()
        {
            if(usingBooster) return;
            if (boosterService.Instance.CanUseBooster(boosterType))
            {
                UseBooster();
            }
            else
            {
                if (unlocked) OnOutOfBooster();
                else
                {
                    BoosterLockFeedback();
                }
            }
        }

        protected virtual void BoosterLockFeedback()
        {
            
        }

        public virtual void OnOutOfBooster()
        {
            UIData uiData = new UIData();
            uiData.Add("booster_config", config);
            PanelManager.Instance.OpenPanelByName<PopupBuyBoosterBase>("PopupBuyBooster", uiData);
        }

        public virtual void UseBooster()
        {
            usingBooster = true;
            Debug.Log($"Use booster: {config.booster}");
        }

        public virtual void OnUseBoosterSuccess()
        {
            usingBooster = false;
            boosterService.Instance.UseBoosterSuccess(boosterType);
        }
    }
}