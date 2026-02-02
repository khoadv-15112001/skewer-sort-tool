using System;
using Manager;
using Sonat.Enums;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace GrillSort.QuestEvent
{
    public class UIReceiveQuestItem : MonoBehaviour
    {
        [SerializeField] private Image iconBg;
        [SerializeField] private Image iconSlider;
        [SerializeField] private TMP_Text txtValue;
        [SerializeField] private TMP_Text txtValuePercent;
        [SerializeField] private GameObject pMain;
        [SerializeField] private bool claimOnEnable = false;
        [SerializeField] private float startX = 0;
        [SerializeField] private float startY = 3;
        private readonly Service<QuestEventService> questEventService = new();

        private int currentItemId;
        private int maxNumItem;
        private int currentNum;
        private int oldValue;

        private void OnEnable()
        {
            if (questEventService.Instance.config.Active == false)
            {
                gameObject.SetActive(false);
                return;
            }

            var questIndex = questEventService.Instance.Data.currentQuestIdx;
            maxNumItem = questEventService.Instance.config.GetMaxNum(questIndex);
            currentItemId = questEventService.Instance.GetCurrentItemId();

            SetSprite(currentItemId);

            currentNum = questEventService.Instance.NumCollectedItemInGame;
            // var fillAmount = (float)currentNum / maxNumItem;
            // iconSlider.DOFillAmount(fillAmount, 0.5f).SetEase(Ease.Linear).From(0).SetDelay(0.5f);

            if (txtValuePercent != null)
            {
                txtValuePercent.text = $"{questEventService.Instance.NumCollectedItemInGame}";
                //     oldValue = Mathf.RoundToInt(fillAmount * 100);
                //     txtValuePercent.text = $"{oldValue}%";
            }

            // if (claimOnEnable)
            // {
            //     SonatUtils.DelayCall(1f, OnClaim, this);
            // }
        }

        private void OnDisable()
        {
        }

        private void SetSprite(int id)
        {
            // iconBg.SetSpriteAsync(PathManager.ItemSprite(id)).Forget();
            // iconBg.SetNativeSize();

            // iconSlider.SetSpriteAsync(PathManager.ItemSprite(id)).Forget();
            // iconSlider.SetNativeSize();
        }

        // [SerializeField] private float strength = 2;
        // [SerializeField] private int vibrato = 10;
        public void OnClaim()
        {
            questEventService.Instance.AddItemCollectIngame();

            // move out
            // SonatUtils.DelayCall(2.25f, () =>
            // {
            //     pMain.transform.DOLocalMoveX(350f, 0.5f).SetEase(Ease.InBack);
            // }, this);
        }

        public void Collect(int numCollectedItemInGame = 10, Action callback = null)
        {
            // collect effect

            var startPos = iconSlider.transform.position + new Vector3(startX, startY, 0);
            var endPos = iconSlider.transform.position;

            var collectEffect = new CollectEffectMultipleItemInGame() { radius = 1f, itemId = currentItemId, scale = 0.8f };
            collectEffect.Collect(GameResource.None, numCollectedItemInGame, startPos, endPos, callback);

            // effect slider/ value
            // var newValue = questEventService.Instance.Data.numItem;
            // var fillAmount = (float)newValue / maxNumItem;
            // iconSlider.DOFillAmount(fillAmount, 0.5f).SetDelay(1.25f).SetEase(Ease.Linear);
            // txtValue.DOCounter(currentNum, newValue, 0.5f).SetDelay(1.25f).SetEase(Ease.Linear);

            // if (txtValuePercent != null)
            // {
            //     DOTween.To(() => oldValue, x =>
            //     {
            //         txtValuePercent.text = $"{x}%";
            //     }, Mathf.RoundToInt(fillAmount * 100), 0.5f)
            //     .SetEase(Ease.Linear)
            //     .SetDelay(1.2f);
            // }
        }
    }
}