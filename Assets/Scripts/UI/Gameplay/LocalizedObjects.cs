using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

public class LocalizedObjects : MonoBehaviour
{
    [SerializeField] private List<LocalizeObjectData> localizedObjects = new List<LocalizeObjectData>();

    private void OnEnable()
    {
        CheckLocalize();
        LocalizationManager.OnLocalizeEvent += OnLanguageChanged;
    }

    private void OnDisable()
    {
        LocalizationManager.OnLocalizeEvent -= OnLanguageChanged;
    }

    private void OnLanguageChanged()
    {
        CheckLocalize();
    }

    private void CheckLocalize()
    {
        bool localized = false;
        foreach (var localizeObjectData in localizedObjects)
        {
            if (LocalizationUtils.IsLanguage(localizeObjectData.languageCode))
            {
                foreach (var obj in localizeObjectData.objects)
                {
                    obj.SetActive(true);
                }

                localized = true;
            }
            else
            {
                foreach (var obj in localizeObjectData.objects)
                {
                    obj.SetActive(false);
                }
            }
        }

        if (localized) return;

        foreach (var obj in localizedObjects[0].objects)
        {
            obj.SetActive(true);
        }
    }
}


[Serializable]
public class LocalizeObjectData
{
    public string languageCode;
    public GameObject[] objects;
}