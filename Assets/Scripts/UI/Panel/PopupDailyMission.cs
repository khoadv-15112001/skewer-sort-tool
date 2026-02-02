using Cysharp.Threading.Tasks;
using Helper;
using I2.Loc;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.AudioManagement;
using SonatFramework.Systems.InventoryManagement;
using SonatFramework.Systems.TimeManagement;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.DailyMission
{
    public class PopupDailyMission : Panel
    {
        [SerializeField] private Button closeBtn;
        [SerializeField] private Button tutBtn;
        [SerializeField] private TMP_Text timeTxt;
        //[SerializeField] private Localize titleLocalize;
        [SerializeField] private SkeletonGraphic tagAnim;

        [SerializeField] private GameObject baseStageParent;
        [SerializeField] private List<UIBaseStage> stages;
        [SerializeField] private UIXLStage xlStage;

        [SerializeField] private GameObject cheat;
        [SerializeField] private TMP_InputField cheatTxt;

        private long timeEnd;

        public override void OnSetup()
        {
            base.OnSetup();
            closeBtn.onClick.AddListener(OnClickClose);
            tutBtn.onClick.AddListener(OnClickTut);
            //titleLocalize.SetTerm(DailyMissionService.Instance.GetCurrentDailyMission().NameTerm);

            SetupStage();
            SetupTag();

            StartCoroutine(IECountdown());

            cheat.SetActive(CheatPanel.IsCheating);

            CheckTut().Forget();
        }

        public void OnCheatItem()
        {
            if (int.TryParse(cheatTxt.text, out var value))
            {
                DailyMissionService.Instance.CheatSetItem(value);
            }
        }

        public override void Open(UIData uiData)
        {
            base.Open(uiData);
        }

        public override void Close()
        {
            base.Close();

            StopAllCoroutines();
        }

        private void OnClickClose()
        {
            Close();
        }

        private void OnClickTut()
        {
            PanelManager.Instance.OpenPanelByName<BasePanel>(DailyMissionConfig.NameOfPopupTut);
        }

        private async UniTaskVoid SetupTag()
        {
            tutBtn.gameObject.SetActive(!DailyMissionService.Instance.isXLStage.BoolValue);

            tagAnim.gameObject.SetActive(false);

            await UniTask.WaitForSeconds(0.2f);

            if (DailyMissionService.Instance.isXLStage.BoolValue)
            {
                tagAnim.gameObject.SetActive(true);
                tagAnim.AnimationState.SetAnimation(0, "Appear", false);
                tagAnim.AnimationState.AddAnimation(0, "Idle", true, 0);
            }
        }

        private void SetupStage()
        {
            if (DailyMissionService.Instance.isXLStage.BoolValue)
            {
                baseStageParent.gameObject.SetActive(false);
                xlStage.gameObject.SetActive(true);

                xlStage.Setup();
            }
            else
            {
                baseStageParent.gameObject.SetActive(true);
                xlStage.gameObject.SetActive(false);

                for (int i = 0; i < stages.Count; i++)
                {
                    UIBaseStage stage = stages[i];
                    stage.Setup(i);
                }
            }
        }

        private IEnumerator IECountdown()
        {
            timeEnd = DailyMissionService.Instance.timeEnd.Value;

            long now;

            var waitSecond = new WaitForSeconds(1f);

            while (true)
            {
                now = DailyMissionService.Instance.GetTimeUnixNow();

                if (now >= timeEnd)
                    Close();

                timeTxt.text = SonatUtils.GetTimeByFormat(timeEnd - now, TxtTimeFormat.ShortDay_FullTime);

                yield return waitSecond;
            }
        }

        private async UniTaskVoid CheckTut()
        {
            if (!DailyMissionService.Instance.isShownTut.BoolValue)
            {
                DailyMissionService.Instance.isShownTut.BoolValue = true;
                DailyMissionService.OnShowTut?.Invoke();

                BlockPanel.Set(true);

                await UniTask.WaitForSeconds(0.5f);

                BlockPanel.Set(false);

                PanelManager.Instance.OpenPanelByName<BasePanel>(DailyMissionConfig.NameOfPopupTut);
            }
        }

        public override string GetPlacement()
        {
            string basePlacement = "LO:::dm";
            string type = DailyMissionService.Instance.GetCurrentDailyMission().MissionType.ToString().ToLower();
            string stage = DailyMissionService.Instance.isXLStage.BoolValue ? "xl" : "basic";

            return $"{basePlacement}_{type}_{stage}";
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                MySonatFramework.GetService<AudioService>().PlaySound(Sonat.Enums.AudioId.ButtonClick);
                //btn.onClick.RemoveAllListeners();

                var rewardData = DailyMissionService.Instance.GetRewardXLStage();

                MySonatFramework.GetService<InventoryService>().AddReward(rewardData, new EarnResourceLogData
                {
                    source = "non_iap",
                    spendType = "feature",
                    spendId = "daily_mission"
                });

                UIData uiData = new UIData();
                uiData.Add("Reward", rewardData);
                PanelManager.Instance.OpenPanel<PopupDailyMissionReward>(uiData);

                //DailyMissionService.Instance.CompleteMission();

                //SetClaimed();
            }
        }

#endif
    }
}