using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Manager;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.UIModule.UIElements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIProgress : MonoBehaviour
{
    [SerializeField] private TMP_Text txtProgress;
    [SerializeField] private Slider sliderProgress;

    [SerializeField] private FixedImageRatio icon;
    [SerializeField] private ProgressData progressData;
    private ProgressMilestone progressMilestone;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        UpdateProgress();
    }

    private void UpdateProgress()
    {
        int level = MySonatFramework.userDataService.GetLevel() - 1;
        progressMilestone = progressData.GetMilestoneData(level);
        if (progressMilestone == null)
        {
            gameObject.SetActive(false);
            return;
        }
        SetIcon();
        int progress = level - (progressMilestone.level - progressMilestone.progress);
        txtProgress.text = $"{progress - 1}/{progressMilestone.progress}";
        sliderProgress.value = (progress - 1) * 1.0f / progressMilestone.progress;
        sliderProgress.DOValue(progress * 1.0f / progressMilestone.progress, 0.3f).SetDelay(1).OnComplete(() =>
        {
            txtProgress.text = $"{progress}/{progressMilestone.progress}";
        });
        if (progress == progressMilestone.progress)
        {
            PanelManager.Instance.OpenPanel<PopupUnlockNewItem>(new UIData().Add("Icon", icon.sprite));
        }
    }

    private void SetIcon()
    {
        icon.Setup();
        switch (progressMilestone.type)
        {
            case ProgressType.Item:
                byte itemId = byte.Parse(progressMilestone.description);
                icon.SetSpriteAsync(PathManager.ItemSprite(itemId));
                break;
        }
    }
}
