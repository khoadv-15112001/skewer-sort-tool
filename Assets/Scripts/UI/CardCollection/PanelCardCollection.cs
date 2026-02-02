using System.Collections.Generic;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems;
using SonatFramework.Systems.ObjectPooling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MyGame.Modules.CardCollection
{
    public class CardCollectionPanel : MonoBehaviour
    {
        [SerializeField] private Transform container;

        [Header("Info")]
        [SerializeField] private Slider cardSlider;
        [SerializeField] private GameObject tickObj;
        [SerializeField] private TMP_Text txtTotalCard;
        [SerializeField] private TMP_Text txtStar;
        [SerializeField] private UIRewardGroup rewardGroup;
        [SerializeField] private UITimeCounter timeCounter;
        [SerializeField] private I2.Loc.LocalizationParamsManager levelUnlockParamsManager;

        private readonly Service<PoolingContainerService> _poolingContainerService = new();
        private readonly Service<CardCollectionService> _cardCollectionService = new();
        private List<UIAlbum> _albums = new();

        private void OnEnable()
        {
            if (_cardCollectionService.Instance.IsUnlocked() == false)
            {
                gameObject.SetActive(false);
                if (levelUnlockParamsManager)
                    levelUnlockParamsManager.SetParameterValue("VALUE", $"{_cardCollectionService.Instance.config.unlockLevel}");
                return;
            }

            _poolingContainerService.Instance.CleanContainer(container);
            foreach (var album in _cardCollectionService.Instance.config.albums)
            {
                var albumObj = _poolingContainerService.Instance.CreateObject<UIAlbum>(container);
                albumObj.Setup(album.type);
                _albums.Add(albumObj);
            }

            var reward = _cardCollectionService.Instance.config.rewardInSeason;
            rewardGroup.SetData(reward);

            UpdateData();

            _cardCollectionService.Instance.OnNewCardCountChanged += UpdateData;
            _cardCollectionService.Instance.OnChangeData += UpdateData;

        }

        private void OnDisable()
        {
            _cardCollectionService.Instance.OnNewCardCountChanged -= UpdateData;
            _cardCollectionService.Instance.OnChangeData -= UpdateData;
        }

        private void UpdateData()
        {
            var totalCard = _cardCollectionService.Instance.TotalCard;
            var maxCard = _cardCollectionService.Instance.config.GetNumCard();

            tickObj.SetActive(totalCard >= maxCard);
            cardSlider.value = totalCard * 1.0f / maxCard;
            txtTotalCard.text = $"{totalCard}/{maxCard}";

            txtStar.text = _cardCollectionService.Instance.CardStar.ToString();

            foreach (var album in _albums)
            {
                album.UpdateData();
            }

            timeCounter.SetData(_cardCollectionService.Instance.GetRemainTime(), null);
        }
    }
}
