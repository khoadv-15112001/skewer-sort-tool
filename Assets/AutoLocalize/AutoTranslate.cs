using TMPro;
using UnityEngine;
using I2.Loc;
using Sirenix.OdinInspector;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System;
using Sirenix.Utilities;
using System.Collections.Generic;

public class AutoTranslate : MonoBehaviour
{
    [TabGroup("Component")]
    public TMP_Text textToTranslate;
    [TabGroup("Config")]
    public List<AutoLanguage> autoLanguages = new(){
        new("English", "English"),
        new("Korean", "Korean"),
        new("Japanese", "Japanese"),
        new("German", "German"),
        new("Spanish", "Spanish"),
        new("Portuguese (Brazil)", "Portuguese"),
        new ("Vietnamese", "Vietnamese"),
        new("Russian", "Russian"),
        new("Indonesian", "Indonesian"),
        new("Thai", "Thai"),
        new("French", "French"),
        //new("$Arabic", "Arabic"),
        new("Hindi", "Hindi"),
        new("Turkish", "Turkish"),
    };

    [TabGroup("Config")]
    public string startHeader = "";
    [TabGroup("Config")]
    public string defaultFont = "TextMeshPro Material None Effect";

    public bool destroyAfterTranslate = true;
    public string Additional_Promt = "";



    private void Reset()
    {
        textToTranslate = GetComponent<TMP_Text>();
    }

    [Button("Translate")]
    public void TranslateSingleText()
    {
        ApplyAutoTranslation().Forget();
    }

    public async UniTask ApplyAutoTranslation()
    {
        if (!IsTextValid()) return;

        Localize localize = GetI2Localize();

        bool success = await SetupTermAfterInitAsync(localize, textToTranslate.text);

        if (destroyAfterTranslate && success)
        {
            DestroyImmediate(this);
        }
    }

    /// <summary>
    /// Check if the text is valid
    /// </summary>
    /// <returns></returns>
    private bool IsTextValid()
    {
        if (textToTranslate == null)
        {
            textToTranslate = GetComponent<TMP_Text>();
            if (textToTranslate == null)
            {
                Debug.LogError("No TextMeshPro component found!");
                return false;
            }
        }

        string originalText = textToTranslate.text;
        if (string.IsNullOrEmpty(originalText))
        {
            Debug.LogWarning("Text is empty, cannot translate!");
            return false;
        }
        return true;
    }

    /// <summary>
    /// Get or add I2 Localize component
    /// </summary>
    /// <returns></returns>
    private Localize GetI2Localize()
    {
        Localize localize = GetComponent<Localize>();

        if (localize == null)
        {
            localize = gameObject.AddComponent<Localize>();
        }
        return localize;
    }

    /// <summary>
    /// Let the localize component initialize itself on the next frame
    /// This will automatically detect and setup the TMP component
    /// </summary>
    /// <param name="localize"></param>
    /// <param name="originalText"></param>
    /// <returns></returns>
    private async UniTask<bool> SetupTermAfterInitAsync(Localize localize, string originalText)
    {
        // Wait one frame to allow the Localize component to initialize
        await UniTask.Yield();

        // Check if term exists in any source
        string termName = await FindOrCreateTermAsync(originalText);

        if (termName.IsNullOrWhitespace()) return false;

        // Apply the term to the Localize component
        localize.Term = termName;
        localize.SecondaryTerm = defaultFont;
        Debug.Log($"Successfully set up localization for '{originalText}' with term: {termName}");
        return true;
    }

    private async UniTask<string> FindOrCreateTermAsync(string text)
    {
        // Ensure LocalizationManager is initialized
        LocalizationManager.InitializeIfNeeded();

        // Try to find if this text already has a term
        TermData termData = null;
        LanguageSourceData sourceData = LocalizationManager.Sources.Count > 0 ? LocalizationManager.Sources[0] : null;

        if (sourceData != null)
        {
            // Search for existing term by checking English translations
            foreach (TermData term in sourceData.mTerms)
            {
                int englishIndex = sourceData.GetLanguageIndex("English");
                if (englishIndex >= 0 && englishIndex < term.Languages.Length &&
                    term.Languages[englishIndex] == text)
                {
                    termData = term;
                    Debug.Log("Term already exists in the source data");
                    return termData.Term;
                }
            }
        }
        else
        {
            Debug.LogError("No source data found!");
            return null;
        }

        // Otherwise create a new term
        // Generate a term name - often this would be a category + key pattern
        string baseName = startHeader + FormatToStringText(text);
        string termName = GenerateUniqueTermName(baseName, sourceData);

        bool success = await TranslateAndAddToSheet(text, termName, sourceData);

        if (success)
        {
            return termName;
        }
        else
        {
            Debug.LogError("Failed to translate and add to sheet");
            return null;
        }
    }
    /// <summary>
    /// Translate one by one language then add to google sheet
    /// </summary>
    /// <param name="text"></param>
    /// <param name="termName"></param>
    /// <param name="sourceData"></param>
    /// <returns></returns>
    private async UniTask TranslateAndAddToSheetOld(string text, string termName, LanguageSourceData sourceData)
    {
        SetTranslation(termName, text, sourceData);
        GoogleSheetController.UploadTerm("English", termName, text).Forget();

        // German translation
        string germanText = await TranslateFromEnglish(text, "German", Additional_Promt);
        if (SetTranslation(termName, germanText, sourceData, "German"))
        {
            GoogleSheetController.UploadTerm("German", termName, germanText).Forget();
        }

        // Japanese translation
        string japaneseText = await TranslateFromEnglish(text, "Japanese", Additional_Promt);
        if (SetTranslation(termName, japaneseText, sourceData, "Japanese"))
        {
            GoogleSheetController.UploadTerm("Japanese", termName, japaneseText).Forget();
        }

        // Korean translation
        string koreanText = await TranslateFromEnglish(text, "Korean", Additional_Promt);
        if (SetTranslation(termName, koreanText, sourceData, "Korean"))
        {
            GoogleSheetController.UploadTerm("Korean", termName, koreanText).Forget();
        }

        // Chinese (Taiwan) translation
        string chineseText = await TranslateFromEnglish(text, "Taiwan", Additional_Promt);
        if (SetTranslation(termName, chineseText, sourceData, "Twi"))
        {
            GoogleSheetController.UploadTerm("Taiwan", termName, chineseText).Forget();
        }

        // Spanish translation
        string spanishText = await TranslateFromEnglish(text, "Spanish", Additional_Promt);
        if (SetTranslation(termName, spanishText, sourceData, "Spanish"))
        {
            GoogleSheetController.UploadTerm("Spanish", termName, spanishText).Forget();
        }

        // Portuguese translation
        string portugueseText = await TranslateFromEnglish(text, "Portuguese", Additional_Promt);
        if (SetTranslation(termName, portugueseText, sourceData, "Portuguese"))
        {
            GoogleSheetController.UploadTerm("Portuguese", termName, portugueseText).Forget();
        }

        // Russian translation
        string russianText = await TranslateFromEnglish(text, "Russian", Additional_Promt);
        if (SetTranslation(termName, russianText, sourceData, "Russian"))
        {
            GoogleSheetController.UploadTerm("Russian", termName, russianText).Forget();
        }
    }

    /// <summary>
    /// Translate to multiple languages in one promt then add to google sheet
    /// </summary>
    /// <param name="text"></param>
    /// <param name="termName"></param>
    /// <param name="sourceData"></param>
    /// <returns></returns>
    private async UniTask<bool> TranslateAndAddToSheet(string text, string termName, LanguageSourceData sourceData)
    {
        List<ResultLanguage> resultLanguages = await TranslateToMultipleLanguages(text, autoLanguages, Additional_Promt);

        if (resultLanguages.Count == 0)
        {
            Debug.LogError("No translations were returned from the translation service.");
            return false;
        }

        // Track successful translations
        int successfulTranslations = 0;
        int totalLanguages = autoLanguages.Count;

        // First, add the term to the source data so we can set translations
        sourceData.AddTerm(termName);

        foreach (AutoLanguage autoLanguage in autoLanguages)
        {
            string translation = autoLanguage.language == "English" ? text : resultLanguages.Find(r => r.language.ToLower().Equals(autoLanguage.language.ToLower()))?.translation;

            if (translation.IsNullOrWhitespace())
            {
                Debug.LogError($"No translation found for {autoLanguage.language} in the result. Skip this language.");
                continue;
            }

            // Attempt to set the translation
            bool translationSet = SetTranslation(termName, translation, sourceData, autoLanguage.term);

            if (translationSet)
            {
                // Only upload to Google Sheets if the translation was successfully set
                GoogleSheetController.UploadTerm(autoLanguage.language, termName, translation).Forget();
                successfulTranslations++;
                Debug.Log($"Successfully set translation for {autoLanguage.language}: {translation}");
            }
            else
            {
                Debug.LogError($"Failed to set translation for {autoLanguage.language} in I2 Localization");
            }
        }

        // Check if all translations were successful
        bool allTranslationsSuccessful = successfulTranslations == totalLanguages;

        if (!allTranslationsSuccessful)
        {
            Debug.LogError($"Translation incomplete: {successfulTranslations}/{totalLanguages} languages successfully translated for term '{termName}'");
            // Remove the term if not all translations were successful
            sourceData.RemoveTerm(termName);
        }
        else
        {
            Debug.Log($"All translations successful for term '{termName}' ({successfulTranslations}/{totalLanguages})");
        }

        return allTranslationsSuccessful;
    }

    /// <summary>
    /// Format the text to alphanumeric string, limit to 30 characters
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private string FormatToStringText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "";

        // Trim whitespace from beginning and end
        text = text.Trim();

        // Replace all spaces with underscores
        text = text.Replace(" ", "_");

        // Remove any characters that are not letters, numbers, or underscores
        string result = "";
        foreach (char c in text)
        {
            if (char.IsLetterOrDigit(c) || c == '_')
            {
                result += c;
            }
        }

        // Limit to maximum 30 characters
        if (result.Length > 24)
        {
            result = result.Substring(0, 24);
        }

        result = result.ToLower();

        return result;
    }

    private async UniTask<string> TranslateFromEnglish(string engText, string language, string additionalPrompt = "")
    {
        //Use Gemini API to translate from English to the target language
        string result = await GeminiRequest.Translate(engText, language, additionalPrompt);
        return result;
    }

    private async UniTask<List<ResultLanguage>> TranslateToMultipleLanguages(string engText, List<AutoLanguage> languages, string additionalPrompt = "")
    {
        //Use Gemini API to translate from English to the target languages
        List<ResultLanguage> result = await GeminiRequest.Translate(engText, languages, additionalPrompt);
        return result;
    }

    private bool SetTranslation(string termName, string text, LanguageSourceData sourceData, string language = "English")
    {
        int languageIndex = sourceData.GetLanguageIndex(language);
        if (languageIndex >= 0)
        {
            TermData newTerm = sourceData.GetTermData(termName);
            if (newTerm != null && languageIndex < newTerm.Languages.Length)
            {
                newTerm.Languages[languageIndex] = text;

                // Save the changes to the source
                sourceData.Editor_SetDirty();
                return true;
            }
            else
            {
                Debug.LogError($"Failed to get term data for '{termName}' or invalid language index {languageIndex}");
                return false;
            }
        }
        else
        {
            Debug.LogError($"Language '{language}' not found in source data");
            return false;
        }
    }

    // [Button("Process All TMP Text in Children")]
    public async Task ProcessAllTMPTextInChildren()
    {
        TMP_Text[] allTexts = GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text text in allTexts)
        {
            if (text.GetComponent<Localize>() == null)
            {
                // Set current text component and apply auto translation
                textToTranslate = text;
                await ApplyAutoTranslation();
            }
        }

        Debug.Log($"Processed {allTexts.Length} TMP_Text components");
    }

    /// <summary>
    /// Generate a unique term name by checking if it already exists and adding a number suffix if needed
    /// </summary>
    /// <param name="baseName">Base term name</param>
    /// <param name="sourceData">Language source data to check against</param>
    /// <returns>Unique term name</returns>
    private string GenerateUniqueTermName(string baseName, LanguageSourceData sourceData)
    {
        string termName = baseName;
        int counter = 1;

        // Keep checking and incrementing counter until we find a unique name
        while (sourceData.ContainsTerm(termName))
        {
            termName = $"{baseName}_{counter}";
            counter++;

            // Safety check to prevent infinite loop (shouldn't happen but just in case)
            if (counter > 100)
            {
                Debug.LogWarning($"GenerateUniqueTermName: Too many iterations for term '{baseName}'. Using timestamp.");
                termName = $"{baseName}_{DateTime.Now.Ticks}";
                break;
            }
        }

        Debug.Log($"Generated unique term name: '{termName}'" + (counter > 1 ? $" (original '{baseName}' already existed)" : ""));
        return termName;
    }
}

[Serializable]
public class AutoLanguage
{
    public string term;
    public string language;

    public AutoLanguage()
    {

    }

    public AutoLanguage(string term, string language)
    {
        this.term = term;
        this.language = language;
    }
}
