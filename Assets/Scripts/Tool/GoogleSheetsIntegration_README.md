# Google Sheets Integration for Grill Sort Tool

This integration allows you to load and display level data from a Google Sheets document directly in the Unity tool panel.

## Features

- **Load Data from Google Sheets**: Automatically fetch level data from the specified Google Sheets URL
- **Display Level Information**: Show detailed level data including items, obstacles, locks, etc.
- **Level Navigation**: Navigate between levels using previous/next buttons
- **Data Validation**: Compare current level data with sheet data
- **Integration with Existing Tool**: Works seamlessly with the existing UIToolPanel

## Setup Instructions

### 1. Create the UI Panel

1. Create a new GameObject in your scene and name it "UIToolSheetData"
2. Add the `UIToolSheetData` component to this GameObject
3. Create a UI panel with the following elements:

#### Required UI Elements:
- **Load Data Button** (`btnLoadData`)
- **Refresh Data Button** (`btnRefreshData`)
- **Status Text** (`txtStatus`)
- **Current Level Text** (`txtCurrentLevel`)
- **Total Levels Text** (`txtTotalLevels`)
- **Navigation Buttons** (`btnPreviousLevel`, `btnNextLevel`)

#### Level Data Display Fields:
- Level (`txtLevel`)
- Layout (`txtLayout`)
- Obstacles (`txtObstacles`)
- Items (`txtItems`)
- Drop Line (`txtDropLine`)
- Mini Tray (`txtMiniTray`)
- Lock (`txtLock`)
- Nap Vung (`txtNapVung`)
- Vending Tray (`txtVendingTray`)
- Hidden (`txtHidden`)
- Lock & Key (`txtLockAndKey`)
- Frozen (`txtFrozen`)
- Ice (`txtIce`)
- Boom (`txtBoom`)
- OctoChef (`txtOctoChef`)
- Total IDs (`txtTotalIds`)
- Unique (`txtUnique`)
- Variations (`txtVariations`)
- No Match3 (`txtNoMatch3`)

### 2. Configure UIToolPanel

1. In your existing `UIToolPanel`, assign the `UIToolSheetData` component to the `uiToolSheetData` field
2. Add buttons to your UI to call the following methods:
   - `ShowSheetData()` - Show the sheet data panel
   - `LoadSheetData()` - Load data from Google Sheets
   - `ValidateCurrentLevelWithSheet()` - Validate current level against sheet data
   - `ApplySheetDataToCurrentLevel()` - Apply sheet data to current level

### 3. Google Sheets Setup

The integration is configured to work with the Google Sheets document at:
```
https://docs.google.com/spreadsheets/d/1oo_Wp5MOUnvOAh-C0WNR7edQBpW0uklENg0fAORuJzU/edit?gid=149409390#gid=149409390
```

The sheet should have the following column structure:
- Column A: Level number
- Column B: Layout
- Column C: Obstacles
- Column D: Items
- Column E: Drop Line
- Column F: Mini Tray
- Column G: Lock
- Column H: Nắp vung
- Column I: Vending Tray
- Column J: Hidden
- Column K: Lock & Key
- Column L: Frozen
- Column M: Ice
- Column N: Boom
- Column O: OctoChef
- Column P: Total IDs
- Column Q: Unique
- Column R: Variations
- Column S: No Match3

## Usage

### Loading Data
1. Click the "Load Data" button to fetch data from Google Sheets
2. The system will automatically parse the CSV data and cache it
3. Status will be displayed in the status text field

### Navigating Levels
1. Use the Previous/Next buttons to navigate between levels
2. The current level data will be displayed in the UI fields
3. You can also enter a level number in the main tool panel and it will automatically display the corresponding sheet data

### Integration with Level Loading
When you load a level using the existing `LoadLevel()` function, the system will automatically display the corresponding Google Sheets data if available.

### Validation
Use the validation feature to compare your current level setup with the sheet data:
- Item counts
- Lock counts
- Other relevant metrics

## API Reference

### UIToolSheetData Methods
- `LoadSheetData()` - Load data from Google Sheets
- `DisplayLevelData(int level)` - Display data for a specific level
- `DisplayLevelFromInput(int level)` - Display data for level from input
- `GetCurrentLevelData()` - Get the currently displayed level data
- `IsDataLoaded()` - Check if data has been loaded

### UIToolPanel Methods
- `ShowSheetData()` - Show the sheet data panel
- `LoadSheetData()` - Load data from Google Sheets
- `ValidateCurrentLevelWithSheet()` - Validate current level against sheet data
- `ApplySheetDataToCurrentLevel()` - Apply sheet data to current level
- `DisplaySheetDataForCurrentLevel()` - Display sheet data for current level

### GoogleSheetsIntegrationHelper Methods
- `ApplySheetDataToLevel(sheetData, levelData)` - Apply sheet data to level
- `GetSuggestedGrillTypes(sheetData)` - Get grill type suggestions
- `ValidateLevelAgainstSheet(levelData, sheetData)` - Validate level against sheet
- `GenerateComparisonReport(levelData, sheetData)` - Generate comparison report

## Troubleshooting

### Common Issues

1. **"GoogleSheetsService not found"**
   - Make sure the GoogleSheetsService component is present in the scene
   - The service will be created automatically if not found

2. **"Failed to load Google Sheets data"**
   - Check your internet connection
   - Verify the Google Sheets URL is accessible
   - Ensure the sheet is publicly accessible or properly shared

3. **"Level X not found in sheet data"**
   - Verify the level number exists in the Google Sheets
   - Check that the level number is in the first column (Column A)

4. **Data not displaying correctly**
   - Verify the CSV format matches the expected structure
   - Check that the column headers are in the correct order
   - Ensure numeric fields contain valid numbers

### Debug Information
The system provides detailed debug logs in the Unity Console. Check the console for:
- Data loading status
- Parsing errors
- Validation results
- Integration messages

## Notes

- The Google Sheets must be publicly accessible or properly shared for the integration to work
- Data is cached locally after loading to improve performance
- The system automatically handles CSV parsing and data validation
- Integration works with the existing level loading and saving system 