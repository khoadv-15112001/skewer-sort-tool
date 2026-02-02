using UnityEngine;

[CreateAssetMenu(fileName = "TeamConfig", menuName = "Sonat Configs Custom/Team Config")]
public class TeamConfig : ScriptableObject
{
    [Header("Config unlock")]
    public int levelUnlock;
    public LiveOpsPackData liveOpsPackData;

    [Space(10)]
    [Header("Others")]
    public int numLogo;
    public int createCost;
    public int messageLimit = 20;
    public int messagePerRequest = 20;
    public int limitChat = 100;
}
