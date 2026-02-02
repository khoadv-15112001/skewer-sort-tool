using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using UnityEngine;

public class LocalizationJapanSpriteRenderer : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite globalSP;
    [SerializeField] private Sprite japanSP;

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
        
        if(spriteRenderer == null)
            return;

        bool isJP = LocalizationUtils.IsJapanese();

        if(isJP && japanSP != null)
            spriteRenderer.sprite = japanSP;

        if (!isJP && globalSP != null)
            spriteRenderer.sprite = globalSP;
    }
}
