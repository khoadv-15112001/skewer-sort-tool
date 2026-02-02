using UnityEngine;
using UnityEditor;

public class ResourcesNavigatorWindow : EditorWindow
{
    [MenuItem("Tools/Resources Navigator")]
    public static void ShowWindow()
    {
        GetWindow<ResourcesNavigatorWindow>("Resources Navigator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Resources Folder Navigator", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Go to Resources Folder", GUILayout.Height(40)))
        {
            ForceToResourcesFolder();
        }

        // GUILayout.Space(10);
        //
        // if (GUILayout.Button("Create Resources Folder", GUILayout.Height(30)))
        // {
        //     CreateResourcesFolder();
        // }
    }

    private void ForceToResourcesFolder()
    {
        string resourcesPath = "Assets/Resources";
       
        if (!AssetDatabase.IsValidFolder(resourcesPath))
        {
            Debug.LogWarning("Resources folder doesn't exist! Creating one...");
            CreateResourcesFolder();
            return;
        }
       
        // Focus đến thư mục Resources
        Object resourcesFolder = AssetDatabase.LoadAssetAtPath(resourcesPath, typeof(Object));
        Selection.activeObject = resourcesFolder;
        EditorGUIUtility.PingObject(resourcesFolder);
        EditorUtility.FocusProjectWindow();
       
        Debug.Log("Navigated to Resources folder!");
    }

    private void CreateResourcesFolder()
    {
        string resourcesPath = "Assets/Resources";
       
        if (!AssetDatabase.IsValidFolder(resourcesPath))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
            AssetDatabase.Refresh();
            Debug.Log("Resources folder created successfully!");
        }
        else
        {
            Debug.Log("Resources folder already exists!");
        }
    }
}