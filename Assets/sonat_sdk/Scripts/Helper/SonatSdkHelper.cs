using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Sonat.AdsModule;
using Sonat.FirebaseModule;

namespace Sonat
{
    public static class SonatSdkHelper
    {
        public static void TrySetValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }


        public static int GetInt(this RemoteConfigKey key, int defaultValue = 0)
        {
            return SonatFirebase.remote.GetRemoteInt(key.ToString(), defaultValue);
        }

        public static float GetFloat(this RemoteConfigKey key, float defaultValue = 0)
        {
            return SonatFirebase.remote.GetRemoteFloat(key.ToString(), defaultValue);
        }

        public static string GetString(this RemoteConfigKey key, string defaultValue = "")
        {
            return SonatFirebase.remote.GetRemoteString(key.ToString(), defaultValue);
        }

        public static bool GetBool(this RemoteConfigKey key, bool defaultValue = false)
        {
            return SonatFirebase.remote.GetRemoteBool(key.ToString(), defaultValue);
        }

        public static T Get<T>(this RemoteConfigKey key, T defaultValue = default(T))
        {
            return SonatFirebase.remote.GetRemoteConfig<T>(key.ToString(), defaultValue);
        }


        public static bool HasDuplicate(this IList<int> arr)
        {
            for (var i = 0; i < arr.Count; i++)
            for (int j = i + 1; j < arr.Count; j++)
                if (arr[i] == arr[j])
                    return true;

            return false;
        }

        public static void SetDirty(this UnityEngine.Object obj)
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(obj);
#endif
        }
        
        public static int GetEpochDate(DateTime date)
        {
            DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0);
            return (int) (date - epochStart).TotalDays;
        }
        
        public static int GetEpochDate()
        {
            return GetEpochDate(DateTime.Today);
        }

        public static string LowercaseFirstLetter(this string str)
        {
            return char.ToLower(str[0]) + str.Substring(1);
        }
        
        public static string RemoveWhiteSpace(this string source)
        {
            return Regex.Replace(source, @"\s+", "");
        }
        
        public static string ToTrackingName(this string input)
        {
            string result = "";

            input = Regex.Replace(input, @"\s*\([^)]*\)", "");

            for (int i = 0; i < input.Length; i++)
            {
                if (char.IsUpper(input[i]))
                {
                    if(i > 0)
                    {
                        result += "_";
                    }
                    //result += ' ';
                }

                result += char.ToLower(input[i]);
            }
            return result;
        }


        public static List<int> GetIntListFromRegex(string input)
        {
            input = input.RemoveWhiteSpace();
            List<int> result = new List<int>();
            var split = input.Split(',');
            foreach (var s in split)
            {
                if (int.TryParse(s, out int value))
                {
                    result.Add(value);
                }
                else
                {
                    var p = GetIntListFromExRangeFormat(s);
                    if (p is { Count: > 0 })
                    {
                        result.AddRange(p);
                    }
                    else
                    {
                        p = GetIntListFromRangeFormat(s);
                        if (p is { Count: > 0 })
                        {
                            result.AddRange(p);
                        }
                    }
                }
            }

            return result;
        }

        private static List<int> GetIntListFromRangeFormat(string input)
        {
            Regex pattern = new Regex(@"\[(?<from>\d+)-(?<to>\d+)\]");
            if (!pattern.IsMatch(input)) return null;
            Match match = pattern.Match(input);
            if (int.TryParse(match.Groups["from"].Value, out int from) && int.TryParse(match.Groups["to"].Value, out int to) && to > from)
            {
                return Enumerable.Range(from, to - from + 1).ToList();
            }

            return null;
        }

        private static List<int> GetIntListFromExRangeFormat(string input)
        {
            Regex pattern = new Regex(@"\[(?<from>\d+)-(?<to>\d+)%(?<step>\d+)\]");
            if (!pattern.IsMatch(input)) return null;
            Match match = pattern.Match(input);
            if (int.TryParse(match.Groups["from"].Value, out int from) && int.TryParse(match.Groups["to"].Value, out int to) &&
                int.TryParse(match.Groups["step"].Value, out int step) && to > from && step > 0)
            {
                List<int> result = new List<int>();
                for (int i = from; i <= to; i++)
                {
                    if (i % step == from % step)
                    {
                        result.Add(i);
                    }
                }

                return result;
            }

            return null;
        }

        public static AdsPlatform ToAdsPlatform(this MediationType mediationType)
        {
            switch (mediationType)
            {
                case MediationType.Admob:
                    return AdsPlatform.googleadmob;
                case MediationType.Max:
                    return AdsPlatform.applovinmax;
                case MediationType.IronSource:
                    return AdsPlatform.ironsource;
            }

            return AdsPlatform.googleadmob;
        }

        public static AdTypeLog ToAdTypeLog(this AdType adType)
        {
            switch (adType)
            {
                case AdType.Banner:
                    return AdTypeLog.banner;
                case AdType.Interstitial:
                    return AdTypeLog.interstitial;
                case AdType.Rewarded:
                    return AdTypeLog.rewarded;
                case AdType.CollapsibleBanner:
                    return AdTypeLog.collapsible_banner;
                case AdType.LargeBanner:
                    return AdTypeLog.native_banner;
                case AdType.NativeAds:
                    return AdTypeLog.native;
                case AdType.AppOpenAd:
                    return AdTypeLog.app_open;
                case AdType.MREC:
                    return AdTypeLog.mrec;
            }

            return AdTypeLog.undefined;
        }
    }
}