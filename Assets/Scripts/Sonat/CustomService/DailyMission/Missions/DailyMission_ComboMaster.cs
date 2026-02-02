using SonatFramework.Systems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrillSort.DailyMission
{
    [CreateAssetMenu(fileName = "DailyMission_ComboMaster", menuName = "My Services/DailyMissionService/DailyMission_ComboMaster")]
    public class DailyMission_ComboMaster : DailyMissionBase
    {
        public override DailyMissionConfig.EMission MissionType => DailyMissionConfig.EMission.Combo_Master;

        private ComboService comboService => MySonatFramework.GetService<ComboService>();

        public override void Activate()
        {
            comboService.OnComboChange += OnComboChange;
        }

        public override void Deactivate()
        {
            comboService.OnComboChange -= OnComboChange;
        }

        private void OnComboChange()
        {
            int combo = comboService.Combo;

            if (combo <= 1) return;

            AddTempItem(1);
        }
    }
}