using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalizationJapanSpriteImage : MonoBehaviour
{
    [SerializeField] private Image spriteImage;
    [SerializeField] private Sprite globalSP;
    [SerializeField] private Sprite japanSP;

    public bool SetupOnStart = true;
    public bool SetupOnEnable = true;
    public bool SetNativeSize = false;

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

        if (spriteImage == null)
            return;

        bool isJP = LocalizationUtils.IsJapanese();

        if (isJP && japanSP != null)
            spriteImage.sprite = japanSP;

        if (!isJP && globalSP != null)
            spriteImage.sprite = globalSP;

        if (SetNativeSize)
            spriteImage.SetNativeSize();
    }
}
