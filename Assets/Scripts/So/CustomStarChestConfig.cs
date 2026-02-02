using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using Sonat.Enums;
using SonatFramework.Systems.ConfigManagement;
using SonatFramework.Systems.InventoryManagement.GameResources;
using UnityEngine;

[CreateAssetMenu(fileName = "CustomStarChestConfig", menuName = "Custom Configs/Custom Star Chest Config")]
public class CustomStarChestConfig : StarChestConfig
{
    public override StarChest GetStarChest(int milestone)
    {
        // 🔹 Nếu còn trong danh sách cũ → dùng reward có sẵn
        if (milestone < starChests.Count)
        {
            return starChests[milestone];
        }

        // 🔹 Nếu milestone vượt quá danh sách → tạo reward mới theo pattern loop
        var patternIndex = (milestone - starChests.Count) % 6;

        var chest = new StarChest
        {
            starRequire = 2000,
            reward = new RewardData()
            {
                resourceDatas = new List<ResourceData>()
            }
        };

        switch (patternIndex)
        {
            case 0: // x = 1: 1 card_x2 , 75 coin
                chest.reward.AddReward(new ResourceData(RewardHelper.GetCardMultiplier(2), 1));
                chest.reward.AddReward(new ResourceData(GameResource.Coin, 75));
                break;

            case 1: // x = 2: 1 booster magnet, 30 rail
                chest.reward.AddReward(new ResourceData(GameResource.BoosterMagnet, 1));
                //chest.reward.AddReward(new ResourceData(GameResource., 30));
                break;

            case 2: // x = 3: 1 card_x4, 75 coin
                chest.reward.AddReward(new ResourceData(RewardHelper.GetCardMultiplier(4), 1));
                chest.reward.AddReward(new ResourceData(GameResource.Coin, 75));
                break;

            case 3: // x = 4: 1 pre magnet, 1 pre x2 star, 25 coin
                chest.reward.AddReward(new ResourceData(GameResource.PreBoosterMagnet, 1));
                chest.reward.AddReward(new ResourceData(GameResource.PreBoosterDoubleStar, 1));
                chest.reward.AddReward(new ResourceData(GameResource.Coin, 25));
                break;

            case 4: // x = 5: 1 card_x6, 75 coin
                chest.reward.AddReward(new ResourceData(RewardHelper.GetCardMultiplier(6), 1));
                chest.reward.AddReward(new ResourceData(GameResource.Coin, 75));
                break;

            case 5: // x = 6: 1 booster shuffle, 30 rail
                chest.reward.AddReward(new ResourceData(GameResource.BoosterShuffle, 1));
                //chest.reward.AddReward(new ResourceData(RewardHelper.GetResource(ResourceType.Rail), 30));
                break;
        }

        return chest;
    }
}
