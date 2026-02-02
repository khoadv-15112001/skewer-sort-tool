using System.Collections.Generic;
using Sonat.FirebaseModule.RemoteConfig;
using UnityEditor;
using UnityEngine;

namespace Sonat.Editor.PackageManager.Elements
{
    public class ListRemoteConfigDraw
    {
        private List<RemoteConfigDefaultByString> defaultConfigs;
        private List<DefaultRemoteConfigItemDraw> items;
        private Vector2 scrollPos;

        public void Init(List<RemoteConfigDefaultByString> defaultConfigs)
        {
            this.defaultConfigs = defaultConfigs;
            items = new List<DefaultRemoteConfigItemDraw>();
            foreach (var configDefault in this.defaultConfigs)
            {
                DefaultRemoteConfigItemDraw item = new DefaultRemoteConfigItemDraw(configDefault, this);
                items.Add(item);
            }
        }

        public void Draw()
        {
            GUILayout.BeginVertical();
            GUILayout.Label("Default Remote Config", EditorStyles.boldLabel);
            GUILayout.Space(5);
            if (defaultConfigs != null && defaultConfigs.Count > 0)
            {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, new GUIStyle(GUI.skin.box), GUILayout.ExpandHeight(true));
                for (int i = 0; i < items.Count; i++)
                {
                    items[i].Draw();
                    GUILayout.Space(2);
                }
                EditorGUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Label("Default remote config list is empty", EditorStyles.centeredGreyMiniLabel);
            }

            GUILayout.Space(2);

            if (GUILayout.Button("+ Default Remote Config", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
            {
                AddItem();
            }
            GUILayout.EndVertical();
        }

        private void AddItem()
        {
            var remoteConfig = new RemoteConfigDefaultByString();
            defaultConfigs.Add(remoteConfig);

            var item = new DefaultRemoteConfigItemDraw(remoteConfig, this);
            items.Add(item);
        }

        public void RemoveItem(RemoteConfigDefaultByString item)
        {
            int index = defaultConfigs.IndexOf(item);
            if (index < 0) return;
            defaultConfigs.Remove(item);
            items.RemoveAt(index);
        }
    }
}