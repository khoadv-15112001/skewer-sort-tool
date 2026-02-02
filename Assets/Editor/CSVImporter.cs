using UnityEditor;
using UnityEngine;
using System.IO;

public abstract class CSVImporter : EditorWindow
{
    // private BattlePassConfig targetSO;
    protected string filePath;

    protected abstract void ImportCSV();

    protected virtual void OnGUI()
    {
        EditorGUILayout.Space();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField("-----CSV FILE-----", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Chọn file CSV"))
        {
            filePath = EditorUtility.OpenFilePanel("Chọn CSV", "", "csv");
        }

        if (!string.IsNullOrEmpty(filePath))
        {
            GUILayout.Label("File: " + filePath);
            if (GUILayout.Button("Import vào SO"))
            {
                ImportCSV();
            }
        }
    }


    protected string[] GetLines()
    {
        return File.ReadAllLines(filePath);
    }

    protected string[] GetValues(string line)
    {
        return line.Split(',');
    }
}