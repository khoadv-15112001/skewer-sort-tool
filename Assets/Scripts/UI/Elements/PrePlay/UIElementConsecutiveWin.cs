using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.ConsecutiveWin
{
    public class UIElementConsecutiveWin : MonoBehaviour
    {
        [SerializeField] private TMP_Text txtReward;
        [SerializeField] private LocalizationParamsManager localizationParamsManager;
        [SerializeField] private Image imgSlider;
        [SerializeField] private GameObject[] tickObj;

        public void SetData(int consecutiveWins, int timeToAdd)
        {
            if (localizationParamsManager) localizationParamsManager.SetParameterValue("VALUE", consecutiveWins.ToString());
            if (txtReward) txtReward.text = $"+{timeToAdd}s";
        }

        public void SetComplete(bool isActive, bool playSlider = false)
        {
            foreach (var tick in tickObj)
            {
                tick.SetActive(isActive);
            }

            if (isActive && playSlider)
            {
                imgSlider.DOFillAmount(1, 1f).From(0).SetEase(Ease.InSine);
            }
        }
    }
}
