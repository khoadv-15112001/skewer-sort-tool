using System;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using TMPro;
using UnityEngine;

namespace SonatFramework.Scripts.Feature.Lives.UI
{
    public class UILives : UICurrency
    {
        [SerializeField] private UITimeCounter timeCounter;
        [SerializeField] private UITimeCounter timeCounterRefill;
        [SerializeField] private TMP_Text txtFullLives;
        [SerializeField] private GameObject normalLives, unlimitedLives;

        private readonly Service<LivesService> liveService = new();
        private bool orgBlockClick;

        // public override void OnEnable()
        // {
        //     //base.OnEnable();
        //     OnLiveUpdate();
        //     //liveService.Instance.onLivesUpdate += OnLiveUpdate;
        // }
        //
        // protected override void OnDisable()
        // {
        //     //base.OnDisable();
        //     //liveService.Instance.onLivesUpdate -= OnLiveUpdate;
        // }

        //public override void OnClickCurrency()
        //{
          //  if (blockClick) return;
            //PanelManager.Instance.OpenForget<PopupRefillLives>();
        //}

        private void Awake()
        {
            orgBlockClick = blockClick;
        }


        public override void UpdateValueView(bool doCounter = true)
        {
            if (!liveService.Instance.isUnlimitedLive.BoolValue)
            {
                normalLives.SetActive(true);
                unlimitedLives.SetActive(false);
                base.UpdateValueView(doCounter);
                if (liveService.Instance.IsFullLives())
                {
                    timeCounterRefill.gameObject.SetActive(false);
                    txtFullLives.gameObject.SetActive(true);
                    blockClick = true;
                    plusObj.gameObject.SetActive(!blockClick);
                }
                else
                {
                    txtFullLives.gameObject.SetActive(false);
                    timeCounterRefill.gameObject.SetActive(true);
                    timeCounterRefill.SetData(liveService.Instance.GetTimeRefillRemain(), () =>
                    {
                        SonatUtils.DelayCall(2, () => UpdateValueView(true), this);
                    });
                    blockClick = orgBlockClick;
                    plusObj.gameObject.SetActive(!blockClick);
                }
                
            }
            else
            {
                normalLives.SetActive(false);
                unlimitedLives.SetActive(true);
                timeCounter.SetData(liveService.Instance.GetUnlimitedLiveRemain(), null);
            }
        }

        private void OnFinishUnlimitedLives()
        {
            UpdateValueView();
        }
    }
}