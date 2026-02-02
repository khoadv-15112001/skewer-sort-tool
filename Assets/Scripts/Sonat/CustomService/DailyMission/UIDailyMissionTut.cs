using I2.Loc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrillSort.DailyMission
{
    public class UIDailyMissionTut : MonoBehaviour
    {
        [SerializeField] private Localize[] titles;
        [SerializeField] private Localize describeTxt;
        [SerializeField] private Localize describeTxt_2;

        private void Start()
        {
            var dailyMission = DailyMissionService.Instance.GetCurrentDailyMission();

            foreach (var title in titles)
            {
                title.SetTerm(dailyMission.NameTerm);
            }

            describeTxt.SetTerm(dailyMission.DescribeTerm);
            describeTxt_2.SetTerm(dailyMission.DescribeTerm);
        }
    }
}