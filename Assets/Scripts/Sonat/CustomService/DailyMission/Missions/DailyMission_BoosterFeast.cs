using Sonat.Enums;
using SonatFramework.Systems.EventBus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrillSort.DailyMission
{
    [CreateAssetMenu(fileName = "DailyMission_BoosterFeast", menuName = "My Services/DailyMissionService/DailyMission_BoosterFeast")]
    public class DailyMission_BoosterFeast : DailyMissionBase
    {
        public override DailyMissionConfig.EMission MissionType => DailyMissionConfig.EMission.Booster_Feast;

        private EventBinding<UseBoosterEvent> useBoosterEvent;

        public override void Activate()
        {
            useBoosterEvent = new EventBinding<UseBoosterEvent>(UseBooster);
        }

        public override void Deactivate()
        {
            EventBus<UseBoosterEvent>.Deregister(useBoosterEvent);
        }

        private void UseBooster(UseBoosterEvent @event)
        {
            if (@event.booster.ResourceType() != GameResourceType.Booster) return;

            AddTempItem(1);
        }
    }
}