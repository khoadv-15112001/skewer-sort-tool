# ToolGrill Selection System

This system provides comprehensive multi-selection functionality for `ToolGrill` objects in your Unity scene.

## Features

- **Single-click selection**: Click on individual grills to select them
- **Multi-selection**: Hold Ctrl/Shift while clicking to add/remove grills from selection
- **Drag selection**: Click and drag to create a selection box around multiple grills
- **Visual feedback**: Selection box drawn around selected grills
- **Keyboard shortcuts**: Various operations on selected grills

## Setup

### 1. Add ToolGrillSelector to your scene

1. Create an empty GameObject in your scene
2. Add the `ToolGrillSelector` component to it
3. Configure the settings in the inspector:
   - **Selectable Layer Mask**: Set to the layer your grills are on
   - **Drag Threshold**: Minimum distance to start drag selection (default: 5 pixels)
   - **Visual Feedback Colors**: Customize selection box appearance
   - **Line Renderer**: Assign a LineRenderer for the selection box (optional)

### 2. Ensure ToolGrill objects have Collider2D

Make sure all `ToolGrill` objects have a `Collider2D` component so they can be detected by the selection system.

## Usage

### Basic Selection

- **Single click**: Select one grill
- **Ctrl/Shift + Click**: Add/remove grill from selection
- **Click and drag**: Create selection box to select multiple grills
- **Right click or Escape**: Clear selection

### Visual Feedback

- Selected grills will have a blue selection box drawn around them
- During drag selection, a semi-transparent blue box shows the selection area
- Selection state is logged to the console for debugging

### Example Operations

The `ToolGrillSelectionExample` script demonstrates common operations:

- **Delete**: Press Delete key to remove selected grills
- **Duplicate**: Press D key to duplicate selected grills
- **Move**: Press M key to move selected grills up by 1 unit

## API Reference

### ToolGrillSelector

#### Properties
- `List<ToolGrill> SelectedGrills`: Get currently selected grills

#### Methods
- `void ClearSelection()`: Clear all selections
- `void SelectGrill(ToolGrill grill)`: Select a specific grill
- `void DeselectGrill(ToolGrill grill)`: Deselect a specific grill
- `bool IsGrillSelected(ToolGrill grill)`: Check if a grill is selected

### Example Usage

```csharp
// Get the selector
ToolGrillSelector selector = FindObjectOfType<ToolGrillSelector>();

// Get selected grills
List<ToolGrill> selected = selector.SelectedGrills;

// Perform operations on selected grills
foreach (ToolGrill grill in selected)
{
    // Your custom logic here
    Debug.Log($"Processing grill: {grill.name}");
}

// Clear selection
selector.ClearSelection();

// Select specific grills
selector.SelectGrill(someGrill);
```

## Customization

### Visual Feedback

You can customize the visual feedback by modifying the `UpdateGrillVisual` method in `ToolGrillSelector`:

```csharp
private void UpdateGrillVisual(ToolGrill grill, bool isSelected)
{
    if (grill == null) return;
    
    // Example: Change material color
    Renderer renderer = grill.GetComponent<Renderer>();
    if (renderer != null)
    {
        if (isSelected)
        {
            renderer.material.color = selectedGrillHighlightColor;
        }
        else
        {
            renderer.material.color = Color.white; // Reset to original
        }
    }
    
    // Example: Scale the grill
    if (isSelected)
    {
        grill.transform.localScale = Vector3.one * 1.1f;
    }
    else
    {
        grill.transform.localScale = Vector3.one;
    }
}
```

### Selection Criteria

You can implement custom selection logic by extending the system:

```csharp
// Select all grills in a specific area
public void SelectGrillsInArea(Vector3 center, float radius)
{
    grillSelector.ClearSelection();
    
    foreach (ToolGrill grill in ToolManager.Instance.AllGrills)
    {
        if (grill != null)
        {
            float distance = Vector3.Distance(grill.transform.position, center);
            if (distance <= radius)
            {
                grillSelector.SelectGrill(grill);
            }
        }
    }
}

// Select grills by type
public void SelectGrillsByType(GrillType type)
{
    grillSelector.ClearSelection();
    
    foreach (ToolGrill grill in ToolManager.Instance.AllGrills)
    {
        if (grill != null && grill.GrillData != null && grill.GrillData.type == type)
        {
            grillSelector.SelectGrill(grill);
        }
    }
}
```

## Integration with Existing Systems

The selection system is designed to work alongside existing systems:

- **ToolManager**: Uses `ToolManager.Instance.AllGrills` to find selectable objects
- **ToolGrill**: Modified to not interfere with selection when Ctrl/Shift is held
- **Existing UI**: Can be integrated with existing UI panels and tools

## Troubleshooting

### Grills not selectable
- Ensure grills have `Collider2D` components
- Check the `Selectable Layer Mask` setting
- Verify grills are on the correct layer

### Selection box not appearing
- Check if `LineRenderer` is assigned in the inspector
- Ensure the `ToolGrillSelector` GameObject is active
- Verify the camera reference is set correctly

### Performance issues
- Limit the number of grills in the scene
- Consider using object pooling for large numbers of grills
- Optimize the `UpdateDragSelectedObjects` method if needed

## Future Enhancements

Potential improvements for the selection system:

- **Group operations**: Save and restore selection groups
- **Advanced filtering**: Select by properties, tags, or custom criteria
- **Undo/Redo**: Integration with the existing undo system
- **Keyboard shortcuts**: More customizable hotkeys
- **Visual effects**: Particle effects, animations for selection feedback 