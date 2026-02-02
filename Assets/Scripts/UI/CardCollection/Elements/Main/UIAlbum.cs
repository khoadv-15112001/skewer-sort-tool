using Cysharp.Threading.Tasks;
using DG.Tweening;
using I2.Loc;
using Manager;
using Sirenix.OdinInspector;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MyGame.Modules.CardCollection
{
    public class UIAlbum : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField, ReadOnly] private AlbumType albumType;
        [SerializeField] private TMP_Text albumName;
        [SerializeField] private Image albumImage;
        [SerializeField] private UIRewardItem rewardItem;

        [Header("Data")]
        [SerializeField] private Slider slider;
        [SerializeField] private TMP_Text txtNumCard;
        [SerializeField] private CardNotificationBadge cardNotificationBadge;

        [Header("Completed")]
        [SerializeField] private GameObject completedObj;
        [SerializeField] private GameObject notCompletedObj;

        // [Header("Status")]
        // [SerializeField] private GameObject fullObj;
        // [SerializeField] private GameObject lockObj;
        // [SerializeField] private GameObject normalObj;

        protected readonly Service<CardCollectionService> cardCollectionService = new();
        protected AlbumConfig albumConfig;
        public AlbumType AlbumType => albumType;

        public virtual void Setup(AlbumType albumType)
        {
            this.albumType = albumType;
            albumConfig = cardCollectionService.Instance.GetAlbumConfig(albumType);

            //albumName.text = albumConfig.albumName;
            albumName.GetComponent<Localize>().SetTerm(albumConfig.albumName);
            albumName.SetMaterial(albumConfig.color);

            //albumImage.sprite = albumConfig.albumIcon;
            albumImage.SetSpriteAsync(PathManager.AlbumIcon(AlbumType)).Forget();
            // set reward
            var reward = albumConfig.reward.resourceDatas[0];
            rewardItem.Init(reward.resource, reward.quantity);

            if (cardNotificationBadge)
            {
                cardNotificationBadge.SetData(albumType);
            }
        }

        public virtual void SetData(int numCard, int totalCard)
        {
            slider.value = numCard / (float)totalCard;

            txtNumCard.text = $"{numCard}/{totalCard}";

            // set completed
            if (completedObj != null) completedObj.SetActive(numCard >= totalCard);
            if (notCompletedObj != null) notCompletedObj.SetActive(numCard < totalCard);
        }

        public virtual void UpdateData()
        {
            var numCard = cardCollectionService.Instance.GetNumCardInAlbum(albumType);
            var totalCard = albumConfig.cards.Count;
            SetData(numCard, totalCard);
        }

        public void OnClick()
        {
            //Debug.Log("OnClick Album");
            PanelManager.Instance.OpenPanel<PopupAlbum>(new UIData().Add("albumType", albumType));

            cardNotificationBadge.gameObject.SetActive(false);
        }
    }
}
