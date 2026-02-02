using System;
using System.Collections.Generic;
using UnityEngine;

public enum ComboTextPrefab
{
    ComboText_Wow,          // Combo 2
    ComboText_Good,         // Combo 4
    ComboText_Tasty,        // Combo 6
    ComboText_Awesome,      // Combo 8
    ComboText_Excellent,    // Combo 10
    ComboText_Wonderful,    // Combo 12
    ComboText_Perfect,      // Combo 14
    ComboText_Amazing,      // Combo 16
    ComboText_Unbelievable, // Combo 18
    ComboText_SmokinHot     // Combo 20
}

[Serializable]
public class ComboTextData
{
    public int comboLevel;
    [Tooltip("Prefab for this combo level")]
    public ComboTextPrefab prefab;
}

[CreateAssetMenu(fileName = "ComboConfig", menuName = "Sonat Services/Combo Config")]
public class ComboConfig : ScriptableObject
{
    [SerializeField] private List<int> comboTime;
    [SerializeField] private List<ComboTextData> comboTexts;
    
    [Tooltip("Enable combo voice sounds")]
    [SerializeField] private bool enableComboVoice = true;

    public List<int> ComboTime => comboTime;
    public bool EnableComboVoice => enableComboVoice;
    
    // Track alternating voice for Alternate mode
    private bool useFemaleLast = false;

    /// <summary>
    /// Get the prefab name for a specific combo level
    /// </summary>
    /// <param name="comboLevel">The current combo level</param>
    /// <returns>The prefab name to spawn, or empty string if no match</returns>
    public string GetComboPrefabName(int comboLevel)
    {
        foreach (var comboText in comboTexts)
        {
            if (comboText.comboLevel == comboLevel)
            {
                return comboText.prefab.ToString();
            }
        }
        return string.Empty;
    }

    /// <summary>
    /// Check if there is a combo text for this level
    /// </summary>
    public bool HasComboText(int comboLevel)
    {
        foreach (var comboText in comboTexts)
        {
            if (comboText.comboLevel == comboLevel)
            {
                return true;
            }
        }
        return false;
    }
}

