using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationJapanActive : MonoBehaviour
{
    [SerializeField] private GameObject globalObj;
    [SerializeField] private GameObject japanObj;

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

        if (globalObj != null)
            globalObj.SetActive(!LocalizationUtils.IsJapanese());

        if (japanObj != null)
            japanObj.SetActive(LocalizationUtils.IsJapanese());
    }
}
