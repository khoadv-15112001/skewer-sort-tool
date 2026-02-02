using I2.Loc;
using SonatFramework.Systems;
using TMPro;
using UnityEngine;

public class TabLeaderboard : UITabBase
{
    [SerializeField] private GameObject unlockObj;
    [SerializeField] private GameObject lockObj;

    [SerializeField] private LocalizationParamsManager levelUnlockTxt;
    [SerializeField] private TMP_Text comingSoonTxt;

    private readonly Service<LeaderboardService> leaderboardService = new();

    protected override void Start()
    {
        base.Start();

        if (leaderboardService.Instance.CheckLiveOpsCondition())
        {
            levelUnlockTxt.gameObject.SetActive(true);
            comingSoonTxt.gameObject.SetActive(false);

            levelUnlockTxt.SetParameterValue("VALUE", leaderboardService.Instance.LevelUnlock.ToString());
        }
        else
        {
            levelUnlockTxt.gameObject.SetActive(false);
            comingSoonTxt.gameObject.SetActive(true);
        }
    }

    public override void OnShow()
    {
        base.OnShow();

        bool isUnlocked = leaderboardService.Instance.IsUnlockFeature();
        lockObj.SetActive(!isUnlocked);
        unlockObj.SetActive(isUnlocked);
    }
}
