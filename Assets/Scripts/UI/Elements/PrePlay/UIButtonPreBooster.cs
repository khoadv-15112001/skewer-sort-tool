using DG.Tweening;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.BoosterManagement;
using SonatFramework.Systems.EventBus;
using SonatFramework.Templates.UI.ScriptBase;
using TMPro;
using UnityEngine;

namespace GrillSort.PreBooster
{
    public class UIButtonPreBooster : MonoBehaviour
    {
        public GameResource boosterType;
        public GameObject[] lockObj;
        public GameObject[] unlockObj;
        public GameObject[] selectObj;
        public GameObject[] unselectObj;
        public TMP_Text txtQuantity;
        public TMP_Text txtLevelUnlock;
        public GameObject priceObj;
        public TMP_Text txtPrice;

        [SerializeField] protected readonly Service<PreBoosterService> preBoosterService = new();
        protected BoosterData boosterData;
        protected BoosterConfig config;

        private bool inited = false;
        protected bool unlocked;
        protected EventBinding<AddItemEvent> collectItemEvent;
        protected EventBinding<ReduceItemEvent> reduceItemEvent;
        protected bool usingBooster;
        private bool inactive = false;

        public bool IsSelected => usingBooster;

        protected virtual void OnEnable()
        {
            Init();
            UpdateData();
            preBoosterService.Instance.onUnlockBooster += OnBoosterUnlocked;
            collectItemEvent = new EventBinding<AddItemEvent>(OnCollectResource);
            reduceItemEvent = new EventBinding<ReduceItemEvent>(OnReduceBooster);
            inactive = false;
        }

        protected virtual void OnDisable()
        {
            preBoosterService.Instance.onUnlockBooster -= OnBoosterUnlocked;
            EventBus<AddItemEvent>.Deregister(collectItemEvent);
            EventBus<ReduceItemEvent>.Deregister(reduceItemEvent);
            collectItemEvent = null;
            reduceItemEvent = null;
            inactive = true;
        }

        protected virtual void Init()
        {
            if (inited) return;
            inited = true;

            config = preBoosterService.Instance.GetBoosterConfig(boosterType);
            boosterData = preBoosterService.Instance.GetBoosterData(boosterType);
            Debug.Log("Init booster: " + boosterType + " " + boosterData.quantity + " " + boosterData.unlocked);
            txtPrice.text = config.price.ToString();
            SetLockState(true);
            Select(false);
        }

        private void OnBoosterUnlocked(GameResource resources)
        {
            if (boosterType != resources && resources != GameResource.MAX) return;
            UpdateData();
            SetLockState();
            UnlockBooster();
        }

        protected virtual void OnCollectResource(AddItemEvent eventData)
        {
            if (eventData.resource != this.boosterType) return;
            if (eventData.collectEffect != null)
            {
                eventData.collectEffect.Collect(this.boosterType, eventData.quantity, eventData.position, transform.position, OnCollectEffectCompleted);
            }
            else
            {
                UpdateData();
            }
        }

        private void OnCollectEffectCompleted()
        {
            if(inactive) return;
            float defaultScale = transform.localScale.x;
            transform.DOScale(defaultScale * 1.1f, 0.075f).SetLoops(2, LoopType.Yoyo);
            UpdateData();
        }

        protected virtual void OnReduceBooster(ReduceItemEvent eventData)
        {
            if (eventData.resource != this.boosterType) return;
            SonatUtils.DelayCall(0.1f, UpdateData, this);
        }

        protected void UpdateData()
        {
            txtQuantity.text = boosterData.quantity.ToString();
            priceObj.SetActive(boosterData.quantity <= 0);
        }

        protected virtual void SetLockState(bool force = false)
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

        }


        public virtual void ClickBooster()
        {
            if (preBoosterService.Instance.CanUseBooster(boosterType))
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
            PopupToast.Cretate($"Unlock at level:", config.levelUnlock.ToString());
        }

        public virtual void OnOutOfBooster()
        {
            UIData uiData = new UIData();
            uiData.Add("booster_config", config);
            PanelManager.Instance.OpenPanelByName<PopupBuyPreBooster>("PopupBuyPreBooster", uiData);
        }

        public virtual void UseBooster()
        {
            usingBooster = !usingBooster; // chọn booster
            Select(usingBooster);

            if (usingBooster)
            {
                UIPreBoosterManager.usedBoosters.Add(boosterType);
            }
            else
            {
                UIPreBoosterManager.usedBoosters.Remove(boosterType);
            }
        }

        private void Select(bool isSelect)
        {
            usingBooster = isSelect;
            foreach (var obj in selectObj)
            {
                obj.SetActive(isSelect);
            }
            foreach (var obj in unselectObj)
            {
                obj.SetActive(!isSelect);
            }
        }
    }
}
