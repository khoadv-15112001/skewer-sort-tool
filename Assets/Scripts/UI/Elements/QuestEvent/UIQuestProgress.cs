using DG.Tweening;
using Manager;
using SonatFramework.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using SonatFramework.Scripts.UIModule.SpriteService;
using SonatFramework.Systems.UserData;
using SonatFramework.Scripts.UIModule.UIElements;
using I2.Loc;

namespace GrillSort.QuestEvent
{
    public class UIQuestProgress : MonoBehaviour
    {
        [Header("Quest")] [SerializeField] private FixedImageRatio iconQuest;
        [SerializeField] private Image iconReward;
        [SerializeField] private Slider sliderProgress;
        [SerializeField] private TMP_Text txtProgress;
        [SerializeField] private TMP_Text txtCompleted;
        [SerializeField] private UIBubbleReward uiBubbleReward;
        [SerializeField] private bool enableUpdateProgress = true;
        private readonly Service<QuestEventService> questEventService = new();

        void OnEnable()
        {
            if (enableUpdateProgress)
            {
                UpdateUI();
            }
            else
            {
                UpdatePreValue(questEventService.Instance.NumCollectedItemInGame);
            }

            questEventService.Instance.OnDataChanged += UpdateUI;
        }

        private void OnDisable()
        {
            questEventService.Instance.OnDataChanged -= UpdateUI;
        }

        private void UpdateUI()
        {
            var data = questEventService.Instance.Data;
            var config = questEventService.Instance.config;
            var maxNum = config.GetMaxNum(data.currentQuestIdx);

            if (questEventService.Instance.CheckCompleteAllQuest())
            {
                txtCompleted.gameObject.SetActive(true);
                txtProgress.gameObject.SetActive(false);
            }
            else
            {
                txtCompleted.gameObject.SetActive(false);
                txtProgress.gameObject.SetActive(true);
                txtProgress.text = $"{data.numItem}/{maxNum}";
            }

            var value = (float)data.numItem / maxNum;
            sliderProgress.DOValue(value, 0.5f);

            // update icon
            var currentItemId = questEventService.Instance.GetCurrentItemId();
            iconQuest.SetSprite(SonatSystem.GetService<SpriteAtlasService>().GetSprite($"SpecialItem_{currentItemId}"));

            // update reward
            uiBubbleReward.SetReward(config.GetReward(data.currentQuestIdx));
        }

        public void OnClick()
        {
        }

        // public void UpdateProgress(int numItem)
        // {
        //     var data = questEventService.Instance.Data;
        //     var maxNum = questEventService.Instance.config.GetMaxNum(data.currentQuestIdx);
        //     var value = questEventService.Instance.PreNumItem;
        //
        //     var targetFillAmount = (float)data.numItem / maxNum;
        //     var currentFillAmount = (float)value / maxNum;
        //     sliderProgress.DOValue(targetFillAmount, 0.5f);
        //
        //     var currentItemId = questEventService.Instance.GetCurrentItemId();
        //     iconQuest.SetSprite(SonatSystem.GetService<SpriteAtlasService>().GetSprite($"SpecialItem_{currentItemId}"));
        //
        //     DOTween.To(() => value, x => { txtProgress.text = $"{x}/{maxNum}"; }, data.numItem, 0.5f).SetEase(Ease.Linear);
        //     questEventService.Instance.UpdatePreNum();
        // }

        public void UpdatePreValue(int numItem)
        {
            var data = questEventService.Instance.Data;
            var maxNum = questEventService.Instance.config.GetMaxNum(data.currentQuestIdx);
            var value = questEventService.Instance.PreNumItem;

            if (questEventService.Instance.CheckCompleteAllQuest())
            {
                txtCompleted.gameObject.SetActive(true);
                txtProgress.gameObject.SetActive(false);
            }
            else
            {
                txtCompleted.gameObject.SetActive(false);
                txtProgress.gameObject.SetActive(true);
                txtProgress.text = $"{value}/{maxNum}";
            }

            var currentFillAmount = (float)value / maxNum;
            sliderProgress.value = currentFillAmount;

            // update icon
            var currentItemId = questEventService.Instance.GetCurrentItemId();
            //iconQuest.SetSpriteAsync(PathManager.ItemSprite(currentItemId)).Forget();
            iconQuest.SetSprite(SonatSystem.GetService<SpriteAtlasService>().GetSprite($"SpecialItem_{currentItemId}"));

            // update reward
            var config = questEventService.Instance.config;
            uiBubbleReward.SetReward(config.GetReward(data.currentQuestIdx));
        }
    }
}