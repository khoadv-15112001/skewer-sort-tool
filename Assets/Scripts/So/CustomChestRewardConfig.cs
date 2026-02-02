using System.Collections.Generic;
using System.Security.AccessControl;
using Newtonsoft.Json;
using Sonat.Enums;
using SonatFramework.Scripts.Feature;
using SonatFramework.Systems.InventoryManagement.GameResources;
using UnityEngine;

[CreateAssetMenu(fileName = "CustomChestRewardConfig", menuName = "GrillSort/SO/CustomChestRewardConfig")]
public class CustomChestRewardConfig : ChestRewardProgressConfig
{
    public override ChestConfig GetChestRewardData(int index)
    {
        if (index >= 0 && index < ChestRewards.Count)
        {
            //Debug.Log($"anhnt: start chest {index + 1}, levelRq={ChestRewards[index].levelRequired}, resource={JsonConvert.SerializeObject(ChestRewards[index])}");

            return ChestRewards[index];
        }

        var chestConfig = new ChestConfig
        {
            levelRequired = chestConfigDefault.levelRequired,
            reward = new RewardData
            {
                resourceDatas = new List<ResourceData>()
            }
        };

        if (index >= 20)
        {
            chestConfig.levelRequired = index < 50 ? 10 : 15;

            int patternIndex = (index - 20) % 6;

            switch (patternIndex)
            {
                case 0: // 1 prebooster + 20 coin
                    chestConfig.reward.resourceDatas.Add(new ResourceData
                    {
                        resource = RewardHelper.GetRandomPreBooster(),
                        quantity = 1
                    });
                    chestConfig.reward.resourceDatas.Add(new ResourceData
                    {
                        resource = GameResource.Coin,
                        quantity = 20
                    });
                    break;

                case 1: // 50 coin + 0.5 infinite lives
                    chestConfig.reward.resourceDatas.Add(new ResourceData
                    {
                        resource = GameResource.Coin,
                        quantity = 50
                    });
                    chestConfig.reward.resourceDatas.Add(new ResourceData
                    {
                        resource = GameResource.Lives,
                        quantity = 1800
                    });
                    break;

                case 2: // 1 booster ngẫu nhiên
                    chestConfig.reward.resourceDatas.Add(new ResourceData
                    {
                        resource = RewardHelper.GetRandomBooster(),
                        quantity = 1
                    });
                    break;

                case 3: // 50 coin + 0.5 infinite lives
                    chestConfig.reward.resourceDatas.Add(new ResourceData
                    {
                        resource = GameResource.Coin,
                        quantity = 50
                    });
                    chestConfig.reward.resourceDatas.Add(new ResourceData
                    {
                        resource = GameResource.Lives,
                        quantity = 1800
                    });
                    break;

                case 4: // 20 coin + card pack 3
                    chestConfig.reward.resourceDatas.Add(new ResourceData
                    {
                        resource = GameResource.Coin,
                        quantity = 20
                    });
                    chestConfig.reward.resourceDatas.Add(new ResourceData
                    {
                        resource = GameResource.Card_Randomx3,
                        quantity = 1
                    });
                    break;

                case 5: // 50 coin + 0.5 infinite lives
                    chestConfig.reward.resourceDatas.Add(new ResourceData
                    {
                        resource = GameResource.Coin,
                        quantity = 50
                    });
                    chestConfig.reward.resourceDatas.Add(new ResourceData
                    {
                        resource = GameResource.Lives,
                        quantity = 1800
                    });
                    break;
            }
        }
        Debug.Log($"anhnt: start random chest {index + 1}, levelRq={chestConfig.levelRequired}, resource={JsonConvert.SerializeObject(chestConfig)}");

        return chestConfig;
    }
}
