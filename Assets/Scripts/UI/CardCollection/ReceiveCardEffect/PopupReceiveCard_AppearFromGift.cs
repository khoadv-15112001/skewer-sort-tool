using Cysharp.Threading.Tasks;
using DG.Tweening;
using MyGame.Modules.CardCollection;
using UnityEngine;

public class PopupReceiveCard_AppearFromGift : PopupReceiveCardBase
{
    [SerializeField] private Transform containerGift;

    [Header("Animation")]
    [SerializeField] private float duration = 0.3f;
    [SerializeField] private AnimationCurve curveScale = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private AnimationCurve curveMove = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private AnimationCurve curveRotate = AnimationCurve.Linear(0, 0, 1, 1);

    protected override async UniTask PlayAppearAnimation()
    {
        // đặt các card về vị trí cái túi
        for (int i = 0; i < uiCards.Count; i++)
        {
            uiCards[i].transform.SetParent(containerGift);
            uiCards[i].transform.localPosition = Vector3.zero;
        }
        
        await UniTask.Delay(3000);

        // sau đó bắn lần lượt ra
        for (int i = 0; i < uiCards.Count; i++)
        {
            var root = targetRoots[i];
            uiCards[i].transform.SetParent(root);
            PlayEachCardAnimation(uiCards[i]);
            await UniTask.Delay(100);
        }
    }

    private void PlayEachCardAnimation(UICard card)
    {
        card.transform.DOLocalMove(Vector3.zero, duration).SetEase(curveMove);
        card.transform.DORotate(Vector3.zero, duration, RotateMode.FastBeyond360).SetEase(curveRotate);
    }
}