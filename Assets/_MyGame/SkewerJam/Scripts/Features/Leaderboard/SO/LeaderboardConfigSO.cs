using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame.Leaderboard.SO
{
    [CreateAssetMenu(fileName = "LeaderboardConfigSO", menuName = "MyGame/Features/Leaderboard/LeaderboardConfigSO")]
    public class LeaderboardConfigSO : ScriptableObject
    {
        [Space(10)]
        [Header("Config medal")]
        public List<Sprite> medals;
        public List<Sprite> chests;

        public Sprite bgItem;
        public Sprite bgItemSelf;

        public Sprite bgScore;
        public Sprite bgScoreSelf;

        public Sprite GetMedalSprite(int rank, bool isSelf)
        {
            if (rank >= 1 && rank <= 3) return medals[rank - 1];

            if (isSelf) return medals[4];
            else return medals[3];
        }

        public Sprite GetBgItemSprite(bool isSelf)
        {
            if (!isSelf) return bgItem;
            else return bgItemSelf;
        }

        public Sprite GetChestSprite(int rank)
        {
            if (rank >= 1 && rank <= 3) return chests[rank - 1];
            return chests[2];
        }

        public Sprite GetBgScoreSprite(bool isSelf)
        {
            if (isSelf) return bgScoreSelf;
            else return bgScore;
        }
    }
}