using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Tool
{
    public class UIToolSheetData : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button btnLoadData;
        [SerializeField] private Button btnRefreshData;
        [SerializeField] private TMP_Text txtStatus;
        [SerializeField] private TMP_Text txtCurrentLevel;
        [SerializeField] private TMP_Text txtTotalLevels;
        
        [Header("Level Data Display")]
        [SerializeField] private TMP_Text txtLevel;
        [SerializeField] private TMP_Text txtDropLine;
        [SerializeField] private TMP_Text txtMiniTray;
        [SerializeField] private TMP_Text txtLock;
        [SerializeField] private TMP_Text txtNapVung;
        [SerializeField] private TMP_Text txtVendingTray;
        [SerializeField] private TMP_Text txtHidden;
        [SerializeField] private TMP_Text txtLockAndKey;
        [SerializeField] private TMP_Text txtFrozen;
        [SerializeField] private TMP_Text txtIce;
        [SerializeField] private TMP_Text txtBoom;
        [SerializeField] private TMP_Text txtOctoChef;
        [SerializeField] private TMP_Text txtTotalIds;
        [SerializeField] private TMP_Text txtUnique;
        [SerializeField] private TMP_Text txtVariations;
        [SerializeField] private TMP_Text txtNoMatch3;
        [SerializeField] private TMP_Text txtConvey;
        
        [Header("Navigation")]
        [SerializeField] private Button btnPreviousLevel;
        [SerializeField] private Button btnNextLevel;
        
        private GoogleSheetsService sheetsService;
        private LevelDataFromSheet currentLevelData;
        private int currentLevelIndex = 1;
        private List<LevelDataFromSheet> allLevelData = new List<LevelDataFromSheet>();

        private void Start()
        {
            InitializeUI();
            SetupEventListeners();
            
            // Find or create GoogleSheetsService
            sheetsService = FindObjectOfType<GoogleSheetsService>();
            if (sheetsService == null)
            {
                GameObject serviceObj = new GameObject("GoogleSheetsService");
                sheetsService = serviceObj.AddComponent<GoogleSheetsService>();
            }
            
            UpdateStatus("Ready to load data");
            LoadSheetData();
        }

        private void InitializeUI()
        {
            if (txtStatus != null) txtStatus.text = "Initializing...";
            if (txtCurrentLevel != null) txtCurrentLevel.text = "Level: --";
            if (txtTotalLevels != null) txtTotalLevels.text = "Total: --";
            
            ClearLevelDataDisplay();
        }

        private void SetupEventListeners()
        {
            if (btnLoadData != null)
                btnLoadData.onClick.AddListener(LoadSheetData);
                
            if (btnRefreshData != null)
                btnRefreshData.onClick.AddListener(RefreshData);
                
            if (btnPreviousLevel != null)
                btnPreviousLevel.onClick.AddListener(PreviousLevel);
                
            if (btnNextLevel != null)
                btnNextLevel.onClick.AddListener(NextLevel);
        }

        public void LoadSheetData()
        {
            if (sheetsService == null)
            {
                UpdateStatus("GoogleSheetsService not found!");
                return;
            }

            UpdateStatus("Loading data from Google Sheets...");
            
            sheetsService.OnDataLoaded += OnDataLoaded;
            sheetsService.OnError += OnDataLoadError;
            sheetsService.LoadSheetData();
        }

        private void OnDataLoaded()
        {
            allLevelData = sheetsService.GetAllLevelData();
            currentLevelIndex = 1;
            
            UpdateStatus($"Loaded {allLevelData.Count} levels successfully!");
            UpdateTotalLevels();
            
            if (allLevelData.Count > 0)
            {
                DisplayLevelData(currentLevelIndex);
            }
            
            // Unsubscribe from events
            sheetsService.OnDataLoaded -= OnDataLoaded;
            sheetsService.OnError -= OnDataLoadError;
        }

        private void OnDataLoadError(string error)
        {
            UpdateStatus($"Error: {error}");
            
            // Unsubscribe from events
            sheetsService.OnDataLoaded -= OnDataLoaded;
            sheetsService.OnError -= OnDataLoadError;
        }

        public void RefreshData()
        {
            if (sheetsService != null)
            {
                LoadSheetData();
            }
        }

        public void DisplayLevelData(int level)
        {
            currentLevelData = sheetsService.GetLevelData(level);
            
            if (currentLevelData == null)
            {
                UpdateStatus($"Level {level} not found in sheet data");
                ClearLevelDataDisplay();
                return;
            }

            currentLevelIndex = level;
            UpdateCurrentLevel();
            UpdateLevelDataDisplay();
            UpdateStatus($"Displaying Level {level}");
        }

        public void PreviousLevel()
        {
            if (allLevelData.Count == 0) return;
            
            currentLevelIndex = Mathf.Max(1, currentLevelIndex - 1);
            DisplayLevelData(currentLevelIndex);
        }

        public void NextLevel()
        {
            if (allLevelData.Count == 0) return;
            
            currentLevelIndex = Mathf.Min(allLevelData.Count, currentLevelIndex + 1);
            DisplayLevelData(currentLevelIndex);
        }

        public void DisplayLevelFromInput(int level)
        {
            if (sheetsService == null || !sheetsService.IsDataLoaded)
            {
                UpdateStatus("Please load sheet data first");
                return;
            }
            
            DisplayLevelData(level);
        }

        private void UpdateLevelDataDisplay()
        {
            if (currentLevelData == null) return;

            if (txtLevel != null) txtLevel.text = $"Level: {currentLevelData.level}";
            if (txtDropLine != null) txtDropLine.text = $"Drop Line: {currentLevelData.dropLine}";
            if (txtMiniTray != null) txtMiniTray.text = $"Mini Tray: {currentLevelData.miniTray}";
            if (txtLock != null) txtLock.text = $"Lock: {currentLevelData.lockCount}";
            if (txtNapVung != null) txtNapVung.text = $"Nắp vung: {currentLevelData.napVung}";
            if (txtVendingTray != null) txtVendingTray.text = $"Vending Tray: {currentLevelData.vendingTray}";
            if (txtHidden != null) txtHidden.text = $"Hidden: {currentLevelData.hidden}";
            if (txtLockAndKey != null) txtLockAndKey.text = $"Lock & Key: {currentLevelData.lockAndKey}";
            if (txtFrozen != null) txtFrozen.text = $"Frozen: {currentLevelData.frozen}";
            if (txtIce != null) txtIce.text = $"Ice: {currentLevelData.ice}";
            if (txtBoom != null) txtBoom.text = $"Boom: {currentLevelData.boom}";
            if (txtOctoChef != null) txtOctoChef.text = $"OctoChef: {currentLevelData.octoChef}";
            if (txtTotalIds != null) txtTotalIds.text = $"Total IDs: {currentLevelData.totalIds}";
            if (txtUnique != null) txtUnique.text = $"Unique: {currentLevelData.unique}";
            if (txtVariations != null) txtVariations.text = $"Variations: {currentLevelData.variations}";
            if (txtNoMatch3 != null) txtNoMatch3.text = $"No Match3: {currentLevelData.noMatch3}";
            if (txtConvey != null) txtConvey.text = $"Convey: {currentLevelData.convey}";
        }

        private void ClearLevelDataDisplay()
        {
            TMP_Text[] textFields = { txtLevel, txtDropLine, txtMiniTray, txtLock, txtNapVung, txtVendingTray, 
                                     txtHidden, txtLockAndKey, txtFrozen, txtIce, txtBoom, txtOctoChef, 
                                     txtTotalIds, txtUnique, txtVariations, txtNoMatch3, txtConvey };
            
            foreach (var textField in textFields)
            {
                if (textField != null) textField.text = "--";
            }
        }

        private void UpdateStatus(string status)
        {
            if (txtStatus != null)
            {
                txtStatus.text = status;
                Debug.Log($"[UIToolSheetData] {status}");
            }
        }

        private void UpdateCurrentLevel()
        {
            if (txtCurrentLevel != null)
            {
                txtCurrentLevel.text = $"Level: {currentLevelIndex}";
            }
        }

        private void UpdateTotalLevels()
        {
            if (txtTotalLevels != null)
            {
                txtTotalLevels.text = $"Total: {allLevelData.Count}";
            }
        }

        public LevelDataFromSheet GetCurrentLevelData()
        {
            return currentLevelData;
        }

        public bool IsDataLoaded()
        {
            return sheetsService != null && sheetsService.IsDataLoaded;
        }

        private void OnDestroy()
        {
            if (sheetsService != null)
            {
                sheetsService.OnDataLoaded -= OnDataLoaded;
                sheetsService.OnError -= OnDataLoadError;
            }
        }
    }
} 