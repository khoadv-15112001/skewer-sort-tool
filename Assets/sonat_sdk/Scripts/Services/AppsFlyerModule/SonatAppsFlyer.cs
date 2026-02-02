using System;
using System.Collections.Generic;
using System.Text;
#if using_appsflyer
using AppsFlyerSDK;
#endif
using UnityEngine;
#if using_appsflyer
#endif

namespace Sonat.AppsFlyerModule
{
    [CreateAssetMenu(menuName = "SonatSDK/Services/AppsFlyer Service", fileName = nameof(SonatAppsFlyer))]
    public class SonatAppsFlyer : SonatService
    {
        private static SonatAppsFlyer instance;

        public class Util
        {
            private static readonly string AUTO_ID_ALPHABET =
                "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            private static readonly int AUTO_ID_LENGTH = 24;
            private static readonly System.Random rand = new System.Random();

            public static string generateSonatARMUserID()
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;
                long unixTimeMilliseconds = now.ToUnixTimeMilliseconds();

                return unixTimeMilliseconds.ToString() + "-" + _AutoId();
            }

            private static string _AutoId()
            {
                StringBuilder builder = new StringBuilder();
                int maxRandom = AUTO_ID_ALPHABET.Length;
                for (int i = 0; i < AUTO_ID_LENGTH; i++)
                {
                    builder.Append(AUTO_ID_ALPHABET[rand.Next(maxRandom)]);
                }

                return builder.ToString();
            }
        }

        public override SonatServiceType ServiceType => SonatServiceType.AppsFlyerService;
        public override bool Ready { get; set; }

        public string devKey = "Ry3zd9x7iEihJdcAdRb7PX";
        public string UWPAppID;

        public string PlatformID =>
#if UNITY_WSA_10_0 && !UNITY_EDITOR
            UWPAppID;
#endif
#if UNITY_IOS
            SonatSdkManager.Settings.appID_IOS;
#endif
#if UNITY_ANDROID
            //SonatSdkManager.Settings.appID_Android;
            "";
#endif
#if UNITY_STANDALONE
            "";
#endif
#if UNITY_WEBGL
            "";
#endif

        public bool isDebug;
        //public bool splitCampaignName;

        public bool getConversionData => true;
        //******************************//

        public static string CustomerUserId
        {
            get
            {
                if (!PlayerPrefs.HasKey(nameof(CustomerUserId)))
                    PlayerPrefs.SetString(nameof(CustomerUserId), Util.generateSonatARMUserID());
                return PlayerPrefs.GetString(nameof(CustomerUserId));
            }
            set => PlayerPrefs.SetString(nameof(CustomerUserId), value);
        }


        public override void Initialize(Action<ISonatService> onInitialized)
        {
            instance = this;
            base.Initialize(onInitialized);
#if using_appsflyer
            AppsFlyerHandle appsFlyerHandle = SonatSdkManager.instance.gameObject.AddComponent<AppsFlyerHandle>();
            appsFlyerHandle.Initialize(this);
#endif
        }

        public void OnInitializeCompleted()
        {
            Ready = true;
            OnInitialized?.Invoke(this);
        }

        public static string GetUserId()
        {
#if using_appsflyer
            return AppsFlyer.getAppsFlyerId();
#endif
            return "";
        }


        public static bool ConversionLogged
        {
            get => PlayerPrefs.GetInt(nameof(ConversionLogged)) == 1;
            set => PlayerPrefs.SetInt(nameof(ConversionLogged), value ? 1 : 0);
        }

        public static bool Firebase
        {
            get => PlayerPrefs.GetInt(nameof(ConversionLogged)) == 1;
            set => PlayerPrefs.SetInt(nameof(ConversionLogged), value ? 1 : 0);
        }


        public static void ReloadGdprConsent()
        {
#if using_appsflyer
            AppsFlyer.stopSDK(true);
            var gdprUserConsent = AppsFlyerConsent.ForGDPRUser(true, true);
            AppsFlyer.setConsentData(gdprUserConsent);
            AppsFlyer.startSDK();
#endif
        }


        public static void SendEvent(string eventName, Dictionary<string, string> eventValues)
        {
#if using_appsflyer
            AppsFlyer.sendEvent(eventName, eventValues);
#endif
        }

        public static void SendEvent(string eventName)
        {
#if using_appsflyer
            Dictionary<string, string> eventValues = new Dictionary<string, string>();
            AppsFlyer.sendEvent(eventName, eventValues);
#endif
        }
    }


    [System.Serializable]
    public class DictStringString2
    {
        public List<string> keys = new List<string>();
        public List<string> values = new List<string>();

        public string GetString()
        {
            string str = "";
            for (var i = 0; i < keys.Count; i++)
            {
                str += keys[i] + ":" + values[i];
                if (i < keys.Count - 1)
                    str += ",";
            }

            return str;
        }

        public int Count => keys.Count;


        public virtual bool Exist(string key)
        {
            return keys.Contains(key);
        }

        public virtual string Get(string key)
        {
            for (var i = 0; i < keys.Count; i++)
            {
                if (keys[i] == key)
                    return values[i];
            }

            Debug.LogError("not found");
            return string.Empty;
        }

        public virtual void Set(string key, string value)
        {
            for (var i = 0; i < keys.Count; i++)
            {
                if (keys[i] == key)
                    values[i] = value;
            }

            keys.Add(key);
            values.Add(value);
        }

        public bool Equal(DictStringString2 other)
        {
            if (other.Count != Count)
                return false;
            for (var i = 0; i < other.keys.Count; i++)
            {
                if (!Exist(other.keys[i]))
                    return false;
                if (Get(other.keys[i]) != other.values[i])
                    return false;
            }

            return true;
        }
    }
}