using Cysharp.Threading.Tasks;
using SonatFramework.Scripts.Utils;
using UnityEngine;

namespace MyGame.Modules.CardCollection.Animation
{
    public class PopupReceiveCard_Immediately : PopupReceiveCardBase
    {
        [Header("Immediately")]
        [SerializeField] private PackAnimation packAnim;
        [SerializeField] private ParticleSystem psAppear;
        [SerializeField] private bool forceHideAnim = true;
        [SerializeField] private float animLifeTime = 2f;
        [SerializeField] private float delayAppearPs = 0.5f;
        [SerializeField] private float delayBeforePlayCardParticle = 1f;

        protected override async UniTask PlayAppearAnimation()
        {
            // Ẩn các thẻ trước
            for (int i = 0; i < uiCards.Count; i++)
            {
                uiCards[i].gameObject.SetActive(false);
            }


            var packIndex = CardPackHelper.GetPackIndex(uiCards.Count);
            packAnim.Play(packIndex, false, () =>
            {
                SonatUtils.DelayCall(delayAppearPs, () =>
                {
                    psAppear.Play();
                });
                for (int i = 0; i < uiCards.Count; i++)
                {
                    uiCards[i].gameObject.SetActive(true);
                    isCompleteAppearCard = true;
                    int idx = i;
                    SonatUtils.DelayCall(delayBeforePlayCardParticle, () =>
                    {
                        uiCards[idx].PlayParticle();
                    });
                }
            }, animLifeTime, forceHideAnim);
        }
    }
}