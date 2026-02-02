using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public static class GeminiRequest
{
    public static string apiKey = "AIzaSyDEM7VZ_DjjAR7S5d241slJOmgWY7nWd1o";
    public static string link = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key=";

    public static async UniTask<string> Translate(string text, string language, string additionalPrompt = "")
    {
        string result = await TranslateSingle(text, language, additionalPrompt);
        return result;
    }

    public static async UniTask<List<ResultLanguage>> Translate(string text, List<AutoLanguage> languages, string additionalPrompt = "")
    {
        List<ResultLanguage> result = await TranslateMultiple(text, languages, additionalPrompt);
        return result;
    }

    private static async UniTask<List<ResultLanguage>> TranslateMultiple(string originText, List<AutoLanguage> languages, string additionalPrompt = "")
    {
        string endpoint = $"{link}{apiKey}";

        string additionalRequirement = "";
        if (!string.IsNullOrEmpty(additionalPrompt))
        {
            additionalRequirement = " Additional requirement: " + additionalPrompt;
        }

        // Build language list for the prompt
        string languageList = "";
        for (int i = 0; i < languages.Count; i++)
        {
            if (i > 0) languageList += ", ";
            languageList += languages[i].language;
        }

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = $"Translate the following text from English to multiple languages: {languageList}.\n\n" +
                                    $"Text to translate: \"{originText}\"\n\n" +
                                    $"Output ONLY valid JSON in this exact format:\n" +
                                    $"{{\n" +
                                    $"  \"translations\": [\n" +
                                    $"    {{\"language\": \"German\", \"translation\": \"your translation here\"}},\n" +
                                    $"    {{\"language\": \"Japanese\", \"translation\": \"your translation here\"}}\n" +
                                    $"  ]\n" +
                                    $"}}" +
                                    additionalRequirement }
                    }
                }
            }
        };

        var json = JsonConvert.SerializeObject(requestBody);

        var client = new HttpClient();
        var response = await client.PostAsync(endpoint, new StringContent(json, Encoding.UTF8, "application/json"));

        string result = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            JObject jsonObj = JObject.Parse(result);
            string output = jsonObj["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

            Debug.Log("Gemini says:");
            Debug.Log(output);

            // Try to extract the translations from the JSON response
            try
            {
                // Remove any potential markdown code blocks that might surround the JSON
                Debug.LogWarning("Gemini Output: " + output);
                output = output.Replace("```json", "").Replace("```", "").Trim();

                // Parse the JSON response

                // Check if the output is valid JSON
                JObject translationObj = JObject.Parse(output);
                JArray translationsArray = translationObj["translations"] as JArray;

                List<ResultLanguage> results = new List<ResultLanguage>();

                if (translationsArray != null)
                {
                    foreach (JObject translation in translationsArray)
                    {
                        string language = translation["language"]?.ToString();
                        string translationText = translation["translation"]?.ToString();

                        if (!string.IsNullOrEmpty(language) && !string.IsNullOrEmpty(translationText))
                        {
                            results.Add(new ResultLanguage
                            {
                                language = language,
                                translation = translationText
                            });
                        }
                    }
                }

                return results;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to parse JSON response: {e.Message}. Returning empty list.");
                return new List<ResultLanguage>();
            }
        }
        else
        {
            Debug.Log("Error:");
            Debug.Log(result);
            return new List<ResultLanguage>();
        }
    }

    private static async UniTask<string> TranslateSingle(string originText, string language, string additionalPrompt = "")
    {
        string endpoint = $"{link}{apiKey}";

        string additionalRequirement = "";
        if (!string.IsNullOrEmpty(additionalPrompt))
        {
            additionalRequirement = ".Additional requirement: " + additionalPrompt;
        }

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = $"Translate the following text from English to {language}."
                        +" Output ONLY valid JSON in this exact format: {{\"translation\": \"your translation here\"}}. "
                        + additionalRequirement }
                    }
                }
            }
        };

        var json = JsonConvert.SerializeObject(requestBody);
        Debug.Log("Request JSON: " + json);

        var client = new HttpClient();
        var response = await client.PostAsync(endpoint, new StringContent(json, Encoding.UTF8, "application/json"));

        string result = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            JObject jsonObj = JObject.Parse(result);
            string output = jsonObj["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

            Debug.Log("Gemini says:");
            Debug.Log(output);

            // Try to extract the translation from the JSON response
            try
            {
                // Remove any potential markdown code blocks that might surround the JSON
                Debug.LogWarning("Gemini Output: " + output);
                output = output.Replace("```json", "").Replace("```", "").Trim();

                // Parse the JSON response
                JObject translationObj = JObject.Parse(output);
                return translationObj["translation"]?.ToString() ?? output;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to parse JSON response: {e.Message}. Returning raw output.");
                return output;
            }
        }
        else
        {
            Debug.Log("Error:");
            Debug.Log(result);
            return "";
        }
    }
}

public class ResultLanguage
{
    public string language;
    public string translation;
}
