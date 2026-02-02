using DG.Tweening;
using SonatFramework.Scripts.UIModule;
using Unity.VisualScripting;
using UnityEngine;
public class PopupRewardWinstreak : PopupReward
{
    public Transform rewardTF;
    //public float delayShowRewards = 1.5f;
    public GameObject btnClaim;
    public ParticleSystem[] particleIdles;
    //public ParticleSystem particleOpen;

    public float delayShowEffectOpen = 1.5f;
    public float delayShowClaim = 2f;
    public SpineAnimationController bagAnim;
    public AnimState animStateAppear;

    public override void Open(UIData uiData)
    {
        base.Open(uiData);

        btnClaim.SetActive(false);

        for (int i = 0; i < particleIdles.Length; i++) 
        { 
            particleIdles[i].gameObject.SetActive(false); 
        };

        //if (uiData != null && uiData.TryGet("bagType", out var bagType)) { }
        //bagAnim.skeletonGraphic.initialSkinName = bagType.ToString();
        rewardTF.localScale = UnityEngine.Vector3.zero;
        //rewardTF.DOScale(1, 0.3f).SetDelay(delayShowRewards);

        DOVirtual.DelayedCall(delayShowEffectOpen, () => 
        {
            for (int i = 0; i < particleIdles.Length; i++) 
            { 
                particleIdles[i].gameObject.SetActive(true); particleIdles[i].Play(); 
            };

            rewardTF.DOScale(1, 0.3f);
        });

        bagAnim.PlayOnceThenLoop(animStateAppear);

        DOVirtual.DelayedCall(delayShowClaim, () =>
        {
            btnClaim.SetActive(true);
        });
    }
}
