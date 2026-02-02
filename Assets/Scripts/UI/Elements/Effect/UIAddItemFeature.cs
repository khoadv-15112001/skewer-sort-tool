using DG.Tweening;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems.ObjectPooling;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAddItemFeature : MonoBehaviour, IPoolingObject
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text txtValue;

    RectTransform _rt;  // RectTransform của effect

    [Header("Config")]
    [SerializeField] private float offsetX = 150f;
    [SerializeField] private float appearDuration = 0.3f;
    [SerializeField] private float jumpDuration = 0.6f;
    [SerializeField] private float jumpPower = 1f;
    [SerializeField] private float endScale = 0.3f;
    [SerializeField] private bool mirrorToCenter = true;
    [SerializeField] private Ease ease = Ease.OutQuad;

    public void Setup(int value, Sprite sprite, Vector3 curWorldPos, Action action = null, float delay = 0f, Vector2 size = default)
    {
        if (_rt == null)
            _rt = (RectTransform)transform;

        if (size != default)
            _rt.sizeDelta = size;

        txtValue.gameObject.SetActive(value != 0);

        txtValue.text = $"+{value}";

        icon.sprite = sprite;

        transform.position = curWorldPos;

        PlayWidgetEffectAnchored(curWorldPos, action, delay);
    }
    public void PlayWidgetEffectAnchored(Vector2 widgetAnchoredPos, Action onDone = null, float delay = 0)
    {
        _rt.DOKill(true);

        // chọn phía đặt effect: bên phải nếu widget ở x < 0, ngược lại bên trái (mirror)
        float dir = mirrorToCenter ? (widgetAnchoredPos.x < 0f ? +1f : -1f) : +1f;

        // start nằm ngang với widget (cùng Y), lệch X một đoạn offsetX
        Vector2 start = widgetAnchoredPos + new Vector2(dir * offsetX, 0f);

        // setup
        _rt.anchoredPosition = start;
        _rt.localScale = Vector3.zero;
        gameObject.SetActive(false);

        DOVirtual.DelayedCall(delay, () =>
        {
            gameObject.SetActive(true);

            // scale 0 -> 1 (xuất hiện)
            _rt.DOScale(1f, appearDuration).SetEase(Ease.OutBack).OnComplete(() =>
            {
                // nhảy theo vòng cung (parabol) tới widget
                var seq = DOTween.Sequence();
                seq.Join(_rt.DOJumpAnchorPos(widgetAnchoredPos, jumpPower, 1, jumpDuration).SetEase(ease));
                seq.Join(_rt.DOScale(endScale, jumpDuration).SetEase(ease));
                seq.OnComplete(() =>
                {
                    onDone?.Invoke();
                    gameObject.SetActive(false);
                });
            });
        });
    }

    public void OnCreateObj(params object[] args)
    {
    }

    public void OnReturnObj()
    {

    }
    public void Setup()
    {
    }

    void LogX(Transform t, string tag)
    {
        var p = t.position;
        var lp = t.localPosition;
        var rot = t.rotation.eulerAngles;
        var s = t.lossyScale;
        Debug.Log($"{tag} | world:{p} local:{lp} rot:{rot} lossyScale:{s} parent:{t.parent?.name}");
    }

}
