using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalizedObject : MonoBehaviour
{
    [Serializable]
    public class LocalizedObjectData
    {
        public string languageCode;
        public GameObject[] objects;
    }

    [SerializeField] private List<LocalizedObjectData> localizedObjects;

    private void OnEnable()
    {
        CheckLocalized();
        GameplayController.OnLoadLevel += OnLevelLoaded;
    }

    private void OnDisable()
    {
        GameplayController.OnLoadLevel -= OnLevelLoaded;
    }

    private void OnLevelLoaded(int level)
    {
        CheckLocalized();
    }

    private void CheckLocalized()
    {
        bool hasLanguage = false;
        foreach (var data in localizedObjects)
        {
            var enable = LocalizationUtils.IsLanguage(data.languageCode);
            foreach (var obj in data.objects) obj.SetActive(enable);
            if (!hasLanguage && enable) hasLanguage = true;
        }

        if (!hasLanguage && localizedObjects.Count > 0)
        {
            foreach (var obj in localizedObjects[0].objects) obj.SetActive(true);
        }
    }
}