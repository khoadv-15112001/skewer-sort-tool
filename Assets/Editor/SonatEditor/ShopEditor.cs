using System.Collections;
using System.Collections.Generic;
using Sonat.Enums;
using SonatFramework.Scripts.Feature.Shop;
using SonatFramework.Systems.InventoryManagement.GameResources;
using UnityEditor;
using UnityEngine;

namespace Sonat.SonatEditor.Shop
{
    public class ShopImporter : CSVImporter
    {
        private ShopConfig shopConfig;
        private List<GameResource> gameResources = new List<GameResource> {
            GameResource.Lives,
            GameResource.BoosterMagnet,
            GameResource.BoosterFreeze,
            GameResource.BoosterShuffle,
            GameResource.BoosterMagicKey,
            GameResource.BoosterBlowTorch,
            GameResource.Coin
            }; // Default values
        private string log = "";
        SerializedObject serializedObject;


        [MenuItem("SonatEditor/Shop/ShopImporter")]
        static void Init()
        {
            GetWindow<ShopImporter>("ShopImporter");
        }

        void OnEnable()
        {
            serializedObject = new SerializedObject(this);
        }

        protected override void OnGUI()
        {
            shopConfig = (ShopConfig)EditorGUILayout.ObjectField("ShopConfigSO", shopConfig, typeof(ShopConfig), false);

            // update serialized object
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("-----CSV FORMAT-----", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("The first column is the price", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("The following columns are game resources", EditorStyles.boldLabel);
            for (int i = 0; i < gameResources.Count; i++)
            {
                gameResources[i] = (GameResource)EditorGUILayout.EnumPopup($"Resource {i + 1}", gameResources[i]);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Game Resource"))
            {
                gameResources.Add(GameResource.Lives); // Default value
            }

            if (GUILayout.Button("Remove Last Game Resource"))
            {
                if (gameResources.Count > 0)
                {
                    gameResources.RemoveAt(gameResources.Count - 1);
                }
                else
                {
                    log = "No game resources to remove";
                }
            }
            EditorGUILayout.EndHorizontal();

            // apply changes
            serializedObject.ApplyModifiedProperties();

            base.OnGUI();

            ShowLog();
        }

        protected override void ImportCSV()
        {
            if (shopConfig == null)
            {
                Debug.LogError("Chưa chọn ScriptableObject!");
                return;
            }

            string[] lines = GetLines();
            List<ShopPack> shopPacks = new List<ShopPack>();

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                string[] values = GetValues(line);

                var reward = new RewardData();
                for (int i = 0; i < gameResources.Count; i++)
                {
                    if (string.IsNullOrEmpty(values[i])) continue;

                    switch (gameResources[i])
                    {
                        case GameResource.Lives:
                            var lives = float.Parse(values[i]);
                            lives = lives * 3600;
                            reward.AddReward(new ResourceData(gameResources[i], (int)lives));
                            break;
                        default:
                            reward.AddReward(new ResourceData(gameResources[i], int.Parse(values[i])));
                            break;
                    }
                }

                ShopPack data = new ShopPack
                {
                    rewardData = reward
                };
                shopPacks.Add(data);
            }

            Undo.RecordObject(shopConfig, "Import Excel Data");
            shopConfig.packs.AddRange(shopPacks);
            EditorUtility.SetDirty(shopConfig);
            AssetDatabase.SaveAssets();
        }

        void ShowLog()
        {
            if (string.IsNullOrEmpty(log) == false)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Log Output", EditorStyles.boldLabel);
                GUIStyle redStyle = new GUIStyle(EditorStyles.label);
                redStyle.normal.textColor = Color.red;
                EditorGUILayout.LabelField(log, redStyle);
            }
        }
    }
}
