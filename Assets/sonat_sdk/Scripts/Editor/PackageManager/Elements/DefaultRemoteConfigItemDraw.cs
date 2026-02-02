using Sonat.FirebaseModule.RemoteConfig;
using UnityEditor;
using UnityEngine;

namespace Sonat.Editor.PackageManager.Elements
{
    public class DefaultRemoteConfigItemDraw
    {
        private ListRemoteConfigDraw listRemoteConfigDraw;
        private RemoteConfigDefaultByString itemData;
        private bool foldOut;
        private int index;
        private int keyEnumValue;

        public DefaultRemoteConfigItemDraw(RemoteConfigDefaultByString itemData, ListRemoteConfigDraw listRemoteConfigDraw)
        {
            this.itemData = itemData;
            this.listRemoteConfigDraw = listRemoteConfigDraw;

            UpdateKey();
        }

        private void UpdateKey()
        {
        }

        public void Draw()
        {
            GUILayout.BeginHorizontal(GUI.skin.box);
            itemData.active = EditorGUILayout.Toggle("", itemData.active, GUILayout.Width(15));
            EditorGUI.BeginDisabledGroup(!itemData.active);

            GUILayout.BeginVertical();
            string label = string.IsNullOrEmpty(itemData.GetKey()) ? "Empty Key" : itemData.GetKey();
            foldOut = EditorGUILayout.Foldout(foldOut, label, true);

            if (foldOut)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.BeginVertical();

                EditorGUIUtility.labelWidth = 80;
                itemData.dataType = (DataType)EditorGUILayout.EnumPopup("Data Type", itemData.dataType, GUILayout.MaxWidth(300));

                GUILayout.BeginHorizontal();
                if (!itemData.customKey)
                {
                    itemData.keyEnum = (RemoteConfigKey)EditorGUILayout.EnumPopup("Key", itemData.keyEnum, GUILayout.MaxWidth(300));
                }
                else
                {
                    itemData.key = EditorGUILayout.TextField("Key", itemData.key, GUILayout.MaxWidth(300));
                }
            
                itemData.customKey = EditorGUILayout.ToggleLeft("Custom", itemData.customKey, GUILayout.MaxWidth(80));
            
                GUILayout.EndHorizontal();
                switch (itemData.dataType)
                {
                    case DataType.Boolean:
                        itemData.defaultBoolean = EditorGUILayout.Toggle("Value", itemData.defaultBoolean, GUILayout.Width(50));
                        break;
                    case DataType.String:
                        itemData.defaultString = EditorGUILayout.TextField("Value", itemData.defaultString, GUILayout.MaxWidth(300));
                        break;
                    case DataType.Int:
                        itemData.defaultInt = EditorGUILayout.IntField("Value", itemData.defaultInt, GUILayout.MaxWidth(300));
                        break;
                    case DataType.Float:
                        itemData.defaultFloat = EditorGUILayout.FloatField("Value", itemData.defaultFloat, GUILayout.MaxWidth(300));
                        break;
                    case DataType.Json:
                        itemData.jsonTextAsset = (TextAsset)EditorGUILayout.ObjectField("Value", itemData.jsonTextAsset, typeof(TextAsset), false, GUILayout.MaxWidth(300));
                        break;
                }

                GUILayout.Space(5);

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();


            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                listRemoteConfigDraw.RemoveItem(itemData);
            }

            GUILayout.EndHorizontal();
        }
    }
}