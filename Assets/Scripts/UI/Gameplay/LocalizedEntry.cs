using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class LocalizedEntry<T> where T : Object
{
    public List<LocalizedEntryData<T>> entries = new List<LocalizedEntryData<T>>();

    public T GetLocalizedEntry()
    {
        foreach (var entry in entries)
        {
            if(LocalizationUtils.IsLanguage(entry.languageCode)) return entry.entry;
        }
        return entries[0].entry;
    }
}

[Serializable]
public class LocalizedEntryData<T> where T : Object
{
    public string languageCode;
    public T entry;
}
