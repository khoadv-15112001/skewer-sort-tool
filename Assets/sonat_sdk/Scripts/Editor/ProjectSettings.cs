// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using System.Reflection;
// using UnityEngine;
// #if UNITY_EDITOR
// using UnityEditor;
//
// #endif
//
// [System.Serializable]
// public class ProjectSettings
// {
//     private const string Path = "Assets/ProjectSettings.json";
//     public string projectSku = "sample_project";
//     public string startSceneName = "Main";
//     public string levelEditorSceneName = "LevelEditor";
//     public string nameSpace = "MergeBlock";
//     public string keystorePassword = "sample_project";
//     public string folder1 = "Assets/Prefabs";
//     public string folder2 = "Assets/Prefabs";
//
//     public bool IsNotCurrent => projectSku == "sample_project";
//
//     public void SaveProjectSettings()
//     {
//         File.WriteAllText(Path, JsonUtility.ToJson(this));
// #if UNITY_EDITOR
//         AssetDatabase.SaveAssets();
//         AssetDatabase.Refresh();
// #endif
//     }
//
//     public static ProjectSettings LoadProjectSettings()
//     {
// #if UNITY_EDITOR
//         if (!File.Exists(Path))
//         {
//             var projectSettings = new ProjectSettings();
//             projectSettings.SaveProjectSettings();
//             return projectSettings;
//         }
//
//         var data = LoadTextAtPath(Path);
//         if (data == null)
//         {
//             var projectSettings = new ProjectSettings();
//             projectSettings.SaveProjectSettings();
//             return projectSettings;
//         }
//
//         return JsonUtility.FromJson<ProjectSettings>(data);
// #else
//         return null;
// #endif
//     }
//
// #if UNITY_EDITOR
//     public static IEnumerable<string> LoadTextAtPaths(string folder, string extensions = "*.json")
//     {
//         var info = new DirectoryInfo(folder);
//         var files = info.GetFiles(extensions)
//             .OrderBy(x => int.Parse(System.IO.Path.GetFileNameWithoutExtension(x.Name)));
//         foreach (var file in files)
//         {
//             StreamReader reader = new StreamReader(file.FullName);
//             yield return reader.ReadToEnd();
//             reader.Close();
//         }
//     }
//
//     public static string LoadTextAtPath(string path)
//     {
//         string readContents;
//         using (StreamReader streamReader = new StreamReader(path))
//         {
//             readContents = streamReader.ReadToEnd();
//         }
//
//         return readContents;
//     }
// #endif
//
//     public static string GetKey(string enumName, int key)
//     {
// #if UNITY_EDITOR
//         var projectSettings = LoadProjectSettings();
//         var type = Type.GetType(projectSettings.nameSpace + "." + enumName);
//
//         if (type == null)
//             type = GetType(projectSettings.nameSpace, enumName);
//
//         if (type != null && type.IsEnum)
//         {
//             var index = ((int[]) Enum.GetValues(type)).ToList().IndexOf(key);
//             if (index >= 0)
//             {
//                 return Enum.GetNames(type)[index];
//             }
//         }
// #endif
//         return string.Empty;
//     }
//
//     public static Type GetType(string nameSpace, string className)
//     {
//         Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
//         foreach (var assembly in assemblies)
//         {
//             var type = Type.GetType($"{nameSpace}.{className},{assembly.GetName().Name}");
//             if (type != null)
//                 return type;
//         }
//
//         return null;
//     }
// }