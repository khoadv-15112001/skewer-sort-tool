using System;
using System.Collections.Generic;
using System.Linq;
using Sonat.Enums;
using SonatFramework.Scripts.Helper;
using SonatFramework.Systems;
using SonatFramework.Systems.SceneManagement;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

namespace SonatFramework.Scripts.Feature.Shop.UI
{
    public class UIShopPackContent : MonoBehaviour
    {
        [SerializeField] private ScrollView scrollView;
        [SerializeField] private Transform content;
        [SerializeField] private Button expandBtn;
        [SerializeField] private TMP_Text txtExpand;
        [SerializeField] private GameObject lessObj;
        [SerializeField] private float bottomPadding = 50;
        [SerializeField] private int maxElementsShow = 10;
        private RectTransform rectTransform;
        private List<UIShopPackBase> uiShopPacks;
        private List<UIShopElement> shopElements;
        private int maxShopElementShow;
        private bool inited;
        private bool expanding;
        private readonly Service<ShopService> shopService = new Service<ShopService>();
        [SerializeField] private bool showFull;

        public UnityEvent<bool> onExpand;

        public static Action OnExpand;
        [SerializeField] private int minElementsShow = 4;
        [SerializeField] private List<GameObject> decoObjects;

        private void Awake()
        {
            if (expandBtn != null)
            {
                expandBtn.onClick.RemoveAllListeners();
                expandBtn.onClick.AddListener(OnExpandClick);
            }
        }

        private void OnEnable()
        {
            var placement = SonatSystem.GetService<SceneService>().GetCurrentGamePlacement();
            
            if (placement == GamePlacement.Gameplay)
            {
                showFull = false;
                maxElementsShow = minElementsShow;
            }
            else
            {
                showFull = true;
                maxElementsShow = 30;
            }
            
            inited = false;
            Init();
            
            SetDecoObjectsActive(showFull);
            
            expanding = false;
            SetExpand(false);
            shopService.Instance.OnBuySuccess += OnBuySuccess;
        }

        private void OnDisable()
        {
            shopService.Instance.OnBuySuccess -= OnBuySuccess;
        }

        private void Init()
        {
            rectTransform = GetComponent<RectTransform>();
            uiShopPacks = new List<UIShopPackBase>();
            var packs = GetComponentsInChildren<UIShopPackBase>(true);
            foreach (var pack in packs)
            {
                if (pack.IsActive())
                {
                    uiShopPacks.Add(pack);
                }
                else
                {
                    pack.gameObject.SetActive(false);
                }
            }

            shopElements = new List<UIShopElement>();
            if (content != null)
            {
                var allElements = content.GetComponentsInChildren<UIShopElement>(true);
                
                foreach (var element in allElements)
                {
                    if (element.gameObject != expandBtn.gameObject)
                    {
                        shopElements.Add(element);
                    }
                }
            }

            shopElements.Sort((a, b) => b.priority.CompareTo(a.priority));
            
            if (showFull)
            {
                FilterEmptyElements();
            }
            else
            {
                foreach (var element in shopElements)
                {
                    element.CheckContent();
                }
            }

            maxShopElementShow = Mathf.Min(shopElements.Count, maxElementsShow);

            UpdateExpandButton();

            inited = true;
        }

        private void OnBuySuccess(ShopItemKey shopItemKey)
        {
            // Clean up destroyed elements first
            CleanupDestroyedElements();
            UpdateElements();
        }

        private void FilterEmptyElements()
        {
            List<UIShopElement> elements = new List<UIShopElement>();
            foreach (var element in shopElements)
            {
                // Skip null or destroyed elements
                if (element == null) continue;
                if (element.priority < 0) continue;

                element.CheckContent();

                if (element.IsEmpty())
                {
                    element.gameObject.SetActive(false);
                }
                else
                {
                    elements.Add(element);
                }
            }

            shopElements = elements;
        }

        private void UpdateExpandButton()
        {
            if (expandBtn == null) return;
            bool shouldShowExpand = maxShopElementShow < shopElements.Count && !showFull;
            expandBtn.gameObject.SetActive(shouldShowExpand);
        }

        public void UpdateElements()
        {
            FilterEmptyElements();
            UpdateExpandButton();
        }

        private void OnExpandClick()
        {
            expanding = !expanding;
            SetExpand(expanding);
            OnExpand?.Invoke();
        }

        private void SetExpand(bool expand)
        {
            CleanupDestroyedElements();

            for (int i = 0; i < shopElements.Count; i++)
            {
                if (shopElements[i] == null) continue;
                
                bool shouldBeActive = expand || i < maxShopElementShow;
                shopElements[i].gameObject.SetActive(shouldBeActive);
            }

            onExpand?.Invoke(expand);
            
            if (txtExpand != null)
            {
                string text = expand ? "Hide" : "More Offers!";
                txtExpand.SetLocalize(text);
            }

            if (lessObj != null) lessObj.SetActive(expand);
            
            SetDecoObjectsActive(showFull || expand);
        }
        
        private void SetDecoObjectsActive(bool active)
        {
            if (decoObjects == null) return;
            
            foreach (var obj in decoObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(active);
                }
            }
        }
        
        private void CleanupDestroyedElements()
        {
            shopElements.RemoveAll(element => element == null);
        }
    }
}
