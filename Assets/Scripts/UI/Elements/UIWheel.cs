using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using TMPro;
using UnityEngine;

public class UIWheel : MonoBehaviour
{
    [SerializeField] private RectTransform pArrow;
    [SerializeField] private float duration = 2f;
    [SerializeField] private bool isRotate = true;
    [SerializeField, ShowIf("isRotate")] private float maxAngle = 25f;
    [SerializeField, ShowIf("@!isRotate")] private RectTransform startPoint;
    [SerializeField, ShowIf("@!isRotate")] private RectTransform endPoint;

    private Tween _wheelTween;
    private Vector2 _centerPosition;
    // Start is called before the first frame update
    void Awake()
    {
        _centerPosition = (startPoint.anchoredPosition + endPoint.anchoredPosition) / 2;
    }

    private void OnDisable()
    {
        StopWheel();
    }

    private void OnEnable()
    {
        StartWheel();
    }

    private void StartWheel()
    {
        _wheelTween?.Kill();
        if (isRotate)
        {
            pArrow.localRotation = Quaternion.Euler(0, 0, -maxAngle);
            _wheelTween = pArrow.DORotate(new Vector3(0, 0, maxAngle), duration / 2)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
        else
        {
            pArrow.anchoredPosition = startPoint.anchoredPosition;
            _wheelTween = pArrow.DOAnchorPos(endPoint.anchoredPosition, duration / 2)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
    }

    public void StopWheel()
    {
        _wheelTween?.Kill();
    }

    public float GetWheelValue()
    {
        if (isRotate)
        {
            return GetWheelValue_Rotate();
        }
        else
        {
            return GetWheelValue_Move();
        }
    }

    private float GetWheelValue_Move()
    {
        // Debug.Log($"pArrow.anchoredPosition: {pArrow.anchoredPosition}");
        // Debug.Log($"_originalPosition: {_centerPosition}");
        var distance = Vector3.Distance(pArrow.anchoredPosition, _centerPosition);
        if (distance < 67)
        {
            return 5;
        }
        else if (distance < 202)
        {
            return 3;
        }
        else
        {
            return 2;
        }
    }

    public float GetWheelValue_Rotate()
    {
        var rotation = pArrow.localRotation.eulerAngles.z;
        rotation = (rotation > 180) ? rotation - 360 : rotation; // Normalize to [-180, 180]
        if (rotation < -18.5f || rotation > 18.5f)
        {
            return 2;
        }
        else if (rotation < -6.5f || rotation > 6.5f)
        {
            return 3;
        }
        else
        {
            return 5;
        }
    }

    //private void UpdateProgress()
    //{
    //    int level = MySonatFramework.userDataService.GetLevel() - 1;
    //    progressMilestone = progressData.GetMilestoneData(level);
    //    if (progressMilestone == null)
    //    {
    //        gameObject.SetActive(false);
    //        return;
    //    }
    //    SetIcon();
    //    int progress = level - (progressMilestone.level - progressMilestone.progress);
    //    txtProgress.text = $"{progress - 1}/{progressMilestone.progress}";
    //    sliderProgress.value = (progress - 1) * 1.0f / progressMilestone.progress;
    //    sliderProgress.DOValue(progress * 1.0f / progressMilestone.progress, 0.3f).SetDelay(1).OnComplete(() =>
    //    {
    //        txtProgress.text = $"{progress}/{progressMilestone.progress}";
    //    });
    //    if (progress == progressMilestone.progress)
    //    {
    //        PanelManager.Instance.OpenPanel<PopupUnlockNewItem>(new UIData().Add("Icon", icon.sprite));
    //    }
    //}

    //private void SetIcon()
    //{
    //    icon.Setup();
    //    switch (progressMilestone.type)
    //    {
    //        case ProgressType.Item:
    //            byte itemId = byte.Parse(progressMilestone.description);
    //            icon.SetSpriteAsync(PathManager.ItemSprite(itemId));
    //            break;
    //    }
    //}
}
