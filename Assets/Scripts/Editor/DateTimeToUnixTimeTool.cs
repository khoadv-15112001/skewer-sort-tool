#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Tool.Editor
{
    public class DateTimeToUnixTimeTool : EditorWindow
    {
        private string dateTimeInput = "";
        private string unixTimeOutput = "";
        private string humanReadableOutput = "";
        private bool useCurrentTime = false;
        private Vector2 scrollPosition;

        [MenuItem("Tools/DateTime to Unix Time Converter")]
        public static void ShowWindow()
        {
            GetWindow<DateTimeToUnixTimeTool>("DateTime to Unix Time");
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("DateTime to Unix Time Converter", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Current time section
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Current Time", EditorStyles.boldLabel);

            DateTime currentTime = DateTime.Now;
            EditorGUILayout.LabelField("Current Local Time:", currentTime.ToString("yyyy-MM-dd HH:mm:ss"));
            EditorGUILayout.LabelField("Current UTC Time:", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
            EditorGUILayout.LabelField("Current Unix Time:", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            // Input section
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Input", EditorStyles.boldLabel);

            useCurrentTime = EditorGUILayout.Toggle("Use Current Time", useCurrentTime);

            if (!useCurrentTime)
            {
                EditorGUILayout.LabelField("Enter DateTime or Unix Timestamp", EditorStyles.miniLabel);
                dateTimeInput = EditorGUILayout.TextField("Input:", dateTimeInput);

                EditorGUILayout.HelpBox("Supported formats:\n" +
                                        "• yyyy-MM-dd HH:mm:ss (e.g., 2024-01-15 14:30:00)\n" +
                                        "• yyyy-MM-dd (e.g., 2024-01-15)\n" +
                                        "• MM/dd/yyyy HH:mm:ss (e.g., 01/15/2024 14:30:00)\n" +
                                        "• MM/dd/yyyy (e.g., 01/15/2024)\n" +
                                        "• Unix timestamp (seconds) (e.g., 1705324200)\n" +
                                        "• Unix timestamp (milliseconds) (e.g., 1705324200000)", MessageType.Info);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            // Convert button
            if (GUILayout.Button("Convert", GUILayout.Height(30)))
            {
                ConvertDateTime();
            }

            EditorGUILayout.Space();

            // Output section
            if (!string.IsNullOrEmpty(unixTimeOutput))
            {
                EditorGUILayout.BeginVertical("box");
                GUILayout.Label("Output", EditorStyles.boldLabel);

                EditorGUILayout.LabelField("Unix Time (seconds):", unixTimeOutput);
                EditorGUILayout.LabelField("Unix Time (milliseconds):", (long.Parse(unixTimeOutput) * 1000).ToString());

                if (!string.IsNullOrEmpty(humanReadableOutput))
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Human Readable:", humanReadableOutput);
                }

                // Copy button
                if (GUILayout.Button("Copy Unix Time to Clipboard"))
                {
                    GUIUtility.systemCopyBuffer = unixTimeOutput;
                    Debug.Log($"Unix time copied to clipboard: {unixTimeOutput}");
                }

                EditorGUILayout.EndVertical();
            }

            // Additional tools section
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Quick Tools", EditorStyles.boldLabel);

            if (GUILayout.Button("Set to Current Time"))
            {
                useCurrentTime = true;
                ConvertDateTime();
            }

            if (GUILayout.Button("Set to Start of Today"))
            {
                DateTime startOfToday = DateTime.Today;
                dateTimeInput = startOfToday.ToString("yyyy-MM-dd HH:mm:ss");
                useCurrentTime = false;
                ConvertDateTime();
            }

            if (GUILayout.Button("Set to End of Today"))
            {
                DateTime endOfToday = DateTime.Today.AddDays(1).AddSeconds(-1);
                dateTimeInput = endOfToday.ToString("yyyy-MM-dd HH:mm:ss");
                useCurrentTime = false;
                ConvertDateTime();
            }

            EditorGUILayout.Space();
            GUILayout.Label("Common Unix Timestamps", EditorStyles.boldLabel);

            if (GUILayout.Button("Unix Epoch (1970-01-01)"))
            {
                dateTimeInput = "0";
                useCurrentTime = false;
                ConvertDateTime();
            }

            if (GUILayout.Button("Year 2000 (2000-01-01)"))
            {
                dateTimeInput = "946684800";
                useCurrentTime = false;
                ConvertDateTime();
            }

            if (GUILayout.Button("Year 2020 (2020-01-01)"))
            {
                dateTimeInput = "1577836800";
                useCurrentTime = false;
                ConvertDateTime();
            }

            if (GUILayout.Button("Year 2030 (2030-01-01)"))
            {
                dateTimeInput = "1893456000";
                useCurrentTime = false;
                ConvertDateTime();
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
        }

        private void ConvertDateTime()
        {
            try
            {
                DateTime dateTime = DateTime.Now;

                if (useCurrentTime)
                {
                    dateTime = DateTime.Now;
                    dateTimeInput = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    // Try to parse the input
                    if (string.IsNullOrEmpty(dateTimeInput))
                    {
                        unixTimeOutput = "";
                        humanReadableOutput = "";
                        return;
                    }

                    // Check if input is already a Unix timestamp
                    if (long.TryParse(dateTimeInput, out long unixTimestamp))
                    {
                        // Determine if it's seconds or milliseconds based on length
                        if (unixTimestamp > 9999999999) // Likely milliseconds
                        {
                            // Convert milliseconds to seconds
                            long unixSeconds = unixTimestamp / 1000;
                            dateTime = DateTimeOffset.FromUnixTimeSeconds(unixSeconds).DateTime;
                            unixTimeOutput = unixSeconds.ToString();
                            humanReadableOutput = $"Converted from Unix timestamp (milliseconds): {dateTime.ToString("yyyy-MM-dd HH:mm:ss")}";
                        }
                        else // Likely seconds
                        {
                            dateTime = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime;
                            unixTimeOutput = unixTimestamp.ToString();
                            humanReadableOutput = $"Converted from Unix timestamp (seconds): {dateTime.ToString("yyyy-MM-dd HH:mm:ss")}";
                        }

                        return;
                    }

                    // Try different date formats
                    string[] formats =
                    {
                        "yyyy-MM-dd HH:mm:ss",
                        "yyyy-MM-dd",
                        "MM/dd/yyyy HH:mm:ss",
                        "MM/dd/yyyy",
                        "dd/MM/yyyy HH:mm:ss",
                        "dd/MM/yyyy",
                        "yyyy/MM/dd HH:mm:ss",
                        "yyyy/MM/dd"
                    };

                    bool parsed = false;
                    foreach (string format in formats)
                    {
                        if (DateTime.TryParseExact(dateTimeInput, format, null, System.Globalization.DateTimeStyles.None, out dateTime))
                        {
                            parsed = true;
                            break;
                        }
                    }

                    if (!parsed)
                    {
                        // Try general parsing
                        if (!DateTime.TryParse(dateTimeInput, out dateTime))
                        {
                            unixTimeOutput = "Error: Invalid date format";
                            humanReadableOutput = "";
                            return;
                        }
                    }
                }

                // Convert to Unix timestamp
                DateTimeOffset dateTimeOffset = new DateTimeOffset(dateTime);
                long unixTimeSeconds = dateTimeOffset.ToUnixTimeSeconds();

                unixTimeOutput = unixTimeSeconds.ToString();
                humanReadableOutput = $"Converted: {dateTime.ToString("yyyy-MM-dd HH:mm:ss")} ({dateTimeOffset.Offset})";
            }
            catch (Exception ex)
            {
                unixTimeOutput = $"Error: {ex.Message}";
                humanReadableOutput = "";
                Debug.LogError($"DateTime conversion error: {ex.Message}");
            }
        }
    }
}
#endif