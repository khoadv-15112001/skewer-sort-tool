#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Sonat.Editor
{
    public static class SonatEditorHelper
    {
        public static Rect[] GetSplitRect(this Rect position, int n, float gap = 0)
        {
            var rects = new Rect[n];
            var d = position.width / rects.Length;
            for (var i = 0; i < rects.Length; i++)
            {
                rects[i] = position;
                rects[i].width = d - (i == rects.Length - 1 ? 0 : gap);
                rects[i].x += i * d;
            }

            return rects;
        }

        public static object GetParentSerializedValueRaw(this SerializedProperty property)
        {
            var properties = property.propertyPath.Split('.').ToList();
            if (properties.Count > 1)
            {
                properties.RemoveAt(properties.Count - 1);
                var path = string.Join(".", properties);
//                    var find = property.serializedObject.FindProperty(path);

                object @object = property.serializedObject.targetObject;
                @object = @object.GetType()
                    .GetField(path,
                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                    ?.GetValue(@object);
                return @object;
            }

            return null;
        }

        public static T LoadConfigSo<T>(string assetName) where T : class
        {
            string filePath = FindFilePath(assetName + ".asset", SonatSDKWIZ.DataFolder, false);
            if (filePath == null) return null;

            var t = AssetDatabase.LoadAssetAtPath(filePath, typeof(T)) as T;
            return t;
        }

        public static string TextField(string label, string text, params GUILayoutOption[] options)
        {
            var textDimensions = GUI.skin.label.CalcSize(new GUIContent(label));
            EditorGUIUtility.labelWidth = textDimensions.x + 5;
            return EditorGUILayout.TextField(label, text, options);
        }


        public static string CheckVersionInstalled(string dependencies, string folder, string versionPattern = @"""version"": ""(?<ver>.+)""")
        {
            string filePath = FindFilePath(dependencies, folder, false);
            if (filePath == null) return null;

            var t = AssetDatabase.LoadAssetAtPath(filePath, typeof(TextAsset)) as TextAsset;
            if (t == null) return null;
            //var pattern = @"""com.appsflyer:af-android-sdk:(?<v>.+)""";
            Regex express = new Regex(versionPattern, RegexOptions.IgnoreCase);
            Match match = express.Match(t.text);
            string version = match.Groups["ver"].Value;
            return version;
        }

        public static string CheckVersionInstalledByFileName(string filePathRelease, string folder, string versionPattern = @"""version"": ""(?<ver>.+)""")
        {
            string filePath = FindAssetPath(filePathRelease, folder, false);
            if (string.IsNullOrEmpty(filePath)) return null;
            string fileName = Path.GetFileName(filePath);

            Regex express = new Regex(versionPattern, RegexOptions.IgnoreCase);
            Match match = express.Match(fileName);
            string version = match.Groups["ver"].Value;
            return version;
        }

        public static string CheckVersionInstalledByManifest(string packageId)
        {
            if (!File.Exists("Packages/manifest.json"))
                return null;

            string jsonText = File.ReadAllText("Packages/manifest.json");
            string pattern = @"""PackageId"": ""(?<ver>.+)""";
            pattern = pattern.Replace("PackageId", packageId);
            Regex express = new Regex(pattern, RegexOptions.IgnoreCase);
            Match match = express.Match(jsonText);
            string version = match.Groups["ver"].Value;
            return version;
        }


        public static string FindFilePath(string fileName, string folder, bool full = true)
        {
            var paths = Directory.GetFiles(Application.dataPath, fileName, SearchOption.AllDirectories);
            for (int i = 0; i < paths.Length; i++)
            {
                if (paths[i].Contains(folder) || string.IsNullOrEmpty(folder))
                {
                    if (full)
                        return paths[i];
                    else
                    {
                        return paths[i].Replace(Application.dataPath, "Assets");
                    }
                }
            }

            return null;
        }

        public static string FindAssetPath(string fileName, string folder, bool full = true)
        {
            var guids = AssetDatabase.FindAssets(fileName);
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (path.Contains(folder) || string.IsNullOrEmpty(folder))
                {
                    if (full)
                        return path;
                    else
                    {
                        return path.Replace(Application.dataPath, "Assets");
                    }
                }
            }

            return null;
        }

        public static bool HasSymbol(string symbol, BuildTargetGroup buildTargetGroup)
        {
            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup).Split(';');
            for (int i = 0; i < symbols.Length; i++)
            {
                if (symbols[i] == symbol) return true;
            }

            return false;
        }


        public static void AddSymbol(string[] add_symbols, BuildTargetGroup buildTargetGroup)
        {
            if (buildTargetGroup == BuildTargetGroup.Unknown)
            {
                AddSymbolForBuildTarget(add_symbols, BuildTargetGroup.Standalone);
                AddSymbolForBuildTarget(add_symbols, BuildTargetGroup.Android);
                AddSymbolForBuildTarget(add_symbols, BuildTargetGroup.iOS);
            }
            else
            {
                AddSymbolForBuildTarget(add_symbols, buildTargetGroup);
            }
        }

        private static bool progressBarShowing;

        private static void AddSymbolForBuildTarget(string[] add_symbols, BuildTargetGroup buildTargetGroup)
        {
            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup).Split(';');
            List<string> newSymbols = new List<string>(symbols);
            foreach (var symbol in add_symbols)
            {
                if (!newSymbols.Contains(symbol))
                    newSymbols.Add(symbol);
            }

            if (symbols.Length == newSymbols.Count) return;

            if (!progressBarShowing)
            {
                ShowProgressBar();
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, newSymbols.ToArray());
        }

        private static async Task ShowProgressBar()
        {
            progressBarShowing = true;
            int time = 0;
            while (time < 1000)
            {
                EditorUtility.DisplayProgressBar("Adding Symbol", "Please Wait...", time / 1000f);
                await Task.Delay(100);
                time += 100;
            }

            EditorUtility.ClearProgressBar();
            progressBarShowing = false;
        }

        public static void RemoveSymbolFromBuildTarget(string remove_symbol)
        {
            // var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Split(';');
            // List<string> newSymbols = new List<string>(symbols);
            //
            //
            // if (newSymbols.Contains(remove_symbol))
            //     newSymbols.Remove(remove_symbol);
            //
            // if (symbols.Length == newSymbols.Count) return;
            // PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, newSymbols.ToArray());
        }

        public static void ShowNotification(string msg, bool beep, bool allowFocusedWindow)
        {
            ShowNotification(msg, beep, null, allowFocusedWindow);
        }

        public static void ShowNotification(string msg, bool beep, EditorWindow editorWindow, bool allowFocusedWindow)
        {
            if (!editorWindow)
                editorWindow = GetBestEditorWindowToShowNotification(allowFocusedWindow);

            if (!editorWindow)
                return;

            try
            {
                editorWindow.ShowNotification(new GUIContent(msg));
            }
            catch
            {
            }

            if (beep)
                try
                {
                    EditorApplication.Beep();
                }
                catch
                {
                }
        }

        public static EditorWindow GetBestEditorWindowToShowNotification(bool allowFocusedWindow = true)
        {
            EditorWindow editorWindow = EditorWindow.focusedWindow;
            if (!editorWindow)
            {
                editorWindow = EditorWindow.mouseOverWindow;
                if (!editorWindow)
                    editorWindow = EditorWindow.GetWindow<SceneView>();
            }

            return editorWindow;
        }
    }
}
#endif