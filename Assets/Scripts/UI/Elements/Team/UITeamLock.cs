using I2.Loc;
using SonatFramework.Systems;
using UnityEngine;

public class UITeamLock : MonoBehaviour
{
    [SerializeField] private LocalizationParamsManager levelUnlockTxt;

    [SerializeField] private GameObject comingSoon;

    private readonly Service<TeamService> teamService = new();

    private void Start()
    {
        if (teamService.Instance.CheckLiveOpsCondition())
        {
            levelUnlockTxt.gameObject.SetActive(true);
            comingSoon.SetActive(false);

            levelUnlockTxt.SetParameterValue("VALUE", teamService.Instance.levelUnlock.ToString());
        }
        else
        {
            levelUnlockTxt.gameObject.SetActive(false);
            comingSoon.SetActive(true);
        }
    }
}
