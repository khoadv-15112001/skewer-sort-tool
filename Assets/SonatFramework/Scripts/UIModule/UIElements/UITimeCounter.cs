using System;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.TimeManagement;
using TMPro;
using UnityEngine;

namespace SonatFramework.Scripts.UIModule.UIElements
{
    public class UITimeCounter : MonoBehaviour
    {
        public TMP_Text txtTime;
        public TxtTimeFormat timeFomat;
        public MonoBehaviour go;
        private readonly Service<TimeService> timeService = new();
        private Action callback;
        private Coroutine coroutine;

        public void SetData(long sec, Action callback)
        {
            if (this == null || gameObject == null) return;
            
            gameObject.SetActive(true);
            StopCount();
            go ??= this;
            this.callback = callback;
            if (go != null && go.gameObject != null && go.gameObject.activeInHierarchy)
                coroutine = go.StartCoroutine(timeService.Instance.DoActionRealtime(sec, UpdateTime));
        }

        private void UpdateTime(long sec)
        {
            if (sec <= 0)
            {
                StopCount();
                sec = 0;
                callback?.Invoke();
            }

            if (txtTime != null)
                txtTime.text = SonatUtils.GetTimeByFormat(sec, timeFomat);
        }

        public void SetData(string s)
        {
            gameObject.SetActive(true);
            txtTime.text = s;
        }

        private void StopCount()
        {
            if (coroutine == null) return;
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }
}