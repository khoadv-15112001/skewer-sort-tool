using I2.Loc;
using SonatFramework.Systems;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.Stats
{
    public class UIStat : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Localize nameLocalize;
        //[SerializeField] private TMP_Text nameTxt;
        [SerializeField] private TMP_Text valueTxt;

        private StatBase _statBase;

        public void BindData(StatsConfig.StatType statType, int value = -1)
        {
            _statBase = SonatSystem.GetService<StatsService>().GetStat(statType);

            SetIcon();
            SetName();
            SetValue(value);
        }

        private void SetIcon()
        {
            icon.sprite = _statBase.icon;
        }

        private void SetName()
        {
            nameLocalize.SetTerm(_statBase.Name);
            //nameTxt.text = _statBase.StatType.ToString();
        }

        private void SetValue(int value = -1)
        {
            if (value != -1)
            {
                valueTxt.text = value.ToString();
                return;
            }

            valueTxt.text = _statBase.GetValue().ToString();
        }
    }
}