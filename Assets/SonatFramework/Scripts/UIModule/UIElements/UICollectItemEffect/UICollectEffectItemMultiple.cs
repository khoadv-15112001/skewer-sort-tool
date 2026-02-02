using System;
using DG.Tweening;
using Sonat.Enums;
using SonatFramework.Systems;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.UI;

public class UICollectEffectItemMultiple : UICollectEffectItem
{
    private Vector3 tempPos;
    private float scaleDown = 1.0f;
    [SerializeField] private float timeMoveOut = 0.2f;
    [SerializeField] private float defaultScaleDown = 0.5f;
    [SerializeField] private float timeDelay = 0.3f;
    [SerializeField] private bool random;
    [SerializeField] private float randomAmplitude = 0.1f;
    [SerializeField] private float delayRotate = 0f;

    [SerializeField] private int numCircle = 1;
    [SerializeField] private AnimationCurve rotateCurve;

    [Header("Play On Awake")]
    [SerializeField] private bool playOnAwake = true;

    [Header("Destroy")]
    [SerializeField] private Image iconImage;
    [SerializeField] private float delayToDestroyEfffect = 0.8f;
    [SerializeField] private AudioId collectSound;

    [Header("Particle")]
    [SerializeField] private ParticleSystem psDisappear;

    private void ResetState()
    {
        transform.DOKill(true);
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
        transform.localPosition = Vector3.zero;
    }

    protected override void DOEffect()
    {
        transform.DOMove(tempPos, timeMoveOut).onComplete += () =>
        {
            float rand = random ? Random.Range(0f, randomAmplitude) : 0;
            DOVirtual.DelayedCall(timeDelay + rand, () =>
            {
                //transform.DOMove(tempPos, timeMoveOut);

                float customDuration = speed > 0 ? Vector3.Distance(tempPos, targetPosition) / speed : duration;
                transform.DOScale(scaleDown, customDuration).SetEase(Ease.InOutSine);
                transform.DOMoveX(targetPosition.x, customDuration).SetEase(moveXCurve);
                transform.DOMoveY(targetPosition.y, customDuration).SetEase(moveYCurve).OnComplete(() =>
                {
                    onCollect?.Invoke();
                    //SonatSystem.GetService<PoolingServiceAsync>().ReturnObj(this);
                    OffIcon();
                });
                if (rotate)
                {
                    transform.DORotate(new Vector3(0, 0, numCircle * 360), customDuration, RotateMode.FastBeyond360).SetEase(rotateCurve).SetDelay(delayRotate);
                }

                if (collectSound != AudioId.None)
                    MySonatFramework.audioService.PlaySound(collectSound);
            });
        };
    }

    private void OffIcon()
    {
        if (uiResourceItem.CurrentVisualObject != null)
            uiResourceItem.CurrentVisualObject.SetActive(false);
        if (iconImage != null) iconImage.gameObject.SetActive(false);
        if (psDisappear != null) psDisappear.Play();
        DOVirtual.DelayedCall(delayToDestroyEfffect, () => SonatSystem.GetService<PoolingServiceAsync>().ReturnObj(this));
    }

    public void Setup()
    {
    }

    public override void OnCreateObj(params object[] args)
    {
        ResetState();

        if (iconImage != null) iconImage.gameObject.SetActive(true);

        // Parse arguments first
        GameResource resource = (GameResource)args[0];
        int quantity = (int)args[1];
        startPosition = (Vector3)args[2];
        tempPos = (Vector3)args[3];
        targetPosition = (Vector3)args[4];
        if (args.Length > 5)
            onCollect = (Action)args[5];
        else
        {
            onCollect = null;
        }
        if (args.Length > 6)
            transform.localScale = Vector3.one * (float)args[6];
        else
        {
            transform.localScale = Vector3.one;
        }

        if (args.Length > 7)
            scaleDown = (float)args[7];
        else
        {
            scaleDown = defaultScaleDown;
        }

        transform.position = startPosition;
        
        // ✅ Set data TRƯỚC khi dùng CurrentVisualObject
        uiResourceItem.SetData(resource, quantity);
        
        // ✅ Bây giờ mới an toàn để set active
        if (uiResourceItem.CurrentVisualObject != null)
        {
            uiResourceItem.CurrentVisualObject.SetActive(true);
        }

        if (playOnAwake)
            DOEffect();
    }

    public void PlayEffect()
    {
        DOEffect();
    }
}