using DG.Tweening;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement;
using UnityEngine;
using UnityEngine.UI;

namespace MyGame.Modules.CardCollection
{
    public class PopupCompleteAlbum : Panel
    {
        [SerializeField] private UIAlbum album;

        [SerializeField] private Slider slider;
        [SerializeField] private UIRewardItem rewardItem;

        [Header("Animation Slider")]
        [SerializeField] private float delayAppearSlider = 2f;
        [SerializeField] private AnimationCurve curveEaseSlider = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Animation Completed Obj")]
        [SerializeField] private float durationSlider = 0.3f;
        [SerializeField] private float delaySlider = 0.5f;
        [SerializeField] private AnimationCurve curveSlide = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [SerializeField] private float durationSliderScaleDown = 0.3f;
        [SerializeField] private float delaySliderScaleDown = 0.5f;
        [SerializeField] private AnimationCurve curveSliderScaleDown = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private readonly Service<CardCollectionService> cardCollectionService = new();
        private AlbumType albumType;
        public override void Open(UIData uiData)
        {
            base.Open(uiData);

            if (uiData.TryGet<AlbumType>("AlbumType", out albumType))
            {
                album.Setup(albumType);

                var albumConfig = cardCollectionService.Instance.GetAlbumConfig(albumType);
                var reward = albumConfig.reward.resourceDatas[0];
                rewardItem.Init(reward.resource, reward.quantity);

            }
            PlayAnimationSlider();
            MySonatFramework.audioService.PlaySound(AudioId.Collection_Full_single_album_Grill_sort);
        }

        private void PlayAnimationSlider()
        {
            slider.value = 0;
            slider.DOValue(1, durationSlider).SetEase(curveSlide).SetDelay(delaySlider).OnComplete(() => 
            {
                MySonatFramework.audioService.PlaySound(AudioId.Collection_Unlock_rewards_Album_Grill_sort);
            });
            slider.transform.DOScale(0, durationSliderScaleDown).SetEase(curveSliderScaleDown).SetDelay(delaySliderScaleDown);

        }

        public void OnClickClaim()
        {
            Close();
            var albumConfig = cardCollectionService.Instance.GetAlbumConfig(albumType);
            var reward = albumConfig.reward;

            var logData = new EarnResourceLogData
            {
                spendType = "card_collection",
                spendId = "card_collection",
                isFirstBuy = false,
                source = "non_iap"
            };
            MySonatFramework.inventoryService.AddReward(reward, logData);

            UIData uiData = new UIData();
            uiData.Add("Title", "REWARD!");
            uiData.Add("Reward", reward);
            uiData.Add("x2", false);
            PanelManager.Instance.OpenPanel<PopupReward>(uiData);
        }
    }
}