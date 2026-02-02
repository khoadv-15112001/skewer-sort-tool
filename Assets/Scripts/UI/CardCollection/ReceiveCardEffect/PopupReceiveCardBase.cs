using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.InventoryManagement.GameResources;
using SonatFramework.Systems.ObjectPooling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MyGame.Modules.CardCollection
{

    public abstract class PopupReceiveCardBase : Panel
    {
        public const string REWARD_DATA_KEY = "RewardData";
        [SerializeField] private RectTransform widgetCardBox;
        [SerializeField] private Transform targetStarPos;
        [SerializeField] protected Button btnClaim;
        [SerializeField] private TMP_Text txtStar;

        [Header("Appear Cards")]
        [SerializeField] private Transform container;
        [SerializeField] private int maxRow = 2;
        [SerializeField] private int maxCol = 3;


        [Header("Animation")]
        [SerializeField] private string collectEffectName = "CollectResourceMultipleStar_Card";
        [SerializeField] private int xGiftTarget = -200;
        [SerializeField] private float durationSwitchStar = 0.1f;
        [SerializeField] private float delayBeforeBoxIn = 0.3f;
        [SerializeField] private float delayBeforeBoxOut = 0.3f;
        [SerializeField] private float durationMoveToBox = 0.5f;
        [SerializeField] private float delayBeforeHideCard = 0.3f;
        [SerializeField] private float delayBeforeFlyStar = 0.3f;
        [SerializeField] private float delayBetweenFlyStar = 0.1f;
        [SerializeField] private float starScaleDown = 0.4f;

        [Space(10)]

        private readonly Service<PoolingContainerService> _poolingContainerService = new();
        private readonly Service<CardCollectionService> _cardCollectionService = new();

        protected RewardData rewardData;
        protected List<UICard> uiCards = new();
        protected List<Transform> targetRoots = new();
        protected List<Transform> rootParents = new();
        protected List<UICardStar> uiCardStars = new();



        protected bool isCompleteAppearCard = false;
        protected bool isCollected = false;

        public override void Open(UIData uiData)
        {
            base.Open(uiData);

            if (uiData != null)
            {
                rewardData = uiData.Get<RewardData>(REWARD_DATA_KEY) ?? new();
            }

            SetupCards();

            PlayAppearAnimation().Forget();
            widgetCardBox.anchoredPosition = new Vector2(0, widgetCardBox.anchoredPosition.y);
            isCompleteAppearCard = false;
            isCollected = false;

            UpdateTextStar();
        }

        public void UpdateTextStar()
        {
            txtStar.text = _cardCollectionService.Instance.CardStar.ToString();
        }

        protected abstract UniTask PlayAppearAnimation();


        #region setup
        private void SetupCards()
        {
            foreach (Transform child in container)
            {
                _poolingContainerService.Instance.CleanContainer(child);
            }
            _poolingContainerService.Instance.CleanContainer(container);

            uiCards.Clear();
            targetRoots.Clear();
            rootParents.Clear();
            uiCardStars.Clear();

            //foreach (var item in rewardData.resourceDatas)
            //{
            //    Debug.Log($"[RewardData] resource enum = {item.resource}, int = {(int)item.resource}, type = {GameResourceHelper.ResourceType(item.resource)}");
            //}

            var cardList = GetCardList();

            // Tổng số card
            int totalCards = cardList.Count;

            // Loại unique
            int uniqueTypes = cardList.Distinct().Count();

            // Log đẹp
            Debug.Log($"[ReceiveCardEffect] Pack: {uniqueTypes} unique types, total {totalCards} cards.");
            foreach (var card in cardList)
            {
                Debug.Log($"  - {card}");
            }

            var size = UIHelper.GetGridSize(cardList.Count, maxRow, maxCol);

            int count = 0;
            foreach (var cardType in cardList)
            {
                var row = count / size.col;
                var col = count % size.col;
                count++;

                // Tạo parent row nếu chưa có
                if (rootParents.Count <= row)
                {
                    var rootParent = _poolingContainerService.Instance.CreateObject<Transform>(container);
                    rootParents.Add(rootParent);
                }

                // Tạo card container trong row
                var root = _poolingContainerService.Instance.CreateObject<Transform>(rootParents[row]);
                targetRoots.Add(root);

                // Lấy component UICard
                var card = root.GetComponentInChildren<UICard>();
                card.Setup(cardType);

                // Mỗi phần tử list là 1 thẻ duy nhất ⇒ numCard luôn = 1
                card.SetData(1, _cardCollectionService.Instance.IsNewCard(cardType));
                uiCards.Add(card);

                // Xử lý UICardStar (ẩn mặc định)
                var cardStar = root.GetComponentInChildren<UICardStar>();
                var numStar = CardPackHelper.GetNumStarReward(cardType);
                cardStar.gameObject.SetActive(false);
                cardStar.SetData(numStar);
                uiCardStars.Add(cardStar);

                // Add vào collection (1 thẻ)
                _cardCollectionService.Instance.AddCard(cardType, 1);
            }

        }

        public virtual List<CardType> GetCardList()
        {
            return CardPackHelper.GetCardReward(rewardData);
        }

        #endregion

        #region Claim
        public virtual void OnClickClaim()
        {
            if (isCollected || !isCompleteAppearCard) return;
            isCollected = true;
            PlayCollect().Forget();

            btnClaim.gameObject.SetActive(false);
        }


        private List<UICardStar> _activeCardStars = new();

        // BUG: hard code sửa anim/efx
        protected virtual async UniTask PlayCollect()
        {
            _activeCardStars.Clear();

            MySonatFramework.audioService.PlaySound(AudioId.Card_Disappear_Grill_sort);
            // biến card dư thành star
            await ExchangeCardEffect();

            // widget xuất hiện và star bay vào
            if (CheckOldCard())
            {
                await UniTask.Delay((int)(delayBeforeBoxIn * 1000));
                await widgetCardBox.DOAnchorPosX(xGiftTarget, durationMoveToBox).SetEase(Ease.OutBack);
                await UniTask.Delay((int)(delayBeforeFlyStar * 1000));

                // new card (card còn lại) biến mất
                DisplayCardDisappear();

                // sau đó mới fly star
                foreach (var star in _activeCardStars)
                {
                    FlyToWidgetEffect(star).Forget();
                    await UniTask.Delay((int)(delayBetweenFlyStar * 1000));
                }
                await UniTask.Delay((int)(delayBeforeHideCard * 1000));
            }
            else
            {
                DisplayCardDisappear();
                await UniTask.Delay((int)(delayBeforeHideCard * 1000));
            }

            await UniTask.Delay((int)(delayBeforeBoxOut * 1000));
            await widgetCardBox.DOAnchorPosX(0, durationMoveToBox).SetEase(Ease.OutBack);
            Close();
        }

        private void DisplayCardDisappear()
        {
            foreach (var card in uiCards)
            {
                if (card.IsNew)
                {
                    card.transform.DOScale(0, durationSwitchStar).SetEase(Ease.InBack).OnComplete(() =>
                    {
                        card.PlayParticleDisappear();
                    });
                }
            }
        }

        private bool CheckOldCard()
        {
            return uiCards.Any(card => card.IsNew == false);
        }

        private async UniTask ExchangeCardEffect()
        {
            for (int i = 0; i < uiCards.Count; i++)
            {
                var card = uiCards[i];
                var star = uiCardStars[i];

                if (card.IsNew == false)
                {
                    //card bị ẩn
                    SwitchStar(card, star);
                    _activeCardStars.Add(star);
                }
            }
        }

        private void SwitchStar(UICard card, UICardStar star)
        {
            star.transform.position = card.transform.position;
            star.gameObject.SetActive(true);
            card.transform.DOScale(0, durationSwitchStar).SetEase(Ease.InBack);
            star.transform.DOScale(1, durationSwitchStar).SetEase(Ease.OutBack).From(0).SetDelay(durationSwitchStar);
        }

        private async UniTask FlyToWidgetEffect(UICardStar star)
        {
            var startPos = star.transform.position;
            startPos.z = 0;

            var targetPos = targetStarPos.position;
            targetPos.z = 0;

            var effectStar = await SonatSystem.GetService<PoolingServiceAsync>().CreateAsync<UICollectEffectItemMultiple>(
                                "CollectResourceMultipleStar_Card",
                                PanelManager.Instance.transform,
                                GameResource.None,
                                star.GetQuantity(),
                                startPos,
                                startPos,
                                targetPos,
                                null,
                                1f,
                                starScaleDown
                           );

            star.Hide(() =>
            {
                MySonatFramework.audioService.PlaySound(AudioId.Items_Fly_Whoosh);

                effectStar.PlayEffect();

                _cardCollectionService.Instance.AddCardStar(star.GetQuantity());
                star.Resset();
                SonatUtils.DelayCall(0.25f, () =>
                {
                    MySonatFramework.audioService.PlaySound(AudioId.Stars_Fill_Grill_sort);
                    UpdateTextStar();
                });
            });
        }
        #endregion
    }
}