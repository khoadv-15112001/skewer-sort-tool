using System.Collections.Generic;
using GrillSort.OnlineService;
using UnityEditor;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class ServerDataEditor : EditorWindow
{
    DataSyncKey key = DataSyncKey.UserLevel_Classic;
    string rawValue = "";   // sẽ in ra cửa sổ editor
    string stringValue = "";
    int intValue = 0;
    float floatValue = 0f;
    DataType selectedType = DataType.String;

    enum DataType { String, Int, Float }

    [MenuItem("Tools/Server Data Editor")]
    public static void ShowWindow()
    {
        GetWindow<ServerDataEditor>("Server Data Editor");
    }

    void OnGUI()
    {
        GUILayout.Label("Server Data Editor", EditorStyles.boldLabel);

        key = (DataSyncKey) EditorGUILayout.EnumPopup("Key", key);
        selectedType = (DataType)EditorGUILayout.EnumPopup("Type", selectedType);

        switch (selectedType)
        {
            case DataType.String:
                stringValue = EditorGUILayout.TextField("Value", stringValue);
                break;
            case DataType.Int:
                intValue = EditorGUILayout.IntField("Value", intValue);
                break;
            case DataType.Float:
                floatValue = EditorGUILayout.FloatField("Value", floatValue);
                break;
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Get from Server"))
        {
            string[] paths = new string[] { key.ToString() };
            Debug.Log("Get value from server with key: " + key);
            GetValueFromServer(paths).Forget();
        }

        if (GUILayout.Button("Update to Server"))
        {
            var updateData = new Dictionary<string, object>();
            switch (selectedType)
            {
                case DataType.String:
                    updateData.Add(key.ToString(), stringValue);
                    break;
                case DataType.Int:
                    updateData.Add(key.ToString(), intValue);
                    break;
                case DataType.Float:
                    updateData.Add(key.ToString(), floatValue);
                    break;
            }
            UpdateValueToServer(updateData).Forget();
            Debug.Log("Update value to server with key: " + selectedType + " " + key);
        }

        GUILayout.Space(20);

        // Hiển thị rawValue lấy được từ server
        GUILayout.Label("Server type: " + selectedType, EditorStyles.boldLabel);
        GUILayout.Label("Server Value", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(string.IsNullOrEmpty(rawValue) ? "Chưa có dữ liệu" : rawValue, UnityEditor.MessageType.Info);

        GUILayout.Space(20);
        if (GUILayout.Button("Delete user"))
        {
            if (EditorUtility.DisplayDialog("Confirm Delete", "Are you sure you want to delete the user?", "Yes", "No"))
            {
                DeleteUser().Forget();
            }
        }
    }

    private async UniTaskVoid GetValueFromServer(string[] paths)
    {
        var value = await MySonatFramework.GetService<OnlineService>().BIGetUserData(paths);
        if (value.data.TryGetValue(key.ToString(), out var serverVal))
        {
            rawValue = serverVal.ToString();

            // parse ra field tùy type
            switch (selectedType)
            {
                case DataType.String:
                    stringValue = rawValue;
                    break;
                case DataType.Int:
                    int.TryParse(rawValue, out intValue);
                    break;
                case DataType.Float:
                    float.TryParse(rawValue, out floatValue);
                    break;
            }

            Debug.Log($"<color=green>Successfully get value from server with key: {key}, value: {rawValue}</color>");
        }
        else
        {
            rawValue = "Không tìm thấy key trên server";
            Debug.LogWarning("Key không tồn tại trên server: " + key);
        }

        Repaint(); // bắt editor vẽ lại để cập nhật UI
    }

    private async UniTaskVoid UpdateValueToServer(Dictionary<string, object> updateData)
    {
        var value = await MySonatFramework.GetService<OnlineService>().BIUpdateUserData(updateData);
        Debug.Log("Successfully update value to server with key: " + selectedType + " " + key + " value: " + value);
    }

    private async UniTaskVoid DeleteUser()
    {
        await MySonatFramework.GetService<OnlineService>().BIDeleteUser();
        PlayerPrefs.DeleteKey(CheckConnectServer.DATA_KEY + "_USER_ID");
        Debug.Log("Successfully delete user");
    }
}
