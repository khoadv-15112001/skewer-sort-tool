using Sirenix.OdinInspector;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrillSort.DailyMission
{
    public abstract class DailyMissionBase : ScriptableObject, IDailyMission
    {
        public virtual DailyMissionConfig.EMission MissionType => DailyMissionConfig.EMission.END;

        [ValueDropdown(nameof(GetAllTerms))]
        public string NameTerm;
        [ValueDropdown(nameof(GetAllTerms))]
        public string DescribeTerm;

        public int TimeDuration_Hour;

        public Sprite widgetSprite;
        public GameObject PopupPrefab;
        public string NamePopup => PopupPrefab.name;

        public DailyMissionConfig.BasicStage BasicStageData;
        public DailyMissionConfig.XLStage XLStageData;

        public abstract void Activate();

        public abstract void Deactivate();

        public int GetMaxNumItem()
        {
            return DailyMissionService.Instance.isXLStage.BoolValue ? GetNumItemXLStage()
                : (BasicStageData.Targets.Count != 0 ? GetMaxItemBaseStage() : 0);
        }

        protected void AddTempItem(int item)
        {
            Debug.Log($"[DailyMissionService]: AddTempItem={item}, from={this.MissionType}");

            DailyMissionService.Instance.AddTempItem(item);
        }

        public int GetNumItemBaseStage(int stageIndex)
        {
            return SonatSDKAdapter.GetRemoteInt($"daily_mission_{MissionType.ToString().ToLower()}_base_stage_" + stageIndex,
                BasicStageData.Targets[stageIndex].NumItem);
        }

        public int GetNumItemXLStage()
        {
            return SonatSDKAdapter.GetRemoteInt($"daily_mission_{MissionType.ToString().ToLower()}_xl_stage",
                XLStageData.Target.NumItem);
        }

        public int GetMaxItemBaseStage()
        {
            int num = 0;

            for (int i = 0; i < BasicStageData.Targets.Count; i++)
            {
                num = Mathf.Max(num, GetNumItemBaseStage(i));
            }

            return num;
        }

        protected IEnumerable<string> GetAllTerms()
        {
            return LocalizationUtils.GetAllTerms();
        }
    }
}

public interface IDailyMission
{
    public void Activate();
    public void Deactivate();
}
