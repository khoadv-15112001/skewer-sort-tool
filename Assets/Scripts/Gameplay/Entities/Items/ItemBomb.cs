using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using Sonat.Enums;
using TMPro;
using UnityEngine;

namespace Gameplay.Entities.Items
{
    public class ItemBomb : Item
    {
        [SerializeField] private TMP_Text textBombCountdown;
        [SerializeField] private int timeCountdown;
        private float targetTime;
        private bool exploded = false;

        public override void SetPrimary(bool isPrimary)
        {
            base.SetPrimary(isPrimary);
            if (isPrimary)
            {
                StartBomb();
            }
        }

        public void StartBomb()
        {
            targetTime = GameplayController.instance.timeManager.GetTimeRemaining() - timeCountdown;
            GameplayController.instance.timeManager.OnTimeUpdate += OnTimeUpdate;
            exploded = false;
        }

        private void OnDisable()
        {
            GameplayController.instance.timeManager.OnTimeUpdate -= OnTimeUpdate;
        }

        private void OnTimeUpdate(float currentTime)
        {
            if(exploded) return;
            textBombCountdown.text = ((int)(currentTime - targetTime)).ToString();
            if (currentTime <= targetTime)
            {
                ExplodeBomb();
            }
        }

        public void ExplodeBomb()
        {
            exploded = true;
            GameplayController.instance.ExplosiveBomb().Forget();
        }

        public override void OnCreateObj(params object[] args)
        {
            base.OnCreateObj(args);
            textBombCountdown.text = timeCountdown.ToString();
        }

        public override void OnReturnObj()
        {
            base.OnReturnObj();
            exploded = false;
            targetTime = 0;
        }
    }
}