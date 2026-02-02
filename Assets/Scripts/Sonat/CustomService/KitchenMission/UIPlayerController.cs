using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GrillSort.KitchenMission
{
    public class UIPlayerController : MonoBehaviour
    {
        [SerializeField] private List<UIPlayer> players = new();

        public static Action OnSetMedal;

        private KitchenMissionConfig.Players playerDatas = new();

        public void Setup()
        {
            playerDatas = KitchenMissionService.Instance.players.Value;

            for (int i = 0; i < players.Count && i < playerDatas.datas.Count; i++)
            {
                players[i].Init(playerDatas.datas[i]);
            }

            KitchenMissionService.Instance.UpdateStepAllPlayers();

            OnSetMedal += SetMedal;
        }

        public void SetMedal()
        {
            var datas = playerDatas.datas;
            if (datas == null || datas.Count == 0)
                return;

            var paired = players
                .Select((ui, i) => new { ui, step = datas[i].step })
                .OrderByDescending(p => p.step)
                .ToList();

            int currentRank = 1;
            int previousStep = -1;
            int playersProcessed = 0;

            foreach (var p in paired)
            {
                playersProcessed++;

                if (p.step != previousStep)
                {
                    currentRank = playersProcessed;
                    previousStep = p.step;
                }

                p.ui.SetMedal(currentRank - 1);

                //if (currentRank == 1)
                //    p.ui.SetMedal(UIMedalType.Gold);
                //else if (currentRank == 2)
                //    p.ui.SetMedal(UIMedalType.Silver);
                //else if (currentRank == 3)
                //    p.ui.SetMedal(UIMedalType.Bronze);
                //else
                //    p.ui.SetMedal(UIMedalType.None);
            }

            OnSetMedal = null;
        }
    }
}
