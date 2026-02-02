# Item ID Swap Tool

This tool allows you to automatically swap item IDs in levels based on a mapping defined in a Google Sheet. The tool loads the mapping data from the Google Sheet and applies it when loading levels in the Unity tool panel.

## Features

- **Automatic ID Swapping**: When loading a level, the tool automatically swaps old item IDs to new IDs based on the mapping
- **Bidirectional Swapping**: Support for both forward (old → new) and reverse (new → old) ID swapping
- **Google Sheets Integration**: Loads mapping data directly from a Google Sheet
- **Comprehensive Coverage**: Swaps IDs in all item types (grid items, trays, vending machines, hidden items, frozen items, ice items, boom items, octo chef items)
- **UI Integration**: Integrated with the existing UIToolPanel for seamless workflow

## Setup

### 1. Google Sheet Structure

The tool expects a Google Sheet with the following structure:
- **Column A**: Old ID (integer)
- **Column B**: New ID (integer)
- **Header Row**: "Old Id", "New Id"

Example:
```
| Old Id | New Id |
|--------|--------|
| 1      | 101    |
| 2      | 102    |
| 3      | 103    |
| 4      | 104    |
| 5      | 105    |
```

### 2. Unity Setup

1. **Add ItemIdSwapService to Scene**:
   - Create an empty GameObject in your scene
   - Add the `ItemIdSwapService` component to it
   - The service will automatically set itself up as a singleton

2. **Add UIToolItemIdSwap UI** (Optional):
   - Create UI buttons for the swapping functionality
   - Add the `UIToolItemIdSwap` component to manage the UI
   - Assign the button references in the inspector

3. **Integration with UIToolPanel**:
   - The tool is already integrated into the `LoadLevel()` method
   - Item ID swapping happens automatically when loading levels

## Usage

### Automatic Swapping (Recommended)

The tool automatically applies item ID swapping when you load a level using the `LoadLevel()` method in `UIToolPanel`. This happens transparently without any additional steps.

### Manual Control

If you want manual control over the swapping process, you can use these methods:

```csharp
// Load the mapping data from Google Sheets
ItemIdSwapService.Instance.LoadIdMappingData();

// Apply swapping to current level
int swappedCount = ItemIdSwapHelper.SwapItemIdsInLevel(levelData);

// Reverse swapping (new IDs back to old IDs)
int swappedCount = ItemIdSwapHelper.ReverseSwapItemIdsInLevel(levelData);

// Get mapping summary
string summary = ItemIdSwapHelper.GetMappingSummary();
```

### UI Methods

The `UIToolPanel` provides these public methods for UI integration:

- `LoadItemIdMappingData()`: Load mapping data from Google Sheets
- `ApplyItemIdSwappingToCurrentLevel()`: Apply swapping to current level
- `ReverseItemIdSwappingToCurrentLevel()`: Apply reverse swapping
- `ShowItemIdMappingSummary()`: Display mapping summary

## How It Works

### 1. Data Loading

The `ItemIdSwapService` fetches CSV data from the Google Sheet and parses it into two dictionaries:
- `oldToNewIdMapping`: Maps old IDs to new IDs
- `newToOldIdMapping`: Maps new IDs to old IDs

### 2. Level Processing

When a level is loaded, the `ItemIdSwapHelper.SwapItemIdsInLevel()` method:
1. Iterates through all items in the level data
2. Checks if each item ID has a mapping
3. Replaces the old ID with the new ID if a mapping exists
4. Logs the changes for debugging

### 3. Item Types Covered

The tool processes these item types:
- **Grid Items**: Items placed on the main game grid
- **Tray Items**: Items in trays
- **Vending Machine Items**: Items in vending machines
- **Hidden Items**: Hidden items in the level
- **Frozen Items**: Items that are frozen
- **Ice Items**: Items covered in ice
- **Boom Items**: Explosive items
- **Octo Chef Items**: Special octo chef items

## Configuration

### Google Sheet URL

The tool uses this Google Sheet URL:
```
https://docs.google.com/spreadsheets/d/1oo_Wp5MOUnvOAh-C0WNR7edQBpW0uklENg0fAORuJzU/edit?gid=1619502038#gid=1619502038
```

To change the URL, modify the `SHEET_URL` and `EXPORT_URL` constants in `ItemIdSwapService.cs`.

### Auto-Loading

The `UIToolItemIdSwap` component has an `autoLoadOnStart` option that automatically loads mapping data when the component starts.

## Error Handling

The tool includes comprehensive error handling:
- Network errors when fetching Google Sheet data
- CSV parsing errors
- Missing or invalid mapping data
- Service availability checks

All errors are logged to the Unity console and can be displayed in the UI.

## Debugging

The tool provides extensive logging:
- Mapping data loading status
- Individual item swaps with old and new IDs
- Total number of items swapped
- Error messages and warnings

Check the Unity Console for detailed information about the swapping process.

## Performance

- **Efficient Lookup**: Uses dictionaries for O(1) ID lookup
- **Minimal Overhead**: Only processes items that have mappings
- **Memory Efficient**: Clears event subscriptions to prevent memory leaks

## Troubleshooting

### Common Issues

1. **"ItemIdSwapService not available"**
   - Make sure the `ItemIdSwapService` component is added to a GameObject in the scene

2. **"Mapping data not loaded"**
   - Check your internet connection
   - Verify the Google Sheet URL is correct
   - Ensure the Google Sheet is publicly accessible

3. **"No items swapped"**
   - Check that your mapping data contains the item IDs present in the level
   - Verify the Google Sheet format is correct

4. **Items not updating visually**
   - Call `GenerateLevel()` after applying swapping to refresh the visual representation

### Debug Steps

1. Check the Unity Console for error messages
2. Verify the Google Sheet is accessible and has the correct format
3. Test with a simple mapping (e.g., 1 → 101) to ensure the tool works
4. Check that the level data contains items with the old IDs

## API Reference

### ItemIdSwapService

```csharp
public class ItemIdSwapService : MonoBehaviour
{
    // Events
    public event Action OnDataLoaded;
    public event Action<string> OnError;
    
    // Properties
    public static ItemIdSwapService Instance { get; }
    public bool IsDataLoaded { get; }
    
    // Methods
    public void LoadIdMappingData();
    public int GetNewId(int oldId);
    public int GetOldId(int newId);
    public bool HasMapping(int id);
    public int GetTotalMappings();
    public Dictionary<int, int> GetAllOldToNewMappings();
}
```

### ItemIdSwapHelper

```csharp
public static class ItemIdSwapHelper
{
    public static int SwapItemIdsInLevel(LevelData levelData, ItemIdSwapService swapService = null);
    public static int ReverseSwapItemIdsInLevel(LevelData levelData, ItemIdSwapService swapService = null);
    public static string GetMappingSummary(ItemIdSwapService swapService = null);
}
```

## Example Usage

```csharp
// Load mapping data
ItemIdSwapService.Instance.LoadIdMappingData();

// Wait for data to load (or use events)
if (ItemIdSwapService.Instance.IsDataLoaded)
{
    // Load a level (swapping happens automatically)
    UIToolPanel.Instance.LoadLevel();
    
    // Or apply swapping manually
    int swapped = ItemIdSwapHelper.SwapItemIdsInLevel(levelData);
    Debug.Log($"Swapped {swapped} items");
}
```

This tool provides a powerful and flexible way to manage item ID mappings across your game levels, making it easy to update item IDs without manually editing each level file. 