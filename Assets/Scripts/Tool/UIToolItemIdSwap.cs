using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Tool
{
    public class UIToolItemIdSwap : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button btnLoadMappingData;
        [SerializeField] private Button btnApplySwapping;
        [SerializeField] private Button btnReverseSwapping;
        [SerializeField] private Button btnShowSummary;
        [SerializeField] private TMP_Text txtStatus;
        [SerializeField] private TMP_Text txtMappingCount;

        [Header("Settings")]
        [SerializeField] private bool autoLoadOnStart = true;

        private void Start()
        {
            SetupButtons();
            UpdateStatus();

            if (autoLoadOnStart)
            {
                LoadMappingData();
            }
        }

        private void SetupButtons()
        {
            if (btnLoadMappingData != null)
            {
                btnLoadMappingData.onClick.AddListener(LoadMappingData);
            }

            if (btnApplySwapping != null)
            {
                btnApplySwapping.onClick.AddListener(ApplySwapping);
            }

            if (btnReverseSwapping != null)
            {
                btnReverseSwapping.onClick.AddListener(ReverseSwapping);
            }

            if (btnShowSummary != null)
            {
                btnShowSummary.onClick.AddListener(ShowSummary);
            }
        }

        public void LoadMappingData()
        {
            if (ItemIdSwapService.Instance != null)
            {
                // Subscribe to events
                ItemIdSwapService.Instance.OnDataLoaded += OnMappingDataLoaded;
                ItemIdSwapService.Instance.OnError += OnMappingDataError;

                // Load the data
                ItemIdSwapService.Instance.LoadIdMappingData();
                UpdateStatus("Loading item ID mapping data...");
            }
            else
            {
                UpdateStatus("ItemIdSwapService not available");
            }
        }

        private void OnMappingDataLoaded()
        {
            UpdateStatus($"Item ID mapping data loaded successfully! Total old IDs: {ItemIdSwapService.Instance.GetTotalMappings()}, Total new IDs: {ItemIdSwapService.Instance.GetTotalNewIds()}");
            
            // Unsubscribe from events
            if (ItemIdSwapService.Instance != null)
            {
                ItemIdSwapService.Instance.OnDataLoaded -= OnMappingDataLoaded;
                ItemIdSwapService.Instance.OnError -= OnMappingDataError;
            }
        }

        private void OnMappingDataError(string error)
        {
            UpdateStatus($"Error loading mapping data: {error}");
            
            // Unsubscribe from events
            if (ItemIdSwapService.Instance != null)
            {
                ItemIdSwapService.Instance.OnDataLoaded -= OnMappingDataLoaded;
                ItemIdSwapService.Instance.OnError -= OnMappingDataError;
            }
        }

        public void ApplySwapping()
        {
            if (ItemIdSwapService.Instance == null || !ItemIdSwapService.Instance.IsDataLoaded)
            {
                UpdateStatus("Please load item ID mapping data first");
                return;
            }

            if (UIToolPanel.Instance != null)
            {
                UIToolPanel.Instance.ApplyItemIdSwappingToCurrentLevel();
            }
        }

        public void ReverseSwapping()
        {
            if (ItemIdSwapService.Instance == null || !ItemIdSwapService.Instance.IsDataLoaded)
            {
                UpdateStatus("Please load item ID mapping data first");
                return;
            }

            if (UIToolPanel.Instance != null)
            {
                UIToolPanel.Instance.ReverseItemIdSwappingToCurrentLevel();
            }
        }

        public void ShowSummary()
        {
            if (ItemIdSwapService.Instance == null || !ItemIdSwapService.Instance.IsDataLoaded)
            {
                UpdateStatus("No mapping data available");
                return;
            }

            string summary = ItemIdSwapHelper.GetMappingSummary();
            UpdateStatus("Mapping summary displayed in notification");
            
            if (UIToolPanel.Instance != null)
            {
                UIToolPanel.Instance.ShowItemIdMappingSummary();
            }
        }

        private void UpdateStatus(string message = null)
        {
            if (txtStatus != null)
            {
                if (message != null)
                {
                    txtStatus.text = message;
                }
                else
                {
                    if (ItemIdSwapService.Instance != null && ItemIdSwapService.Instance.IsDataLoaded)
                    {
                        txtStatus.text = $"Mapping data loaded ({ItemIdSwapService.Instance.GetTotalMappings()} mappings)";
                    }
                    else
                    {
                        txtStatus.text = "Mapping data not loaded";
                    }
                }
            }

            if (txtMappingCount != null)
            {
                if (ItemIdSwapService.Instance != null && ItemIdSwapService.Instance.IsDataLoaded)
                {
                    txtMappingCount.text = $"Mappings: {ItemIdSwapService.Instance.GetTotalMappings()} old IDs, {ItemIdSwapService.Instance.GetTotalNewIds()} new IDs";
                }
                else
                {
                    txtMappingCount.text = "Mappings: 0";
                }
            }

            // Update button interactability
            bool hasData = ItemIdSwapService.Instance != null && ItemIdSwapService.Instance.IsDataLoaded;
            
            if (btnApplySwapping != null)
            {
                btnApplySwapping.interactable = hasData;
            }
            
            if (btnReverseSwapping != null)
            {
                btnReverseSwapping.interactable = hasData;
            }
            
            if (btnShowSummary != null)
            {
                btnShowSummary.interactable = hasData;
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events to prevent memory leaks
            if (ItemIdSwapService.Instance != null)
            {
                ItemIdSwapService.Instance.OnDataLoaded -= OnMappingDataLoaded;
                ItemIdSwapService.Instance.OnError -= OnMappingDataError;
            }
        }
    }
} 