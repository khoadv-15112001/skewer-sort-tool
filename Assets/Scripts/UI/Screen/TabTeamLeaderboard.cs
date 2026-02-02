using SonatFramework.Systems;
using UnityEngine;

public class TabTeamLeaderboard : UITabBase
{
    [SerializeField] private GameObject comingsoonObj;
    [SerializeField] private GameObject mainObj;

    protected override void Start()
    {
        base.Start();
    }

    public override void OnShow()
    {
        base.OnShow();

        if (!isActive) return;

        if (Service<TeamService>.Get().IsUnlockFeature())
        {
            comingsoonObj.SetActive(false);
            mainObj.SetActive(true);
        }
        else
        {
            comingsoonObj.SetActive(true);
            mainObj.SetActive(false);
        }
    }
}
