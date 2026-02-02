using I2.Loc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrillSort.Rail
{
    public class UIRailMilestoneTitle : MonoBehaviour
    {
        [SerializeField] private GameObject selectDot;
        [SerializeField] private Localize nameOn;
        [SerializeField] private Localize nameOff;

        private int _stageId;
        private RailConfig.Stage _stageData;

        public void Bind(int stageId)
        {
            _stageId = stageId;
            _stageData = RailService.Instance.GetStage(stageId);

            SetupVisual();
        }

        private void SetupVisual()
        {
            SetupName();
            SetupSelect();
        }

        private void SetupName()
        {
            nameOn.SetTerm(_stageData.Name);
            nameOff.SetTerm(_stageData.Name);
        }

        private void SetupSelect()
        {
            bool isSelect = RailService.Instance.CurrentStage.Value == _stageId;
            selectDot.SetActive(isSelect);
            nameOn.gameObject.SetActive(isSelect);
            nameOff.gameObject.SetActive(!isSelect);
        }
    }
}