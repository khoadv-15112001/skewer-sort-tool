using I2.Loc;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.KitchenMission
{
    public class UIKitchenMissionRaceBanner : MonoBehaviour
    {
        [SerializeField] private GameObject joinContent;
        [SerializeField] private GameObject progressContent;

        [SerializeField] private TMP_Text timeTxt;

        [Space]
        [Header("JoinContent")]
        [SerializeField] private Button startBtn;
        [SerializeField] private LocalizationParamsManager stageParams;
        [SerializeField] private List<ParticleSystem> chestPS = new();

        [Space]

        [Header("ProgressContent")]
        [SerializeField] private LocalizationParamsManager numMission;
        [SerializeField] private Localize missionLocalize;
        [SerializeField] private List<UIPlayerRaceKitchenBanner> players;

        public bool IsActive()
        {
            return KitchenMissionService.Instance.CanActive();
        }

        private void Start()
        {
            Setup();
        }

        private void OnDestroy()
        {
            KitchenMissionService.OnTick -= UpdateTime;
            KitchenMissionService.OnChangePlayerData -= SetupContent;
        }

        private void UpdateTime()
        {
            timeTxt.text = KitchenMissionService.Instance.GetStringTimeCountdown();

            if (KitchenMissionService.Instance.CheckPassTime())
            {
                KitchenMissionService.OnTick -= UpdateTime;
                gameObject.SetActive(false);
            }
        }

        private void Setup()
        {
            if (!IsActive())
            {
                gameObject.SetActive(false);
                return;
            }

            SetupContent();

            UpdateTime();
            KitchenMissionService.OnTick += UpdateTime;
            KitchenMissionService.OnChangePlayerData += SetupContent;
        }

        private void SetupContent()
        {
            if (!KitchenMissionService.Instance.isJoined.BoolValue)
            {
                joinContent.gameObject.SetActive(true);
                progressContent.gameObject.SetActive(false);
                SetupJoin();
            }
            else
            {
                joinContent.gameObject.SetActive(false);
                progressContent.gameObject.SetActive(true);
                SetupProgress();
            }
        }

        private void SetupJoin()
        {
            stageParams.SetParameterValue("value", (KitchenMissionService.Instance.stage.Value + 1).ToString());

            for (int i = 0; i < chestPS.Count; i++)
            {
                chestPS[i].gameObject.SetActive(i == KitchenMissionService.Instance.stage.Value);
            }

            startBtn.onClick.AddListener(OnClickStart);

            void OnClickStart()
            {
                MySonatFramework.customTrackingService.OnShowPopup("WidgetKitchenMission".ToLogString(), "banner", "non_iap", "user");
                PanelManager.Instance.OpenPanel<PopupKitchenMissionStart>();
            }
        }

        private void SetupProgress()
        {
            numMission.SetParameterValue("value", (KitchenMissionService.Instance.stage.Value + 1).ToString());
            missionLocalize.SetTerm(KitchenMissionService.Instance.GetCurrentStageData().nameSkin);

            var datas = KitchenMissionService.Instance.players.Value.datas;

            Dictionary<KitchenMissionConfig.Players.Player, int> rankMap = new();

            var sorted = datas
                .OrderByDescending(p => p.step)
                .ToList();

            int currentRank = 1;
            int nextRank = 1;
            int? lastStep = null;

            foreach (var p in sorted)
            {
                if (lastStep != null && p.step < lastStep)
                {
                    currentRank = nextRank;
                }

                rankMap[p] = currentRank;

                lastStep = p.step;
                nextRank++;
            }

            for (int i = 0; i < players.Count; i++)
            {
                var data = datas[i];
                if (rankMap.TryGetValue(data, out int rank))
                {
                    players[i].Setup(data, rank);
                }
                else
                {
                    players[i].Setup(data, 1);
                }
            }
        }

    }
}