using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GrillSort.Pinata
{
    public class UIPinataMilestoneChanceController : MonoBehaviour
    {
        [SerializeField] private List<Sprite> icons = new();
        [SerializeField] private UIPinataMilestoneChance midPrefab;

        //private List<UIPinataMilestoneChance> milestones = new();
        public static List<UIPinataMilestoneChance> Milestones = new();
        public static int CurrentMilestone;

        public static Action OnBonusCard;

        public void Setup()
        {
            CurrentMilestone = 0;

            Milestones = GetComponentsInChildren<UIPinataMilestoneChance>().ToList();

            var currentStage = PinataService.Instance.GetCurrentStage();

            while (Milestones.Count < currentStage.NumChance)
            {
                AppendMid();
            }

            UpdateMilestone();

            PinataAnimHandle.OnTapPinataSuccessful += OnTapPinata;
            OnBonusCard += BonusCard;
        }

        private void OnDestroy()
        {
            PinataAnimHandle.OnTapPinataSuccessful -= OnTapPinata;
            OnBonusCard -= BonusCard;
        }

        private void BonusCard()
        {
            AppendMid();
        }

        private void AppendMid()
        {
            var appendMid = Instantiate(midPrefab, transform);
            appendMid.transform.SetSiblingIndex(transform.childCount - 2);
            Milestones.Insert(Milestones.Count - 1, appendMid);
        }

        private void OnTapPinata()
        {
            //var bonusChance = Random.value <= 0.8f;
            //if (bonusChance && milestones.Count < 5)
            //    AppendMid();

            CurrentMilestone++;
            UpdateMilestone();

            CheckFullMilestone();
        }

        private void UpdateMilestone()
        {
            for (int i = 0; i < Milestones.Count; i++)
            {
                var milestone = Milestones[i];

                if (i >= CurrentMilestone)
                {
                    if (i == CurrentMilestone)
                    {
                        milestone.BindData(UIPinataMilestoneChance.State.Doing);
                        milestone.SetIcon(GetSpriteIcon());
                    }
                    else
                        milestone.BindData(UIPinataMilestoneChance.State.Locked);
                }
                else milestone.BindData(UIPinataMilestoneChance.State.Done);
            }
        }

        private void CheckFullMilestone()
        {
            if (CurrentMilestone < Milestones.Count) return;

            PinataController.OnFullMilestone?.Invoke();
        }

        private Sprite GetSpriteIcon()
        {
            return icons[Mathf.Clamp(PinataController.CurrentPigIndex - 1, 0, icons.Count - 1)];
        }
    }
}