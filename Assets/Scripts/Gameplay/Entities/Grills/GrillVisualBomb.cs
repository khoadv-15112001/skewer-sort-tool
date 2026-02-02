
using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gameplay.Entities.GrillScripts;
using TMPro;
using UnityEngine;

public class GrillVisualBomb : GrillVisual
{
    [SerializeField]
    private SpriteRenderer dynamiteImg;
    [SerializeField]
    private TMP_Text bombCountTMP;

    public void SetUpBomb(int maxMove)
    {
        bombCountTMP.text = maxMove.ToString();
        dynamiteImg.gameObject.SetActive(true);
        bombCountTMP.gameObject.SetActive(true);
    }

    public async UniTask SetBombCount(int moveCount, bool isPunch = true)
    {
        if (isPunch)
        {
            await bombCountTMP.transform.DOPunchScale(Vector3.one * 1.1f, 0.1f).SetEase(Ease.OutBack);
        }
        //await UniTask.WaitForSeconds(0.1f);
        bombCountTMP.text = moveCount.ToString();
    }
    public void ExplodeDynamite()
    {
        dynamiteImg.gameObject.SetActive(false);
    }

    public void OnComplete()
    {
        dynamiteImg.gameObject.SetActive(false);
        bombCountTMP.gameObject.SetActive(false);
        CloseGrill();
    }
}
