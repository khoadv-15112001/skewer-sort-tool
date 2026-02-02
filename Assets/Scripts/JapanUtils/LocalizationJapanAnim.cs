using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class LocalizationJapanAnim : MonoBehaviour
{
    [SerializeField] private SkeletonGraphic skeletonGraphic;
    [SerializeField] private string globalSkin;
    [SerializeField] private string japanSkin;

    public bool SetupOnStart = true;
    public bool SetupOnEnable = true;

    private bool isSetup;

    private void Start()
    {
        if (!SetupOnStart || isSetup) return;
        Setup();
    }

    private void OnEnable()
    {
        if (!SetupOnEnable) return;
        Setup();
    }

    private void Setup()
    {
        isSetup = true;

        if (skeletonGraphic == null)
            return;

        bool isJP = LocalizationUtils.IsJapanese();
        string targetSkin = isJP ? japanSkin : globalSkin;

        if (string.IsNullOrEmpty(targetSkin))
            return;

        skeletonGraphic.initialSkinName = targetSkin;

        if (skeletonGraphic.Skeleton != null)
        {
            var skin = skeletonGraphic.Skeleton.Data.FindSkin(targetSkin);
            if (skin != null)
            {
                skeletonGraphic.Skeleton.SetSkin(skin);
                skeletonGraphic.Skeleton.SetSlotsToSetupPose();
            }
        }
    }
}
