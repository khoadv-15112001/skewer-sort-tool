using UnityEngine;
using UnityEditor;
using I2.Loc; // Make sure you have the I2 Localization namespace
using System.Collections.Generic;

public class LocalizeObjectSelectorWindow : EditorWindow
{
    private List<GameObject> localizeObjects = new List<GameObject>();
    private Vector2 scrollPosition;

    [MenuItem("Tools/Localize Object Selector")]
    public static void ShowWindow()
    {
        GetWindow<LocalizeObjectSelectorWindow>("Localize Object Selector");
    }

    private void OnEnable()
    {
        RefreshLocalizeObjects();
    }

    private void OnGUI()
    {
        GUILayout.Label("I2.Loc.Localize Objects", EditorStyles.boldLabel);

        if (GUILayout.Button("Refresh List"))
        {
            RefreshLocalizeObjects();
        }

        if (GUILayout.Button("Select All"))
        {
            Selection.objects = localizeObjects.ToArray();
            Debug.Log($"Selected {localizeObjects.Count} GameObjects with I2.Loc.Localize component.");
        }

        EditorGUILayout.Space();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        foreach (GameObject obj in localizeObjects)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(obj, typeof(GameObject), true);
            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                Selection.activeGameObject = obj;
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }

    private void RefreshLocalizeObjects()
    {
        localizeObjects.Clear();
        Localize[] localizeComponents = Object.FindObjectsOfType<Localize>(true);
        foreach (Localize localize in localizeComponents)
        {
            localizeObjects.Add(localize.gameObject);
        }
    }
}