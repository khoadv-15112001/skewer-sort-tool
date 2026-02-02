using GrillSort.DailyMission;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.Pinata
{
    public class PopupPinataParty : Panel
    {
        [SerializeField] private Button closeBtn;
        [SerializeField] private TMP_Text timeTxt;

        private List<UIPinataCardStage> cardStages = new();
        private long timeEnd;

        public override void OnSetup()
        {
            base.OnSetup();

            SetupButton();
            SetupCard();

            timeEnd = PinataService.Instance.GetTimeEndUnix();
            Tick();

            PinataService.OnTick += Tick;
        }

        private void OnDestroy()
        {
            PinataService.OnTick -= Tick;
        }

        private void Tick()
        {
            timeTxt.text = PinataService.Instance.GetRemainTime();

            if (PinataService.Instance.GetTimeUnixNow() >= timeEnd)
                Close();
        }

        private void SetupButton()
        {
            closeBtn.onClick.AddListener(OnClickClose);
        }

        private void SetupCard()
        {
            cardStages.Clear();
            cardStages = GetComponentsInChildren<UIPinataCardStage>().ToList();

            for (int i = 0; i < cardStages.Count; i++)
            {
                cardStages[i].BindData(i + 1);
            }
        }

        private void OnClickClose()
        {
            Close();
        }
    }
}