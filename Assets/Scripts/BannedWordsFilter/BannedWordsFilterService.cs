using SonatFramework.Systems;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

[CreateAssetMenu(fileName = "BannedWordsFilterService", menuName = "Sonat Services Custom/Banned Words Filter Service")]
public class BannedWordsFilterService : SonatServiceSo, IServiceInitialize
{
    [SerializeField] private BannedWordsFilterConfig config;

    private static TextAsset profanityListAsset;

    private static HashSet<string> bannedWords;

    private static readonly Regex _wordRegex = new Regex(@"\b([A-Za-z]+)\b", RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public void Initialize()
    {
        profanityListAsset = config.profanityListAsset;

        LoadProfanityFromTextAsset();
    }

    private void LoadProfanityFromTextAsset()
    {
        if (bannedWords != null) return; // đã có cache dùng lại

        bannedWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (profanityListAsset == null || string.IsNullOrEmpty(profanityListAsset.text)) return;

        var lines = profanityListAsset.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var word = line.Trim();
            if (word.Length == 0) continue;
            //if (word.StartsWith("#")) continue; // cho phép comment
            bool ok = true;
            for (int i = 0; i < word.Length; i++)
            {
                char c = word[i];
                if (!((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))) { ok = false; break; }
            }
            if (ok) bannedWords.Add(word);
        }
    }

    public string ApplyProfanityFilter(string text)
    {
        return Regex.Replace(
            text,
            @"\b([A-Za-z]+)\b",
            m => bannedWords.Contains(m.Groups[1].Value) ? "*" : m.Groups[1].Value,
            RegexOptions.CultureInvariant
        );
    }

    public bool ContainsBannedWords(string text)
    {
        if (string.IsNullOrEmpty(text)) return false;
        if (bannedWords == null || bannedWords.Count == 0) return false;

        foreach (Match m in _wordRegex.Matches(text))
        {
            var word = m.Groups[1].Value;
            if (bannedWords.Contains(word))
                return true;
        }
        return false;
    }
}
