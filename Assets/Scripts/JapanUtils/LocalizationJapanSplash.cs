using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationJapanSplash : MonoBehaviour
{
    [SerializeField] private GameObject global;
    [SerializeField] private GameObject japan;

    private void Start()
    {
        bool isJapan = LocalizationUtils.IsLocalizeJapanSplash();

        if (global)
            global.SetActive(!isJapan);
        if (japan)
            japan.SetActive(isJapan);
    }
}
