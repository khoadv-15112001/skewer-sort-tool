using System;
using Sonat.Enums;
using UnityEngine;

namespace MyGame.SkewerJam.Scripts.Service
{
    [CreateAssetMenu(fileName = "HLWEventConfig", menuName = "MyGame/SkewerJam/HLW Event/HLW Event Config")]
    public class HLWEventConfig : ScriptableObject
    {
        [Header("Config unlock")] public LiveOpsPackData liveOpsPackData;

        [Header("Config expire")] public int expirationDay = 1;
        public int expirationMonth = 11;
        public int expirationYear = 2025;

        [Space(10)] public int energyUnlockEvent = 100;
        public int energyNormal = 20;
        public int energyHard = 30;
        public int energySuper = 40;
        public int energyNightmarishlyHard = 50;

        public long GetExpireTime()
        {
            var date = new DateTime(expirationYear, expirationMonth, expirationDay, 0, 0, 0);
            return ((DateTimeOffset)date).ToUnixTimeSeconds();
        }

        public int GetEnergyReward(int level)
        {
            var difficulty = MySonatFramework.GetLevelDifficulty(level);
            switch (difficulty)
            {
                case LevelDifficulty.Easy:
                    return energyNormal;
                case LevelDifficulty.Hard:
                    return energyHard;
                case LevelDifficulty.SuperHard:
                    return energySuper;
                case LevelDifficulty.NightmarishlyHard:
                    return energyNightmarishlyHard;
                default:
                    return energyNormal;
            }
        }

        public int GetEnergyGift(int level)
        {
            if (level <= 0)
                return 0; // level không hợp lệ

            if (level <= energyArray.Length)
            {
                return energyArray[level - 1]; // vì mảng index bắt đầu từ 0
            }
            else
            {
                // Level >= 41
                return 370 + (level - 40) * 10;
            }
        }
        
        // Mảng lưu energy gift cho level 1 -> 40
        private readonly int[] energyArray = new int[]
        {
            5, // Level 1
            10, // Level 2
            20, // Level 3
            30, // Level 4
            35, // Level 5
            45, // Level 6
            55, // Level 7
            60, // Level 8
            65, // Level 9
            80, // Level 10
            90, // Level 11
            95, // Level 12
            100, // Level 13
            110, // Level 14
            120, // Level 15
            130, // Level 16
            140, // Level 17
            150, // Level 18
            165, // Level 19
            170, // Level 20
            180, // Level 21
            185, // Level 22
            190, // Level 23
            205, // Level 24
            215, // Level 25
            225, // Level 26
            235, // Level 27
            245, // Level 28
            250, // Level 29
            265, // Level 30
            270, // Level 31
            280, // Level 32
            290, // Level 33
            300, // Level 34
            315, // Level 35
            330, // Level 36
            340, // Level 37
            350, // Level 38
            360, // Level 39
            370 // Level 40
        };
    }
}