using I2.Loc;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class UIMultiEventLoseController : MonoBehaviour
{
    public enum EFormat
    {
        Failed_Lose,
        Lose_Failed,
        Failed_Failed,
        Lose_Lose
    }
    public enum EVerbType
    {
        Failed,
        Lose
    }


    [Serializable]
    public class TermData
    {
        public EFormat format;
        [ValueDropdown(nameof(GetAllTerms))] public string term;

        public static IEnumerable<string> GetAllTerms()
        {
#if UNITY_EDITOR
            var sourcePath = LocalizationManager.GlobalSources[0];
            var asset = Resources.Load<LanguageSourceAsset>(sourcePath);
            if (asset == null || asset.mSource == null)
                return new[] { "(source not found or invalid)" };

            return asset.mSource
                .GetTermsList()
                .Distinct()
                .OrderBy(term => term);
#endif
            return null;
        }
    }


    [SerializeField] private Localize txtLocalize;
    [SerializeField] private List<TermData> terms;

    [SerializeField] private List<UIMultiEventLoseBase> multiEventLoses = new();
    private List<UIMultiEventLoseBase> _cacheEvents = new();
    public List<UIMultiEventLoseBase> CacheEvents => _cacheEvents;

    private const int maxEvent = 2;

    public void Setup()
    {
        foreach (var ev in multiEventLoses)
        {
            ev.gameObject.SetActive(false);

            if (_cacheEvents.Count >= maxEvent) continue;

            if (ev.Setup())
            {
                ev.gameObject.SetActive(true);
                _cacheEvents.Add(ev);
            }
        }

        string finalTerm = GetFinalTerm();
        if (finalTerm != null)
        {
            txtLocalize.SetTerm(finalTerm);
            SetLocalizationParams();
        }
    }

    private string GetFinalTerm()
    {
        if (CacheEvents.Count <= 1) return null;

        var verb_1 = CacheEvents[0].GetVerbType();
        var verb_2 = CacheEvents[1].GetVerbType();

        switch (verb_1)
        {
            case EVerbType.Failed:
                return GetTerm(verb_2 == EVerbType.Failed ? EFormat.Failed_Failed : EFormat.Failed_Lose);
            case EVerbType.Lose:
                return GetTerm(verb_2 == EVerbType.Failed ? EFormat.Lose_Failed : EFormat.Lose_Lose);
            default:
                return GetTerm(EFormat.Failed_Lose);
        }
    }

    private void SetLocalizationParams()
    {
        var localizationParams = txtLocalize.GetComponent<LocalizationParamsManager>();

        localizationParams.SetParameterValue("X", CacheEvents[0].GetName());
        localizationParams.SetParameterValue("Y", CacheEvents[1].GetName());
    }

    private string GetTerm(EFormat format)
    {
        return terms.Find(x => x.format == format).term;
    }

    public bool IsAvailable()
    {
        return CacheEvents.Count > 1;
    }
}
