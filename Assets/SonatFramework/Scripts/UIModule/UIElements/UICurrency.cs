using System;
using System.Collections;
using DG.Tweening;
using Sonat.Enums;
using SonatFramework.Scripts.Feature.Shop.UI;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using SonatFramework.Systems.InventoryManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SonatFramework.Scripts.UIModule.UIElements
{
    public class UICurrency : MonoBehaviour
    {
        public GameResource resource;
        public Image icon;
        public TMP_Text txtValue;
        protected int value = -1;
        private bool scaleUp;
        private Coroutine collectAnim;
        public float counterDuration = 0.5f;
        public float scaleSpeed = 3f;
        public float scaleMax = 1.15f;
        [SerializeField] protected bool blockClick;
        public GameObject plusObj;
        private readonly Service<InventoryService> inventoryService = new();
        [SerializeField] private ParticleSystem blastEffect;
        //[SerializeField] private SonatCollectEffect collectEffect;

        private EventBinding<AddItemEvent> addItemEvent;
        private EventBinding<ReduceItemEvent> reduceItemEvent;
        private EventBinding<AddLimitItemEvent> addLimitItemEvent;

        protected virtual void Start()
        {
            if (plusObj)
                plusObj.SetActive(!blockClick);
            if (blastEffect)
                blastEffect.gameObject.SetActive(false);
        }

        public virtual void OnEnable()
        {
            UpdateValueView(false);
            addItemEvent = new EventBinding<AddItemEvent>(OnAddCurrency);
            reduceItemEvent = new EventBinding<ReduceItemEvent>(OnReduceCurrency);
            addLimitItemEvent = new EventBinding<AddLimitItemEvent>(OnAddLimitCurrency);
            //inventoryService.Instance.OnAddCurrency += OnAddCurrency;
        }

        protected virtual void OnDisable()
        {
            //inventoryService.Instance.OnAddCurrency -= OnAddCurrency;
            EventBus<AddItemEvent>.Deregister(addItemEvent);
            EventBus<ReduceItemEvent>.Deregister(reduceItemEvent);
            EventBus<AddLimitItemEvent>.Deregister(addLimitItemEvent);
            addItemEvent = null;
            reduceItemEvent = null;
            addLimitItemEvent = null;
            txtValue.DOKill();
            
            if (blastEffect)
                blastEffect.gameObject.SetActive(false);
        }


        protected virtual void OnAddCurrency(AddItemEvent eventData)
        {
            if (eventData.resource != this.resource && eventData.resource != GameResource.MAX) return;
            //UpdateValueView(true);
            if (eventData.collectEffect != null && icon != null)
            {
                eventData.collectEffect.Collect(this.resource, eventData.quantity, eventData.position, icon.transform.position, OnCollectEffectFinished);
                //SonatUtils.DelayCall(0.9f, OnCollectEffectFinished, this);
            }
            else
            {
                UpdateValueView();
            }
        }

        protected virtual void OnCollectEffectFinished()
        {
            try
            {
                UpdateValueView();
                if (blastEffect)
                {
                    blastEffect.gameObject.SetActive(true);
                    blastEffect.Play();
                }
            }
            catch (Exception e)
            {
                
            }
        }

        protected virtual void OnReduceCurrency(ReduceItemEvent eventData)
        {
            if (eventData.resource != this.resource && eventData.resource != GameResource.MAX) return;
            SonatUtils.DelayCall(0.1f, () => UpdateValueView(false), this);
        }

        protected virtual void OnAddLimitCurrency(AddLimitItemEvent eventData)
        {
            if (eventData.resource != this.resource && eventData.resource != GameResource.MAX) return;
            UpdateValueView();
        }


        public virtual void UpdateValue(float duration, float delay = 0)
        {
            int oldvalue = this.value;
            value = inventoryService.Instance.GetResource(this.resource);
            if (gameObject.activeInHierarchy)
                txtValue.DOCounter(oldvalue, value, duration, addThousandsSeparator: false).SetDelay(delay);
            else
                txtValue.text = value.ToString();
        }

        public virtual void UpdateValueView(bool doCounter = true)
        {
            int oldvalue = this.value;
            value = inventoryService.Instance.GetResourceView(this.resource);

            if (value == oldvalue) return;
            if (gameObject.activeInHierarchy && doCounter)
                txtValue.DOCounter(oldvalue, value, counterDuration, addThousandsSeparator: false);
            else
                txtValue.text = value.ToString();
        }


        public void SetBlockClick(bool block)
        {
            this.blockClick = block;
            if (plusObj)
                plusObj.SetActive(!block);
        }

        public virtual void OnClickCurrency()
        {
            if (blockClick) return;
            switch (resource)
            {
                case GameResource.Coin:
                    PanelManager.Instance.OpenPanelByName<ShopPanelBase>("ShopPanel");
                    break;
                case GameResource.Lives:
                    var uidata = new UIData();
                    uidata.Add("OpenBy", "UICurrency");
                    PanelManager.Instance.OpenPanel<PopupRefillLives>(uidata);
                    break;
            }
        }

        public void PlayCollectEffect()
        {
            if (!gameObject.activeInHierarchy) return;
            if (collectAnim != null)
            {
                //StopCoroutine(collectAnim);
                scaleUp = true;
                //blastEffect?.Play();
            }
            else
            {
                collectAnim = StartCoroutine(CollectEffect());
                //blastEffect?.gameObject.SetActive(true);
            }

            if (blastEffect)
            {
                var eff = Instantiate(blastEffect.gameObject, icon.transform);
                eff.gameObject.SetActive(true);
                Destroy(eff, 1.2f);
            }
        }

        IEnumerator CollectEffect()
        {
            scaleUp = true;
            while (icon.transform.localScale.x < scaleMax)
            {
                icon.transform.localScale += Vector3.one * Time.deltaTime * scaleSpeed;
                yield return null;
            }

            //SettingManager.Vibrations(100);
            yield return null;
            scaleUp = false;

            while (icon.transform.localScale.x > 1)
            {
                if (scaleUp)
                {
                    collectAnim = StartCoroutine(CollectEffect());
                    yield break;
                }

                icon.transform.localScale -= Vector3.one * Time.deltaTime * scaleSpeed;
                yield return null;
            }

            icon.transform.localScale = Vector3.one;
            collectAnim = null;
        }
    }
}