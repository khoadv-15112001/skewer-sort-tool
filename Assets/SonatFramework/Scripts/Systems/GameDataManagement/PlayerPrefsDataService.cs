using Newtonsoft.Json;
using UnityEngine;

namespace SonatFramework.Systems.GameDataManagement
{
    [CreateAssetMenu(fileName = "PlayerPrefsDataService",
        menuName = "Sonat Services/Data Service/PlayerPrefsDataService", order = 0)]
    public class PlayerPrefsDataService : DataService
    {
        public override bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        public override void DeleteKey(string key)
        {
            PlayerPrefs.DeleteKey(key);
        }

        public override void SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }

        public override int GetInt(string key, int defaultValue = 0)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }

        public override void SetFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
        }

        public override float GetFloat(string key, float defaultValue = 0)
        {
            return PlayerPrefs.GetFloat(key, defaultValue);
        }

        public override void SetString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }

        public override string GetString(string key, string defaultValue = null)
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }

        public override bool GetBool(string key, bool defaultValue = false)
        {
            var value = PlayerPrefs.GetInt(key, defaultValue ? 1 : 0);
            return value != 0;
        }

        public override void SetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
        }

        public override T GetData<T>(string key)
        {
            var json = PlayerPrefs.GetString(key);
            if (string.IsNullOrEmpty(json)) return default;
            return JsonConvert.DeserializeObject<T>(json);
        }

        public override void SetData<T>(string key, T data)
        {
            var json = JsonConvert.SerializeObject(data);
            PlayerPrefs.SetString(key, json);
        }
    }
}