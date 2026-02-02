using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace Tool
{
    public class GoogleSheetsService : MonoBehaviour
    {
        private const string SHEET_URL = "https://docs.google.com/spreadsheets/d/1oo_Wp5MOUnvOAh-C0WNR7edQBpW0uklENg0fAORuJzU/edit?gid=149409390#gid=149409390";
        private const string EXPORT_URL = "https://docs.google.com/spreadsheets/d/1oo_Wp5MOUnvOAh-C0WNR7edQBpW0uklENg0fAORuJzU/export?format=csv&gid=149409390";
        
        private Dictionary<int, LevelDataFromSheet> levelDataCache = new Dictionary<int, LevelDataFromSheet>();
        private bool isDataLoaded = false;
        
        public static GoogleSheetsService Instance { get; private set; }
        
        public event Action OnDataLoaded;
        public event Action<string> OnError;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void LoadSheetData()
        {
            StartCoroutine(LoadDataFromGoogleSheets());
        }

        private IEnumerator LoadDataFromGoogleSheets()
        {
            using (UnityWebRequest request = UnityWebRequest.Get(EXPORT_URL))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    ParseCSVData(request.downloadHandler.text);
                    isDataLoaded = true;
                    OnDataLoaded?.Invoke();
                    Debug.Log($"Successfully loaded {levelDataCache.Count} levels from Google Sheets");
                }
                else
                {
                    string error = $"Failed to load Google Sheets data: {request.error}";
                    Debug.LogError(error);
                    OnError?.Invoke(error);
                }
            }
        }

        private void ParseCSVData(string csvData)
        {
            levelDataCache.Clear();
            
            string[] lines = csvData.Split('\n');
            
            // Skip header rows (first 2 rows)
            for (int i = 2; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;
                
                // Parse CSV line (handle quoted values)
                string[] rowData = ParseCSVLine(line);
                
                if (rowData.Length > 0 && int.TryParse(rowData[0], out int level))
                {
                    LevelDataFromSheet levelData = new LevelDataFromSheet(rowData);
                    levelDataCache[level] = levelData;
                }
            }
        }

        private string[] ParseCSVLine(string line)
        {
            List<string> result = new List<string>();
            bool inQuotes = false;
            string currentField = "";
            
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(currentField.Trim());
                    currentField = "";
                }
                else
                {
                    currentField += c;
                }
            }
            
            result.Add(currentField.Trim());
            return result.ToArray();
        }

        public LevelDataFromSheet GetLevelData(int level)
        {
            if (!isDataLoaded)
            {
                Debug.LogWarning("Google Sheets data not loaded yet. Call LoadSheetData() first.");
                return null;
            }
            
            return levelDataCache.TryGetValue(level, out LevelDataFromSheet data) ? data : null;
        }

        public List<LevelDataFromSheet> GetAllLevelData()
        {
            if (!isDataLoaded)
            {
                Debug.LogWarning("Google Sheets data not loaded yet. Call LoadSheetData() first.");
                return new List<LevelDataFromSheet>();
            }
            
            return levelDataCache.Values.OrderBy(x => x.level).ToList();
        }

        public bool IsDataLoaded => isDataLoaded;
        
        public int GetTotalLevels()
        {
            return levelDataCache.Count;
        }

        public void ClearCache()
        {
            levelDataCache.Clear();
            isDataLoaded = false;
        }
    }
} 