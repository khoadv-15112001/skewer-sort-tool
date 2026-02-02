using System.Collections;
using SonatFramework.Scripts.UIModule;
using TMPro;
using UnityEngine;

namespace SonatFramework.Templates.UI.ScriptBase
{
    public class PopupWaitAdsBreakBase : Panel
    {
        public TMP_Text txtTime;

        protected virtual void OnEnable()
        {
            Setup();
        }

        public virtual void Setup()
        {
            StartCoroutine(CountTime());
        }

        protected virtual IEnumerator CountTime()
        {
            int sec = 5;
            while (sec > 0)
            {
                txtTime.text = $"Ads break in {sec}s";
                yield return new WaitForSeconds(1);
                sec--;
            }

            txtTime.text = "Take a break!";
        }
    }
}