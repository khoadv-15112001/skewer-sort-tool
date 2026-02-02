using Cysharp.Threading.Tasks;
using DG.Tweening;
using Manager;
using Sirenix.OdinInspector;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems;
using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MyGame.Modules.CardCollection
{
    public class UICard : MonoBehaviour
    {
        [SerializeField, ReadOnly] private CardType cardType;
        [SerializeField, ReadOnly] private int numCard;
        [SerializeField] private TMP_Text txtMainName;
        [SerializeField] private TMP_Text txtNameOnBack;
        [SerializeField] private TMP_Text txtNumDuplicate;
        [SerializeField] private FixedImageRatio cardImage;
        [SerializeField] private GameObject tagNew;
        [SerializeField] private UIStarGroup starGroup;
        [SerializeField] private GameObject framePs, goldFrame;
        [SerializeField] private GameObject tagDuplicate;

        [Space]
        [Header("Enable / Disable Objects")]
        private bool isEnable = true;
        [SerializeField] private GameObject enableObj;
        [SerializeField] private GameObject disableObj;

        [Space]
        [SerializeField] private bool useParticle = false;
        [SerializeField, ShowIf("useParticle")] private CardParticle cardParticle;
        [SerializeField, ShowIf("useParticle")] private ParticleSystem psDisappear;

        [Space]
        [SerializeField] private bool showDuplicate = true;

        private readonly Service<CardCollectionService> _cardCollectionService = new();
        private CardConfig _cardConfig;

        public CardType CardType => cardType;
        public int NumCard => numCard;
        public bool IsNew => _isNew;

        private bool _isNew = false;
        public void Setup(CardType cardType)
        {
            this.cardType = cardType;

            cardImage.SetSpriteAsync(PathManager.CardSprite(cardType)).Forget();

            _cardConfig = _cardCollectionService.Instance.GetCardConfig(cardType);

            //txtMainName.text = _cardConfig.cardName;

            //cardImage.sprite = _cardConfig.sprite;

            //await UniTask.Yield();

            txtMainName.GetComponent<I2.Loc.Localize>().SetTerm(_cardConfig.cardName);

            txtMainName.SetMaterial(_cardConfig.textColorIndex);

            //txtNameOnBack.text = _cardConfig.cardName;

            txtNameOnBack.GetComponent<I2.Loc.Localize>().SetTerm(_cardConfig.cardName);

            // set star
            starGroup.Setup(_cardConfig.star);

            if (useParticle)
            {
                cardParticle.SetData(_cardConfig.star);
            }

            framePs.SetActive((int)cardType % 9 == 8);
            goldFrame.SetActive(((int)cardType % 9 == 7 || (int)cardType % 9 == 6));

            int numDuplicate = _cardCollectionService.Instance.GetDuplicatedNumCard(cardType);
            tagDuplicate.SetActive(showDuplicate && numDuplicate > 0);
            txtNumDuplicate.text = "+" + numDuplicate;
        }

        public void SetData(int quantity, bool isNew)
        {
            _isNew = isNew;
            numCard = quantity;

            tagNew.SetActive(isNew);

            SetEnable(quantity > 0);
        }

        public void UpdateData()
        {
            var isNew = _cardCollectionService.Instance.IsNewCardButNotSeen(cardType);
            var numCard = _cardCollectionService.Instance.GetNumCard(cardType);

            // check Active
            isEnable = numCard > 0;
            SetEnable(isEnable);

            tagNew.SetActive(isEnable && isNew);
        }

        private void SetEnable(bool isEnable)
        {
            enableObj.SetActive(isEnable);
            disableObj.SetActive(!isEnable);

            starGroup.SetData(isEnable);
        }

        public void OnClick()
        {
            if (isEnable)
            {
                SetEnable(false);
            }

            PanelManager.Instance.OpenPanel<PopupCard>(new UIData()
                    .Add("cardType", cardType)
                    .Add("position", transform.position)
                    .Add("onCompleteClose", (Action)(() =>
                    {
                        UpdateData();
                    }))
                );
        }

        public void PlayParticle()
        {
            if (useParticle)
            {
                cardParticle.PlayPSAppear();
            }
        }

        public void PlayParticleDisappear()
        {
            if (useParticle)
            {
                psDisappear.Play();
            }
        }
    }
}
