using System;
using System.Collections;
using System.Collections.Generic;
using Sonat.Enums;
using SonatFramework.Systems.ConfigManagement;
using SonatFramework.Systems.InventoryManagement.GameResources;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "StarChestConfig", menuName = "Sonat Configs/Star Chest Config", order = 1)]
public class StarChestConfig : ConfigSo
{
    [SerializeField] protected List<StarChest> starChests;

    public virtual StarChest GetStarChest(int milestone)
    {
        if (milestone >= starChests.Count)
        {
            return starChests[^1];
        }
        return starChests[milestone];
    }

    #if UNITY_EDITOR
    public TextAsset data;

    [ContextMenu("Assign Data")]
    public void AssignData()
    {
        // int startIndex = 2;

        // if (data == null)
        // {
        //     Debug.LogError("CSV data file is not assigned.");
        //     return;
        // }

        // starChests.Clear();

        // string[] lines = data.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        // if (lines.Length <= startIndex)
        // {
        //     Debug.LogError("CSV has no data rows.");
        //     return;
        // }

        // for (int i = startIndex; i < lines.Length; i++) // Skip header and index row
        // {
        //    string line = lines[i].Trim();
        //    if (string.IsNullOrWhiteSpace(line)) continue;

        //    string[] values = line.Split(',');

        //    StarChest starChest = new StarChest();
        //    starChest.starRequire = CsvUtils.TryParseInt(values[1]);
        //    starChest.reward = new RewardData();
        //    starChest.railReward = new RewardData();

        //    // Các dòng thời gian → nhân 3600 và ép int
        //    CsvUtils.AddRewardIfValid(starChest.reward, GameResource.Lives, CsvUtils.ConvertHoursToSeconds(values[4]));
        //    CsvUtils.AddRewardIfValid(starChest.reward, GameResource.BoosterMagnet, CsvUtils.TryParseInt(values[5]));
        //    CsvUtils.AddRewardIfValid(starChest.reward, GameResource.BoosterFreeze, CsvUtils.TryParseInt(values[6]));
        //    CsvUtils.AddRewardIfValid(starChest.reward, GameResource.BoosterShuffle, CsvUtils.TryParseInt(values[7]));
        //    CsvUtils.AddRewardIfValid(starChest.reward, GameResource.BoosterMagicKey, CsvUtils.TryParseInt(values[8]));
        //    CsvUtils.AddRewardIfValid(starChest.reward, GameResource.BoosterBlowTorch, CsvUtils.TryParseInt(values[9]));
        //    CsvUtils.AddRewardIfValid(starChest.reward, GameResource.PreBoosterMagnet, CsvUtils.TryParseInt(values[10]));
        //    CsvUtils.AddRewardIfValid(starChest.reward, GameResource.PreBoosterFreeze, CsvUtils.TryParseInt(values[11]));
        //    CsvUtils.AddRewardIfValid(starChest.reward, GameResource.PreBoosterDoubleStar, CsvUtils.TryParseInt(values[12]));
        //    CsvUtils.AddCard(starChest.reward, CsvUtils.TryParseInt(values[13]));
        //    CsvUtils.AddRewardIfValid(starChest.railReward, GameResource.Token_Rail, CsvUtils.TryParseInt(values[14]));
        //    CsvUtils.AddRewardIfValid(starChest.reward, GameResource.Coin, CsvUtils.TryParseInt(values[15]));

        //    starChests.Add(starChest);
        // }

        // UnityEditor.EditorUtility.SetDirty(this);
        // UnityEditor.AssetDatabase.SaveAssets();
        // UnityEditor.AssetDatabase.Refresh();

        // Debug.LogError($"Assigned {starChests.Count} milestones from CSV.");
        // Debug.LogError($"Load cvs completed!");
    }
#endif
}
[Serializable]
public class StarChest
{
    public int starRequire;
    public RewardData reward;
    public RewardData railReward;

    public RewardData GetRewardData()
    {
#if CUSTOM_ENUM
        // Nếu user join Rail event và có railReward → return railReward
        if (GrillSort.Rail.RailService.Instance != null && 
            GrillSort.Rail.RailService.Instance.IsJoinEvent() && 
            railReward != null && 
            railReward.resourceDatas != null && 
            railReward.resourceDatas.Count > 0)
        {
            return railReward;
        }
#endif
        return reward;
    }
}
