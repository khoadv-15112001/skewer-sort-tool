using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SonatFramework.Systems.LevelManagement;

namespace Tool
{
    public class LevelScreenshotTool : MonoBehaviour
    {
        [Header("Screenshot Settings")]
        [SerializeField] private int screenshotWidth = 1920;
        [SerializeField] private int screenshotHeight = 1080;
        [SerializeField] private string screenshotFolderName = "Screenshots";
        [SerializeField] private bool includeUI = false;
        [SerializeField] private bool useCustomCamera = true;
        [SerializeField] private bool hideAllUI = true;
        //[SerializeField] private string[] uiLayersToHide = { "UI", "UI_Overlay" };
        [SerializeField] private Canvas mainCanvas;
        
        [Header("Camera Settings")]
        [SerializeField] private Camera screenshotCamera;
        [SerializeField] private Vector3 cameraPosition = new Vector3(0, 2, -10);
        [SerializeField] private float cameraSize = 15f;
        
        private Camera originalCamera;
        private RenderTexture renderTexture;
        private Camera tempCamera;

        private void Awake()
        {
            // Create render texture for screenshots
            renderTexture = new RenderTexture(screenshotWidth, screenshotHeight, 24);
            renderTexture.Create();
        }

        private void OnDestroy()
        {
            if (renderTexture != null)
            {
                renderTexture.Release();
                DestroyImmediate(renderTexture);
            }
        }

        /// <summary>
        /// Takes a screenshot of the current level and saves it to the level folder
        /// </summary>
        /// <param name="levelNumber">The level number</param>
        /// <param name="levelFolder">The level folder path</param>
        /// <param name="levelService">The level service to get the folder path</param>
        /// <returns>Path to the saved screenshot</returns>
        public string TakeLevelScreenshot(int levelNumber, string levelFolder)
        {
            try
            {
                // Get the level folder path
                string levelFolderPath = GetLevelFolderPath(levelFolder);
                if (string.IsNullOrEmpty(levelFolderPath))
                {
                    Debug.LogError("Could not determine level folder path for screenshot");
                    return null;
                }

                // Create screenshots folder
                string screenshotsPath = Path.Combine(levelFolderPath, screenshotFolderName);
                if (!Directory.Exists(screenshotsPath))
                {
                    Directory.CreateDirectory(screenshotsPath);
                }

                // Generate filename
                //string filename = $"Level_{levelNumber:D4}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                string filename = $"Level_{levelNumber:D4}.png";
                string fullPath = Path.Combine(screenshotsPath, filename);

                // Take the screenshot
                Texture2D screenshot = CaptureScreenshot();
                if (screenshot == null)
                {
                    Debug.LogError("Failed to capture screenshot");
                    return null;
                }

                // Save the screenshot
                byte[] bytes = screenshot.EncodeToPNG();
                File.WriteAllBytes(fullPath, bytes);
                
                // Clean up
                DestroyImmediate(screenshot);

                Debug.Log($"Screenshot saved: {fullPath}");
                return fullPath;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error taking screenshot: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Captures a screenshot using the configured camera settings
        /// </summary>
        /// <returns>Texture2D of the screenshot</returns>
        private Texture2D CaptureScreenshot()
        {
            // Store original camera settings
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("No main camera found for screenshot");
                return null;
            }

            originalCamera = mainCamera;
            Vector3 originalPosition = mainCamera.transform.position;
            float originalSize = mainCamera.orthographicSize;
            bool originalEnabled = mainCamera.enabled;

            // Store UI states if we need to hide UI
            List<Canvas> uiCanvases = new List<Canvas>();
            List<bool> originalCanvasStates = new List<bool>();
            List<GameObject> hiddenUIObjects = new List<GameObject>();
            
            if (!includeUI)
            {
                if (hideAllUI)
                {
                    // Find and hide all UI canvases
                    Canvas[] allCanvases = FindObjectsOfType<Canvas>();
                    foreach (Canvas canvas in allCanvases)
                    {
                        uiCanvases.Add(canvas);
                        originalCanvasStates.Add(canvas.enabled);
                        canvas.enabled = false;
                    }
                }
                else
                {
                    // Hide specific UI layers
                    // foreach (string layerName in uiLayersToHide)
                    // {
                    //     int layerMask = LayerMask.NameToLayer(layerName);
                    //     if (layerMask != -1)
                    //     {
                    //         GameObject[] layerObjects = FindObjectsOfType<GameObject>();
                    //         foreach (GameObject obj in layerObjects)
                    //         {
                    //             if (obj.layer == layerMask)
                    //             {
                    //                 hiddenUIObjects.Add(obj);
                    //                 obj.SetActive(false);
                    //             }
                    //         }
                    //     }
                    // }
                    uiCanvases.Add(mainCanvas);
                    originalCanvasStates.Add(mainCanvas.enabled);
                    mainCanvas.enabled = false;
                }
            }

            try
            {
                if (useCustomCamera && screenshotCamera != null)
                {
                    // Use custom camera
                    SetupCustomCamera();
                    return CaptureWithCamera(screenshotCamera);
                }
                else
                {
                    // Use main camera with custom settings
                    SetupMainCamera();
                    return CaptureWithCamera(mainCamera);
                }
            }
            finally
            {
                // Restore original camera settings
                if (mainCamera != null)
                {
                    mainCamera.transform.position = originalPosition;
                    mainCamera.orthographicSize = originalSize;
                    mainCamera.enabled = originalEnabled;
                }

                // Restore UI states if we hid them
                if (!includeUI)
                {
                    // Restore canvas states
                    for (int i = 0; i < uiCanvases.Count; i++)
                    {
                        if (uiCanvases[i] != null)
                        {
                            uiCanvases[i].enabled = originalCanvasStates[i];
                        }
                    }
                    
                    // Restore hidden UI objects
                    foreach (GameObject obj in hiddenUIObjects)
                    {
                        if (obj != null)
                        {
                            obj.SetActive(true);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets up the main camera for screenshot capture
        /// </summary>
        private void SetupMainCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.transform.position = cameraPosition;
                mainCamera.orthographicSize = cameraSize;
                mainCamera.enabled = true;
            }
        }

        /// <summary>
        /// Sets up the custom camera for screenshot capture
        /// </summary>
        private void SetupCustomCamera()
        {
            if (screenshotCamera == null) return;

            // Create temporary camera if needed
            if (tempCamera == null)
            {
                GameObject tempCameraObj = new GameObject("TempScreenshotCamera");
                tempCamera = tempCameraObj.AddComponent<Camera>();
                tempCamera.CopyFrom(screenshotCamera);
            }

            tempCamera.transform.position = cameraPosition;
            tempCamera.orthographicSize = cameraSize;
            tempCamera.targetTexture = renderTexture;
            tempCamera.enabled = true;
        }

        /// <summary>
        /// Captures screenshot using the specified camera
        /// </summary>
        /// <param name="camera">Camera to use for capture</param>
        /// <returns>Texture2D of the screenshot</returns>
        private Texture2D CaptureWithCamera(Camera camera)
        {
            if (camera == null) return null;

            // Set camera target texture
            camera.targetTexture = renderTexture;

            // Render the scene
            camera.Render();

            // Create texture from render texture
            Texture2D screenshot = new Texture2D(screenshotWidth, screenshotHeight, TextureFormat.RGB24, false);
            RenderTexture.active = renderTexture;
            screenshot.ReadPixels(new Rect(0, 0, screenshotWidth, screenshotHeight), 0, 0);
            screenshot.Apply();

            // Reset camera target texture
            camera.targetTexture = null;
            RenderTexture.active = null;

            return screenshot;
        }

        /// <summary>
        /// Gets the level folder path from the level service
        /// </summary>
        /// <param name="levelFolder">Level folder name</param>
        /// <param name="levelService">Level service</param>
        /// <returns>Full path to the level folder</returns>
        private string GetLevelFolderPath(string levelFolder)
        {

            try
            {
                string savePath = string.IsNullOrEmpty(levelFolder) ? "LevelData" : $"LevelData/{levelFolder}";
                
                // Ensure the directory exists
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }
                
                Debug.Log($"Using screenshot folder path: {savePath}");
                return savePath;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error getting level folder path: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Takes a screenshot immediately using current settings
        /// </summary>
        /// <param name="filename">Optional custom filename</param>
        /// <returns>Path to saved screenshot</returns>
        public string TakeScreenshotNow(string filename = null)
        {
            try
            {
                string screenshotsPath = Path.Combine(Application.dataPath, screenshotFolderName);
                if (!Directory.Exists(screenshotsPath))
                {
                    Directory.CreateDirectory(screenshotsPath);
                }

                if (string.IsNullOrEmpty(filename))
                {
                    filename = $"Screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                }

                string fullPath = Path.Combine(screenshotsPath, filename);
                Texture2D screenshot = CaptureScreenshot();
                
                if (screenshot != null)
                {
                    byte[] bytes = screenshot.EncodeToPNG();
                    File.WriteAllBytes(fullPath, bytes);
                    DestroyImmediate(screenshot);
                    
                    Debug.Log($"Screenshot saved: {fullPath}");
                    return fullPath;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error taking screenshot: {ex.Message}");
            }
            
            return null;
        }
    }
}
