using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;

public class ToolChangeVisual : MonoBehaviour
{
    [SerializeField] private Image bgr;
    [SerializeField] private string path;

    private void OnEnable()
    {
        bgr.SetSpriteAsync($"Assets/Art/GameplayBundle/Background.png");
    }

    // Windows API for file dialog
    [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern IntPtr GetOpenFileName([In, Out] OpenFileName ofn);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class OpenFileName
    {
        public int lStructSize = 0;
        public IntPtr hwndOwner = IntPtr.Zero;
        public IntPtr hInstance = IntPtr.Zero;
        public string lpstrFilter = null;
        public string lpstrCustomFilter = null;
        public int nMaxCustFilter = 0;
        public int nFilterIndex = 0;
        public string lpstrFile = null;
        public int nMaxFile = 0;
        public string lpstrFileTitle = null;
        public int nMaxFileTitle = 0;
        public string lpstrInitialDir = null;
        public string lpstrTitle = null;
        public int Flags = 0;
        public short nFileOffset = 0;
        public short nFileExtension = 0;
        public string lpstrDefExt = null;
        public IntPtr lCustData = IntPtr.Zero;
        public IntPtr lpfnHook = IntPtr.Zero;
        public string lpTemplateName = null;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    public void ChangeBGR()
    {
        string selectedPath = "";

#if UNITY_EDITOR
        // Use Unity Editor file dialog
        selectedPath = UnityEditor.EditorUtility.OpenFilePanel("Select Background Image", path, "png,jpg,jpeg,bmp,tga");
#else
        // Use Windows native file dialog at runtime
        selectedPath = OpenFileDialog();
#endif

        if (!string.IsNullOrEmpty(selectedPath))
        {
            // Load the image from the selected path
            Sprite sprite = LoadImageFromPath(selectedPath);
            if (sprite != null) bgr.sprite = sprite;
        }
    }

    public void ChangeGrill()
    {
        string selectedPath = "";

#if UNITY_EDITOR
        // Use Unity Editor file dialog
        selectedPath = UnityEditor.EditorUtility.OpenFilePanel("Select Grill Image", path, "png,jpg,jpeg,bmp,tga");
#else
        // Use Windows native file dialog at runtime
        selectedPath = OpenFileDialog();
#endif

        if (!string.IsNullOrEmpty(selectedPath))
        {
            // Load the image from the selected path
            Sprite sprite = LoadImageFromPath(selectedPath);
            if (sprite != null)
            {
                ToolManager.Instance.ChangeMainGrillVisual(sprite);
            }
        }
    }

    public void ChangeSubGrill()
    {
        string selectedPath = "";

#if UNITY_EDITOR
        // Use Unity Editor file dialog
        selectedPath = UnityEditor.EditorUtility.OpenFilePanel("Select SubGrill Image", path, "png,jpg,jpeg,bmp,tga");
#else
        // Use Windows native file dialog at runtime
        selectedPath = OpenFileDialog();
#endif

        if (!string.IsNullOrEmpty(selectedPath))
        {
            // Load the image from the selected path
            Sprite sprite = LoadImageFromPath(selectedPath);
            if (sprite != null)
            {
                ToolManager.Instance.ChangeSubGrillVisual(sprite);
            }
        }
    }

    public void ChangeMiniGrill()
    {
        string selectedPath = "";

#if UNITY_EDITOR
        // Use Unity Editor file dialog
        selectedPath = UnityEditor.EditorUtility.OpenFilePanel("Select Mini Grill Image", path, "png,jpg,jpeg,bmp,tga");
#else
        // Use Windows native file dialog at runtime
        selectedPath = OpenFileDialog();
#endif

        if (!string.IsNullOrEmpty(selectedPath))
        {
            // Load the image from the selected path
            Sprite sprite = LoadImageFromPath(selectedPath);
            if (sprite != null)
            {
                ToolManager.Instance.ChangeMainGrillVisual(sprite, true);
            }
        }
    }

    private string OpenFileDialog()
    {
        OpenFileName ofn = new OpenFileName();
        ofn.lStructSize = Marshal.SizeOf(ofn);
        ofn.lpstrFilter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp;*.tga)\0*.png;*.jpg;*.jpeg;*.bmp;*.tga\0All Files (*.*)\0*.*\0";
        ofn.lpstrFile = new string(new char[256]);
        ofn.nMaxFile = ofn.lpstrFile.Length;
        ofn.lpstrFileTitle = new string(new char[64]);
        ofn.nMaxFileTitle = ofn.lpstrFileTitle.Length;
        ofn.lpstrTitle = "Select Background Image";
        ofn.lpstrInitialDir = path;
        ofn.Flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000002; // OFN_EXPLORER | OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_HIDEREADONLY

        if (GetOpenFileName(ofn) != IntPtr.Zero)
        {
            return ofn.lpstrFile;
        }

        return "";
    }

    private Sprite LoadImageFromPath(string imagePath)
    {
        // Read the file as bytes
        byte[] fileData = File.ReadAllBytes(imagePath);

        // Create a new texture
        Texture2D texture = new Texture2D(2, 2);

        // Load the image data into the texture
        if (texture.LoadImage(fileData))
        {
            // Create a sprite from the texture
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            // Assign the sprite to the background image
            if (bgr != null)
            {
                return sprite;
                Debug.Log($"Background image changed to: {imagePath}");
            }
            else
            {
                Debug.LogError("Background Image component is not assigned!");
            }
        }
        else
        {
            Debug.LogError($"Failed to load image from: {imagePath}");
        }
        return null;
    }
}
