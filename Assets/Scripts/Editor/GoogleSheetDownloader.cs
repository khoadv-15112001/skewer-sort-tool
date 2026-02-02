#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System;

public class GoogleSheetDownloader : EditorWindow
{
    private string googleSheetUrl = "";
    private string outputFolder = "Assets/Data";
    private string fileName = "data.csv";
    private bool isDownloading = false;
    private string statusMessage = "";

    [MenuItem("Tools/Google Sheet Downloader")]
    public static void ShowWindow()
    {
        GetWindow<GoogleSheetDownloader>("Google Sheet Downloader");
    }

    private void OnGUI()
    {
        GUILayout.Label("Google Sheet CSV Downloader", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Google Sheet URL input
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Google Sheet URL:", GUILayout.Width(120));
        googleSheetUrl = EditorGUILayout.TextField(googleSheetUrl);
        EditorGUILayout.EndHorizontal();

        // Help text for URL format
        EditorGUILayout.HelpBox(
            "Paste your Google Sheet URL here. The tool will automatically convert it to CSV format.\n" +
            "Example: https://docs.google.com/spreadsheets/d/YOUR_SHEET_ID/edit",
            MessageType.Info);

        EditorGUILayout.Space();

        // Output folder selection
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Output Folder:", GUILayout.Width(120));
        outputFolder = EditorGUILayout.TextField(outputFolder);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("Select Output Folder", "Assets", "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                // Convert to relative path if it's within the project
                if (selectedPath.StartsWith(Application.dataPath))
                {
                    outputFolder = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                }
                else
                {
                    outputFolder = selectedPath;
                }
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // File name input
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("File Name:", GUILayout.Width(120));
        fileName = EditorGUILayout.TextField(fileName);
        EditorGUILayout.EndHorizontal();

        // Ensure .csv extension
        if (!fileName.EndsWith(".csv"))
        {
            fileName += ".csv";
        }

        EditorGUILayout.Space();

        // Download button
        GUI.enabled = !isDownloading && !string.IsNullOrEmpty(googleSheetUrl);
        if (GUILayout.Button(isDownloading ? "Downloading..." : "Download CSV", GUILayout.Height(30)))
        {
            DownloadCSV();
        }

        GUI.enabled = true;

        EditorGUILayout.Space();

        // Status message
        if (!string.IsNullOrEmpty(statusMessage))
        {
            EditorGUILayout.HelpBox(statusMessage,
                statusMessage.Contains("Error") ? MessageType.Error : MessageType.Info);
        }

        // Preview of full path
        if (!string.IsNullOrEmpty(outputFolder) && !string.IsNullOrEmpty(fileName))
        {
            string fullPath = Path.Combine(outputFolder, fileName);
            EditorGUILayout.HelpBox($"File will be saved to: {fullPath}", MessageType.None);
        }
    }

    private async void DownloadCSV()
    {
        if (string.IsNullOrEmpty(googleSheetUrl))
        {
            statusMessage = "Error: Please enter a Google Sheet URL";
            return;
        }

        isDownloading = true;
        statusMessage = "Downloading...";

        try
        {
            // Convert Google Sheet URL to CSV export URL
            string csvUrl = ConvertToCSVUrl(googleSheetUrl);

            // Create output directory if it doesn't exist
            string fullOutputPath = Path.Combine(Application.dataPath, outputFolder.Replace("Assets/", ""));
            if (!Directory.Exists(fullOutputPath))
            {
                Directory.CreateDirectory(fullOutputPath);
            }

            // Download the CSV file
            string filePath = Path.Combine(fullOutputPath, fileName);
            await DownloadFileAsync(csvUrl, filePath);

            statusMessage = $"Successfully downloaded CSV to: {filePath}";

            // Refresh the Asset Database to show the new file
            AssetDatabase.Refresh();

            // Select the downloaded file in the Project window
            string assetPath = Path.Combine(outputFolder, fileName);
            UnityEngine.Object downloadedFile = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (downloadedFile != null)
            {
                Selection.activeObject = downloadedFile;
                EditorGUIUtility.PingObject(downloadedFile);
            }
        }
        catch (Exception ex)
        {
            statusMessage = $"Error: {ex.Message}";
            Debug.LogError($"Google Sheet Download Error: {ex}");
        }
        finally
        {
            isDownloading = false;
            Repaint();
        }
    }

    private string ConvertToCSVUrl(string googleSheetUrl)
    {
        // Extract the sheet ID from the Google Sheet URL
        string sheetId = "";

        // Handle different Google Sheet URL formats
        if (googleSheetUrl.Contains("/spreadsheets/d/"))
        {
            int startIndex = googleSheetUrl.IndexOf("/spreadsheets/d/") + "/spreadsheets/d/".Length;
            int endIndex = googleSheetUrl.IndexOf("/", startIndex);
            if (endIndex == -1)
            {
                endIndex = googleSheetUrl.Length;
            }

            sheetId = googleSheetUrl.Substring(startIndex, endIndex - startIndex);
        }
        else if (googleSheetUrl.Contains("?id="))
        {
            int startIndex = googleSheetUrl.IndexOf("?id=") + "?id=".Length;
            int endIndex = googleSheetUrl.IndexOf("&", startIndex);
            if (endIndex == -1)
            {
                endIndex = googleSheetUrl.Length;
            }

            sheetId = googleSheetUrl.Substring(startIndex, endIndex - startIndex);
        }
        else
        {
            throw new ArgumentException("Invalid Google Sheet URL format");
        }

        // Create CSV export URL
        return $"https://drive.google.com/uc?export=download&id={sheetId}";
    }

    private async Task DownloadFileAsync(string url, string filePath)
    {
        using (HttpClient client = new HttpClient())
        {
            // Set timeout
            client.Timeout = TimeSpan.FromSeconds(30);

            // Download the file
            byte[] fileBytes = await client.GetByteArrayAsync(url);

            // Write to file
            File.WriteAllBytes(filePath, fileBytes);
        }
    }
}
#endif