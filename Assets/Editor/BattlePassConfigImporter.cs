using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using GrillSort.BattlePass;
using Sonat.Enums;
using SonatFramework.Systems.InventoryManagement.GameResources;

public class BattlePassConfigImporter : CSVImporter
{
    private BattlePassConfig targetSO;

    [MenuItem("Tools/CSV Importer/BattlePassConfig")]
    static void Init()
    {
        GetWindow<BattlePassConfigImporter>("BattlePassConfig Importer");
    }

    protected override void OnGUI()
    {
        targetSO = (BattlePassConfig)EditorGUILayout.ObjectField("Target SO", targetSO, typeof(BattlePassConfig), false);

        // if (targetSO != null)
        // {
        //     EditorGUILayout.Space();
        //     EditorGUILayout.LabelField("Preview Data", EditorStyles.boldLabel);

        //     SerializedObject so = new SerializedObject(targetSO);
        //     SerializedProperty milestoneDatasProp = so.FindProperty("milestoneDatas");

        //     // EditorGUILayout.BeginScrollView();
        //     EditorGUILayout.PropertyField(milestoneDatasProp, true);
        //     // EditorGUILayout.EndScrollView();
        //     if (GUI.changed)
        //         so.ApplyModifiedProperties();
        // }

        List<GameResource> keys = new();
        keys = new List<GameResource> {
                GameResource.Lives,
                GameResource.BoosterMagnet,
                GameResource.BoosterShuffle,
                GameResource.BoosterFreeze,
                GameResource.BoosterMagicKey,
                GameResource.BoosterBlowTorch,
                GameResource.PreBoosterMagnet,
                GameResource.PreBoosterFreeze,
                GameResource.PreBoosterDoubleStar,
                GameResource.Coin
            };

        EditorGUILayout.LabelField("Resource Order:", EditorStyles.boldLabel);
        for (int i = 0; i < keys.Count; i++)
        {
            EditorGUILayout.LabelField($"{i + 1}. {keys[i]}");
        }

        base.OnGUI();
    }

    protected override void ImportCSV()
    {
        if (targetSO == null)
        {
            Debug.LogError("Chưa chọn ScriptableObject!");
            return;
        }

        string[] lines = GetLines();
        List<MileStoneData> milestoneDatas = new List<MileStoneData>();

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            string[] values = GetValues(line);

            var keys = new List<GameResource> {
                GameResource.Lives,
                GameResource.BoosterMagnet,
                GameResource.BoosterShuffle,
                GameResource.BoosterFreeze,
                GameResource.BoosterMagicKey,
                GameResource.BoosterBlowTorch,
                GameResource.PreBoosterMagnet,
                GameResource.PreBoosterFreeze,
                GameResource.PreBoosterDoubleStar,
                GameResource.Coin
            };

            var reward = new RewardData();
            for (int i = 0; i < keys.Count; i++)
            {
                Debug.Log(values[i]);
                if (string.IsNullOrEmpty(values[i])) continue;

                switch (keys[i])
                {
                    case GameResource.Lives:
                        var lives = float.Parse(values[i]);
                        lives = lives * 3600;
                        reward.AddReward(new ResourceData(keys[i], (int)lives));
                        break;
                    default:
                        reward.AddReward(new ResourceData(keys[i], int.Parse(values[i])));
                        break;
                }
            }

            MileStoneData data = new MileStoneData
            {
                stt = 0,
                reward = reward
            };
            milestoneDatas.Add(data);
        }

        Undo.RecordObject(targetSO, "Import Excel Data");
        targetSO.milestoneDatas = milestoneDatas;
        EditorUtility.SetDirty(targetSO);
        AssetDatabase.SaveAssets();
    }
}
