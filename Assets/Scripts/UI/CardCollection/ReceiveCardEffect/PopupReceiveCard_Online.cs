using Cysharp.Threading.Tasks;
using DG.Tweening;
using GrillSort.OnlineService;
using SonatFramework.Scripts.UIModule;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MyGame.Modules.CardCollection.Animation
{
    public class PopupReceiveCard_Online : PopupReceiveCardBase
    {
        [SerializeField] private GameObject title;

        private List<UICardSender> uiSenders = new();

        public override void Open(UIData uiData)
        {
            base.Open(uiData);

            uiSenders = GetComponentsInChildren<UICardSender>().ToList();

            List<UserProfile> senders = uiData.Get<List<UserProfile>>("senders");

            for (int i = 0; i < uiCards.Count; i++)
            {
                uiSenders[i].Bind(senders[i]);
            }
        }

        public override List<CardType> GetCardList()
        {
            return uiData.Get<List<CardType>>("cards");
        }

        protected override async UniTask PlayAppearAnimation()
        {
            for (int i = 0; i < uiCards.Count; i++)
            {
                uiCards[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < uiCards.Count; i++)
            {
                uiCards[i].gameObject.SetActive(true);
                int idx = i;

                _ = uiCards[i].transform.DOScale(1f, 0.5f).From(0).OnComplete(() => uiCards[idx].PlayParticle());

                await UniTask.Delay(100);
            }

            isCompleteAppearCard = true;
        }

        protected override async UniTask PlayCollect()
        {
            foreach (var uiSender in uiSenders)
            {
                uiSender.gameObject.SetActive(false);
            }

            title.SetActive(false);

            await base.PlayCollect();
        }
    }
}