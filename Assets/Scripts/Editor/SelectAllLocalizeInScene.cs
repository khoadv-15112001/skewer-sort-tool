using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using UnityEditor;
using UnityEngine;

public static class SelectAllLocalizeInScene
{
    [MenuItem("Tools/Select All I2.Loc.Localize Objects")]
    public static void SelectAllLocalizeComponents()
    {
        // Find all Localize components in the scene (including inactive)
        Localize[] localizeComponents = Object.FindObjectsOfType<Localize>(true);

        // Collect their GameObjects
        GameObject[] gameObjects = new GameObject[localizeComponents.Length];
        for (int i = 0; i < localizeComponents.Length; i++)
        {
            gameObjects[i] = localizeComponents[i].gameObject;
        }

        // Select them in the editor
        Selection.objects = gameObjects;

        Debug.Log($"Selected {gameObjects.Length} GameObjects with I2.Loc.Localize component.");
    }
}
