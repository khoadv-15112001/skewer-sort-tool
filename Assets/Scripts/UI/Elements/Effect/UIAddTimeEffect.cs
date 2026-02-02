using System;
using DG.Tweening;
using SonatFramework.Systems.ObjectPooling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAddTimeEffect : MonoBehaviour, IPoolingObject
{
    [SerializeField] private Image iconClockPreBooster;
    [SerializeField] private Image iconClockConsecutiveWin;
    [SerializeField] private float duration = 0.75f;
    [SerializeField] private float durationTxt = 1;
    [SerializeField] private float durationScale = 0.5f;
    [SerializeField] private float heightTxt = 100f;
    [SerializeField] private float scaleDown = 0.25f;
    [SerializeField] private Transform target;
    [SerializeField] private Transform icon;
    [SerializeField] private Transform pToast;
    [SerializeField] private TMP_Text[] txtToast;
    [SerializeField] private CanvasGroup canvasGroup;

    public void Setup(Vector3 startPosition = default, int time = 0, Action action = null, float delay = 0)
    {
        ResetRect();
        // effect toast
        foreach (var txt in txtToast)
        {
            txt.text = $"+{time}s";
        }
        pToast.gameObject.SetActive(false);
        icon.gameObject.SetActive(false);
        DOVirtual.DelayedCall(delay, () =>
        {
            icon.gameObject.SetActive(true);
            // effect jump
            icon.transform.position = startPosition;
            icon.transform.DOScale(scaleDown, duration);
            icon.transform.DOJump(target.position, 1.75f, 1, duration).OnComplete(() =>
            {
                action?.Invoke();
                icon.gameObject.SetActive(false);
                CreateToast();
            });
        });

    }

    private void CreateToast()
    {
        pToast.gameObject.SetActive(true);
        pToast.transform.localPosition = Vector3.zero;
        pToast.transform.localScale = Vector3.one * 0.5f;
        pToast.transform.DOScale(1, durationScale);
        pToast.transform.DOLocalJump(Vector3.up * heightTxt, 1, 1, durationTxt).SetEase(Ease.InSine).OnComplete(() =>
        {
            MySonatFramework.GetService<PoolingService>().ReturnObj(this);
        });
    }

    public void Setup()
    {

    }

    public void OnCreateObj(params object[] args)
    {
        transform.localScale = Vector3.one;
        icon.transform.localScale = Vector3.one;
        icon.transform.localPosition = Vector3.zero;
        canvasGroup.alpha = 1;

        if (args.Length > 0)
        {
            var type = args[0] as string;
            switch (type)
            {
                case "PreBooster":
                    iconClockPreBooster.gameObject.SetActive(true);
                    iconClockConsecutiveWin.gameObject.SetActive(false);
                    break;
                case "ConsecutiveWin":
                    iconClockPreBooster.gameObject.SetActive(false);
                    iconClockConsecutiveWin.gameObject.SetActive(true);
                    break;
            }
        }

    }

    public void OnReturnObj()
    {

    }



    public void ResetRect()
    {
        var rect = GetComponent<RectTransform>();

        // Đặt anchors full stretch (chiếm toàn bộ parent)
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;

        // Đặt offsets (khoảng cách với viền parent) về 0
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // Quan trọng: reset position & sizeDelta để tránh bị lệch
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;

        // Reset pivot, rotation, scale
        rect.pivot = new Vector2(0.5f, 0.5f);
        target.localRotation = Quaternion.identity;
        target.localScale = Vector3.one;
    }
}
