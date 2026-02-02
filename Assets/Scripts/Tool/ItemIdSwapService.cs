using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Tool
{
    public class ItemIdSwapService : MonoBehaviour
    {
        private const string SHEET_URL = "https://docs.google.com/spreadsheets/d/1oo_Wp5MOUnvOAh-C0WNR7edQBpW0uklENg0fAORuJzU/edit?gid=1619502038#gid=1619502038";
        private const string EXPORT_URL = "https://docs.google.com/spreadsheets/d/1oo_Wp5MOUnvOAh-C0WNR7edQBpW0uklENg0fAORuJzU/export?format=csv&gid=1619502038";
        
        private Dictionary<int, List<int>> oldToNewIdMapping = new Dictionary<int, List<int>>();
        private Dictionary<int, int> newToOldIdMapping = new Dictionary<int, int>();
        private bool isDataLoaded = false;
        
        public static ItemIdSwapService Instance { get; private set; }
        
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

        public void LoadIdMappingData()
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
                }
                else
                {
                    string errorMessage = $"Failed to load ID mapping data: {request.error}";
                    UnityEngine.Debug.LogError(errorMessage);
                    OnError?.Invoke(errorMessage);
                }
            }
        }

        private void ParseCSVData(string csvData)
        {
            try
            {
                string[] lines = csvData.Split('\n');
                
                // Skip header row (Old Id, New Id)
                for (int i = 1; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    string[] rowData = ParseCSVLine(line);
                    if (rowData.Length >= 2)
                    {
                        if (int.TryParse(rowData[0], out int oldId) && int.TryParse(rowData[1], out int newId))
                        {
                            // Add to oldToNewIdMapping (supporting multiple new IDs per old ID)
                            if (!oldToNewIdMapping.ContainsKey(oldId))
                            {
                                oldToNewIdMapping[oldId] = new List<int>();
                            }
                            oldToNewIdMapping[oldId].Add(newId);
                            
                            // Add to newToOldIdMapping (reverse mapping)
                            newToOldIdMapping[newId] = oldId;
                            
                            UnityEngine.Debug.Log($"Mapped Old ID {oldId} -> New ID {newId}");
                        }
                    }
                }

                isDataLoaded = true;
                UnityEngine.Debug.Log($"ID mapping loaded successfully. Total mappings: {oldToNewIdMapping.Count}");
                OnDataLoaded?.Invoke();
            }
            catch (Exception e)
            {
                string errorMessage = $"Error parsing ID mapping data: {e.Message}";
                UnityEngine.Debug.LogError(errorMessage);
                OnError?.Invoke(errorMessage);
            }
        }

        private string[] ParseCSVLine(string line)
        {
            List<string> fields = new List<string>();
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
                    fields.Add(currentField.Trim());
                    currentField = "";
                }
                else
                {
                    currentField += c;
                }
            }

            fields.Add(currentField.Trim());
            return fields.ToArray();
        }

        public int GetNewId(int oldId)
        {
            if (oldToNewIdMapping.TryGetValue(oldId, out List<int> newIds))
            {
                if (newIds.Count > 0)
                {
                    // Randomly select one of the available new IDs
                    int randomIndex = UnityEngine.Random.Range(0, newIds.Count);
                    return newIds[randomIndex];
                }
            }
            return oldId; // Return original ID if no mapping found
        }

        public int GetOldId(int newId)
        {
            if (newToOldIdMapping.TryGetValue(newId, out int oldId))
            {
                return oldId;
            }
            return newId; // Return original ID if no mapping found
        }

        public bool HasMapping(int id)
        {
            return (oldToNewIdMapping.ContainsKey(id) && oldToNewIdMapping[id].Count > 0) || newToOldIdMapping.ContainsKey(id);
        }

        public bool IsDataLoaded => isDataLoaded;

        public int GetTotalMappings()
        {
            return oldToNewIdMapping.Count;
        }

        public int GetTotalNewIds()
        {
            int total = 0;
            foreach (var mapping in oldToNewIdMapping.Values)
            {
                total += mapping.Count;
            }
            return total;
        }

        public void ClearMappings()
        {
            oldToNewIdMapping.Clear();
            newToOldIdMapping.Clear();
            isDataLoaded = false;
        }

        public Dictionary<int, List<int>> GetAllOldToNewMappings()
        {
            return new Dictionary<int, List<int>>(oldToNewIdMapping);
        }

        public Dictionary<int, int> GetRandomOldToNewMappings()
        {
            Dictionary<int, int> result = new Dictionary<int, int>();
            foreach (var kvp in oldToNewIdMapping)
            {
                if (kvp.Value.Count > 0)
                {
                    int randomIndex = UnityEngine.Random.Range(0, kvp.Value.Count);
                    result[kvp.Key] = kvp.Value[randomIndex];
                }
            }
            return result;
        }

        public Dictionary<int, int> GetAllNewToOldMappings()
        {
            return new Dictionary<int, int>(newToOldIdMapping);
        }
    }
} 