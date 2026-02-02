using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.Pinata
{
    public class UIPinataMilestone : MonoBehaviour
    {
        [SerializeField] private Image visual;
        [SerializeField] private Image point;
        [SerializeField] private TMP_Text milestoneTxt;

        private int _milestone;

        public void BindData(int milestone)
        {
            _milestone = milestone;

            SetupVisual();
            SetupMilestone();
        }

        private void SetupVisual()
        {
            visual.gameObject.SetActive(IsReached());
            point.gameObject.SetActive(false);

            //bool reached = IsReached();
            //visual.gameObject.SetActive(reached);
            //point.gameObject.SetActive(reached);

            //int currentWin = PinataService.Instance.WinARow.Value;
            //var currentStage = PinataService.Instance.GetCurrentStage();

            //float fill = 1f;

            //if (_milestone == currentWin)
            //{
            //    if (_milestone != currentStage.MaxMilestone)
            //        fill = 0.8f;
            //}

            //if (_milestone == 1 && currentWin == 1)
            //    fill = 0f;

            //visual.fillAmount = fill;
        }

        private void SetupMilestone()
        {
            milestoneTxt.text = _milestone.ToString();
        }

        private bool IsReached()
        {
            return PinataService.Instance.WinARow.Value >= _milestone;
        }
    }
}