using SonatFramework.Systems.InventoryManagement.GameResources;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GrillSort.ClaimRrdService
{
    [CreateAssetMenu(fileName = "ClaimRwdConfig", menuName = "Sonat Configs Custom/ClaimRwdConfig")]
    [Serializable]
    public class ClaimRwdConfig : ScriptableObject
    {
        public bool active;

        public int retentionDay = 1; // user quay lại vào ngày thứ x thì mới unlock
        public int levelStart = 1;

        public List<ClaimRwdData> datas = new();
    }
 
    [Serializable]
    public class ClaimRwdData
    {
        public int level; // sẽ chia thành nhiều mốc level
        public int cooldown = 10; // cooldown 10s mới unlock
        public RewardData rewardData = new();
    }
}
