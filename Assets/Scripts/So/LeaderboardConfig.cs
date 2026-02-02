using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LeaderboardConfig", menuName = "Sonat Configs Custom/Leaderboard Config")]
public class LeaderboardConfig : ScriptableObject
{
    [Header("Config unlock")]
    public int levelUnlock;
    public LiveOpsPackData liveOpsPackData;

    [Space(10)]
    [Header("Config medal")]
    public List<Sprite> medals;
    public Sprite bgItem;
    public Sprite bgItemSelf;

    public Sprite GetMedalSprite(int rank, bool isSelf)
    {
        if (rank <= 3) return medals[rank - 1];

        if (isSelf) return medals[4];
        else return medals[3];
    }

    public Sprite GetBgItemSprite(bool isSelf)
    {
        if (!isSelf) return bgItem;
        else return bgItemSelf;
    }
}
