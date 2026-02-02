using I2.Loc;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GrillSort.Pinata
{
    public class UIPinataCardStage : MonoBehaviour
    {
        [SerializeField] private LocalizationParamsManager titleParam;

        [SerializeField] private GameObject winstreakIcon;
        [SerializeField] private TMP_Text winstreakCountTxt;
        [SerializeField] private GameObject winstreakTxt;

        [SerializeField] private TMP_Text completedTxt;

        [SerializeField] private GameObject tickObj;

        [SerializeField] private GameObject lockObj;
        [SerializeField] private GameObject shadowObj;

        private int _stage;
        private EState _state;
        private PinataConfig.Stage _stageData;

        public void BindData(int stage)
        {
            _stage = stage;
            _stageData = PinataService.Instance.GetStage(_stage);

            SetupState();

            SetupTitle();
            SetupWinstreak();
            SetupCompleted();
            SetupTick();
            SetupLock();
        }

        private void SetupState()
        {
            var curStage = PinataService.Instance.CurrentStage.Value;

            if (_stage >= curStage)
            {
                if (_stage == curStage)
                    _state = EState.Doing;
                else
                    _state = EState.Lock;
            }
            else _state = EState.Completed;
        }

        private void SetupTitle()
        {
            titleParam.SetParameterValue("value", _stage.ToString());
            titleParam.gameObject.SetActive(_state != EState.Completed);
        }

        private void SetupWinstreak()
        {
            winstreakIcon.SetActive(_state != EState.Completed);
            winstreakCountTxt.text = _stageData.MaxMilestone.ToString();
            winstreakTxt.SetActive(_state != EState.Completed);
        }

        private void SetupCompleted()
        {
            completedTxt.gameObject.SetActive(_state == EState.Completed);
        }

        private void SetupTick()
        {
            tickObj.SetActive(_state == EState.Completed);
        }

        private void SetupLock()
        {
            lockObj.SetActive(_state == EState.Lock);
            shadowObj.SetActive(_state == EState.Lock);
        }

        public enum EState
        {
            Lock,
            Doing,
            Completed
        }
    }
}