using Google.Apis.Sheets.v4;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;

public static class GoogleSheetController
{
    public static string spreadsheetId = "17HQ79MBcANdHdZnYE_igNEokUrJFT-PvdTTlTXb-4_c";
    public static string sheetName = "AutoLocalize"; // Name of the sheet tab
    public static string credentialsFilePath = "autotranslatesheet-key.json"; // Path to your OAuth credentials in streaming assets

    private static SheetsService sheetsService;

    public static void Start()
    {
        InitializeService();
        //UploadTerm("en", "Start Game");
    }

    private static void InitializeService()
    {
        try
        {
            string credPath = Path.Combine(Application.streamingAssetsPath, credentialsFilePath);

            GoogleCredential credential = GoogleCredential.FromFile(credPath)
                .CreateScoped(SheetsService.Scope.Spreadsheets);

            sheetsService = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = Application.productName
            });

            Debug.Log("Google Sheets service initialized successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error initializing Google Sheets service: {e.Message}");
        }
    }

    public static async UniTask UploadTerm(string language, string term, string translation = null)
    {
        if (sheetsService == null)
        {
            InitializeService();
            if (sheetsService == null)
            {
                Debug.LogError("Could not initialize Google Sheets service");
                return;
            }
        }

        try
        {
            // Verify the spreadsheet and get actual sheet names
            var spreadsheet = await sheetsService.Spreadsheets.Get(spreadsheetId).ExecuteAsync();
            if (spreadsheet == null || spreadsheet.Sheets == null || spreadsheet.Sheets.Count == 0)
            {
                Debug.LogError($"Could not access spreadsheet with ID {spreadsheetId}");
                return;
            }

            // Check if the sheet exists, otherwise use the first sheet
            string actualSheetName = sheetName;
            bool sheetFound = false;

            foreach (var sheet in spreadsheet.Sheets)
            {
                Debug.Log($"Found sheet: {sheet.Properties.Title}");
                if (sheet.Properties.Title == sheetName)
                {
                    sheetFound = true;
                    break;
                }
            }

            if (!sheetFound)
            {
                actualSheetName = spreadsheet.Sheets[0].Properties.Title;
                Debug.LogWarning($"Sheet '{sheetName}' not found, using '{actualSheetName}' instead");
            }

            // If translation is null, we're uploading a term in the source language
            bool isSourceLanguage = translation == null;
            string valueToUpload = isSourceLanguage ? term : translation;

            // First, find the language column index
            var columnRequest = sheetsService.Spreadsheets.Values.Get(
                spreadsheetId, $"'{actualSheetName}'!1:1");
            var columnResponse = await columnRequest.ExecuteAsync();

            int languageColumnIndex = -1;

            if (columnResponse.Values == null || columnResponse.Values.Count == 0)
            {
                // Create header row if it doesn't exist
                var headerValueRange = new Google.Apis.Sheets.v4.Data.ValueRange
                {
                    Values = new List<IList<object>> { new List<object> { "Term", "English", language } }
                };

                var headerRequest = sheetsService.Spreadsheets.Values.Update(
                    headerValueRange, spreadsheetId, $"'{actualSheetName}'!A1:C1");
                headerRequest.ValueInputOption =
                    SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
                await headerRequest.ExecuteAsync();

                // Set languageColumnIndex based on our new header
                languageColumnIndex = language.ToLower() == "english" ? 1 : 2;
            }
            else
            {
                var headerRow = columnResponse.Values[0];

                for (int i = 0; i < headerRow.Count; i++)
                {
                    if (headerRow[i].ToString().ToLower() == language.ToLower())
                    {
                        languageColumnIndex = i;
                        break;
                    }
                }

                if (languageColumnIndex == -1)
                {
                    Debug.LogError($"Language column '{language}' not found in the sheet");
                    return;
                }
            }

            // Find the term row or get the next empty row
            int termRowIndex = -1;

            // Get all the terms in the first column
            var termsRequest = sheetsService.Spreadsheets.Values.Get(
                spreadsheetId, $"'{actualSheetName}'!A:A");
            var termsResponse = await termsRequest.ExecuteAsync();

            if (termsResponse.Values != null)
            {
                for (int i = 1; i < termsResponse.Values.Count; i++) // Skip header row
                {
                    if (termsResponse.Values[i].Count > 0 &&
                        termsResponse.Values[i][0].ToString() == term)
                    {
                        termRowIndex = i + 1; // +1 because sheets are 1-indexed
                        break;
                    }
                }
            }

            // If the term doesn't exist yet, add it
            if (termRowIndex == -1)
            {
                termRowIndex = termsResponse.Values != null ?
                    termsResponse.Values.Count + 1 : 2; // +1 for new row, or 2 if empty (1 for header + 1 for new row)

                Debug.Log($"Adding new term '{term}' to sheet at row {termRowIndex}");

                // Add the term to the first column
                var termRange = $"'{actualSheetName}'!A{termRowIndex}";
                var termValueRange = new Google.Apis.Sheets.v4.Data.ValueRange
                {
                    Values = new List<IList<object>> { new List<object> { term } }
                };

                var termUpdateRequest = sheetsService.Spreadsheets.Values.Update(
                    termValueRange, spreadsheetId, termRange);
                termUpdateRequest.ValueInputOption =
                    SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
                await termUpdateRequest.ExecuteAsync();
            }

            // Now update the translation in the proper column
            var cellCoordinate = GetCellCoordinate(languageColumnIndex, termRowIndex);
            var translationRange = $"'{actualSheetName}'!{cellCoordinate}";

            var valueRange = new Google.Apis.Sheets.v4.Data.ValueRange
            {
                Values = new List<IList<object>> { new List<object> { valueToUpload } }
            };

            var updateRequest = sheetsService.Spreadsheets.Values.Update(
                valueRange, spreadsheetId, translationRange);
            updateRequest.ValueInputOption =
                SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;

            var response = await updateRequest.ExecuteAsync();
            Debug.Log($"Updated {response.UpdatedCells} cells at {translationRange}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error uploading term to Google Sheet: {e.Message}\n{e.StackTrace}");
        }
    }

    private static string GetCellCoordinate(int columnIndex, int rowIndex)
    {
        // Convert 0-based column index to A1 notation (A, B, C, ...)
        string columnLetter = "";

        while (columnIndex >= 0)
        {
            columnLetter = (char)('A' + (columnIndex % 26)) + columnLetter;
            columnIndex = (columnIndex / 26) - 1;
        }

        return $"{columnLetter}{rowIndex}";
    }
}
