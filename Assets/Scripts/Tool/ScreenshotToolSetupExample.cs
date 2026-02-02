using UnityEngine;

namespace Tool
{
    /// <summary>
    /// Example script showing how to set up the LevelScreenshotTool in a scene
    /// This is for reference only - you don't need to use this script
    /// </summary>
    public class ScreenshotToolSetupExample : MonoBehaviour
    {
        [Header("Setup Instructions")]
        [TextArea(5, 10)]
        [SerializeField] private string setupInstructions = @"
SETUP INSTRUCTIONS:

1. Create an empty GameObject in your scene
2. Add the LevelScreenshotTool component to it
3. Configure the screenshot settings in the inspector:
   - Screenshot Width/Height (default: 1920x1080)
   - Screenshot Folder Name (default: 'Screenshots')
   - Camera Position (default: 0, 2, -10)
   - Camera Size (default: 15)
4. In the UIToolPanel inspector, assign the LevelScreenshotTool to the 'Screenshot Tool' field
5. The tool will automatically take screenshots when saving levels

OPTIONAL:
- Assign a custom camera for screenshots
- Adjust camera position and size for better framing
- Enable/disable UI inclusion in screenshots
";

        [Header("Example Usage")]
        [SerializeField] private LevelScreenshotTool screenshotTool;
        [SerializeField] private UIToolPanel toolPanel;

        private void Start()
        {
            // Example of how to find and assign the screenshot tool
            if (screenshotTool == null)
            {
                screenshotTool = FindObjectOfType<LevelScreenshotTool>();
            }

            if (toolPanel == null)
            {
                toolPanel = FindObjectOfType<UIToolPanel>();
            }

            // Example of how to manually take a screenshot
            if (screenshotTool != null)
            {
                Debug.Log("Screenshot tool found and ready to use");
            }
            else
            {
                Debug.LogWarning("Screenshot tool not found. Please add LevelScreenshotTool component to a GameObject in the scene.");
            }
        }

        /// <summary>
        /// Example method showing how to take a manual screenshot
        /// </summary>
        [ContextMenu("Take Manual Screenshot")]
        public void TakeManualScreenshot()
        {
            if (screenshotTool != null)
            {
                string screenshotPath = screenshotTool.TakeScreenshotNow("ManualScreenshot.png");
                if (!string.IsNullOrEmpty(screenshotPath))
                {
                    Debug.Log($"Manual screenshot saved: {screenshotPath}");
                }
            }
            else
            {
                Debug.LogError("Screenshot tool not assigned!");
            }
        }

        /// <summary>
        /// Example method showing how to take a level screenshot
        /// </summary>
        [ContextMenu("Take Level Screenshot")]
        public void TakeLevelScreenshot()
        {
            if (screenshotTool != null && toolPanel != null)
            {
                // This would normally be called from UIToolPanel when saving a level
                string screenshotPath = screenshotTool.TakeLevelScreenshot(1, "Food");
                if (!string.IsNullOrEmpty(screenshotPath))
                {
                    Debug.Log($"Level screenshot saved: {screenshotPath}");
                }
            }
            else
            {
                Debug.LogError("Screenshot tool or tool panel not assigned!");
            }
        }
    }
}
