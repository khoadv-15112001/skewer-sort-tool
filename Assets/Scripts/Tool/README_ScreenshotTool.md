# Level Screenshot Tool

This tool automatically takes screenshots when saving levels and saves them in a `Screenshots` folder within the current level folder.

## Features

- **Automatic Screenshots**: Takes a screenshot every time you save a level
- **Customizable Settings**: Configurable resolution, camera position, and screenshot quality
- **Organized Storage**: Screenshots are saved in a dedicated folder within each level folder
- **Timestamped Filenames**: Each screenshot includes the level number and timestamp
- **Manual Screenshot**: Option to take screenshots manually

## Setup

### 1. Add the Screenshot Tool Component

1. Create an empty GameObject in your scene
2. Add the `LevelScreenshotTool` component to it
3. Configure the settings in the inspector

### 2. Connect to UIToolPanel

1. In the `UIToolPanel` inspector, assign the `LevelScreenshotTool` to the `Screenshot Tool` field
2. The tool will automatically take screenshots when saving levels

## Configuration

### Screenshot Settings

- **Screenshot Width/Height**: Resolution of the screenshot (default: 1920x1080)
- **Screenshot Folder Name**: Name of the folder where screenshots are saved (default: "Screenshots")
- **Include UI**: Whether to include UI elements in the screenshot (default: false)
- **Hide All UI**: Whether to hide all UI canvases when taking screenshots (default: true)
- **UI Layers To Hide**: Specific UI layers to hide when Include UI is false and Hide All UI is false
- **Use Custom Camera**: Whether to use a custom camera for screenshots

### Camera Settings

- **Screenshot Camera**: Optional custom camera for taking screenshots
- **Camera Position**: Position of the camera when taking screenshots
- **Camera Size**: Orthographic size for the camera (affects zoom level)

## Usage

### Automatic Screenshots

Screenshots are automatically taken when you save a level using the "Save Level" button in the tool panel. The screenshot will be saved in:

```
Assets/Resources/LevelData/[LevelFolder]/Screenshots/Level_XXXX_YYYYMMDD_HHMMSS.png
```

### Manual Screenshots

You can also take screenshots manually by calling the `TakeScreenshotNow()` method from the `UIToolPanel`.

### UI Hiding

The tool automatically hides UI elements when taking screenshots to ensure clean level previews:

- **Include UI = false, Hide All UI = true**: Hides all Canvas components (default)
- **Include UI = false, Hide All UI = false**: Hides only specific UI layers defined in "UI Layers To Hide"
- **Include UI = true**: Shows all UI elements in screenshots

This ensures that your level screenshots show only the gameplay elements without UI clutter.

## File Structure

```
Assets/
├── Resources/
│   └── LevelData/
│       └── [LevelFolder]/
│           ├── Screenshots/
│           │   ├── Level_0001_20231201_143022.png
│           │   ├── Level_0002_20231201_143045.png
│           │   └── ...
│           └── [Level Files]
```

## Screenshot Naming Convention

Screenshots are named using the following format:
- `Level_XXXX_YYYYMMDD_HHMMSS.png`
- Where:
  - `XXXX` = Level number (padded with zeros)
  - `YYYYMMDD` = Date (year, month, day)
  - `HHMMSS` = Time (hour, minute, second)

## Troubleshooting

### Screenshot Tool Not Found

If you see the warning "LevelScreenshotTool not found", make sure:
1. The `LevelScreenshotTool` component is added to a GameObject in the scene
2. The component is assigned to the `Screenshot Tool` field in `UIToolPanel`

### Screenshots Not Saving

Check the following:
1. Ensure the level folder path is correct
2. Verify that the application has write permissions to the target directory
3. Check the Unity console for error messages

### Poor Screenshot Quality

To improve screenshot quality:
1. Increase the screenshot width and height
2. Adjust the camera position and size for better framing
3. Ensure the scene is properly lit

## API Reference

### LevelScreenshotTool

#### Methods

- `TakeLevelScreenshot(int levelNumber, string levelFolder, SonatLevelService levelService)`
  - Takes a screenshot of the current level and saves it to the level folder
  - Returns the path to the saved screenshot

- `TakeScreenshotNow(string filename = null)`
  - Takes a screenshot immediately using current settings
  - Optional custom filename parameter

### UIToolPanel

#### Methods

- `TakeScreenshotNow()`
  - Public method to take a screenshot manually
  - Shows a notification with the result

## Example Usage

```csharp
// Take a screenshot of level 5
string screenshotPath = screenshotTool.TakeLevelScreenshot(5, "Food", levelService);

// Take a manual screenshot
string manualScreenshotPath = screenshotTool.TakeScreenshotNow("CustomScreenshot.png");
```

## Notes

- Screenshots are taken after the level data is saved
- The tool automatically creates the Screenshots folder if it doesn't exist
- Original camera settings are restored after taking screenshots
- Screenshots are saved in PNG format for best quality
