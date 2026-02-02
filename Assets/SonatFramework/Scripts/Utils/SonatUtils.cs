using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SonatFramework.Scripts.Utils
{
    public static class SonatUtils
    {
        public const int SEC_IN_HOUR = 3600;

        public static void SetAlpha(this SpriteRenderer sprite, float alpha)
        {
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, alpha);
        }

        public static void SetAlpha(this Image sprite, float alpha)
        {
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, alpha);
        }

        public static void SetLocalRotateZ(this Transform transform, float z)
        {
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, z);
        }

        public static void SetPositionZ(this Transform transform, float z)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, z);
        }

        public static void SetLocalPositionX(this Transform transform, float x)
        {
            transform.localPosition = new Vector3(x, transform.localPosition.y, transform.localPosition.z);
        }

        public static void SetLocalPositionY(this Transform transform, float y)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, y, transform.localPosition.z);
        }

        public static void SetLocalPositionZ(this Transform transform, float z)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, z);
        }

        public static void SetLocalScaleX(this Transform transform, float scaleX)
        {
            transform.localScale = new Vector3(scaleX, transform.localScale.y, transform.localScale.z);
        }

        public static void SetLocalScaleY(this Transform transform, float scaleY)
        {
            transform.localScale = new Vector3(transform.localScale.x, scaleY, transform.localScale.z);
        }

        public static void SetAlpha(this Color color, float alpha)
        {
            color.a = alpha;
        }

        public static Type_e ToEnum<Type_e>(this string txt)
        {
            return (Type_e)Enum.Parse(typeof(Type_e), txt);
        }

        //   public static GameResource ToResourceEnum<Type_e>(this Type_e type)
        //{
        //	return (GameResource)Enum.Parse(typeof(GameResource), type.ToString());
        //}

        //public static T ResourceToEnum<T>(this GameResource resource)
        //{
        //	return (T)Enum.Parse(typeof(T), resource.ToString());
        //}

        public static Coroutine TxtCountTime(TMP_Text txt, long currValue, Action callback = null,
            TxtTimeFormat format = TxtTimeFormat.Smart)
        {
            txt.StopAllCoroutines();
            return txt.StartCoroutine(CountTime(txt, currValue, format, callback));
        }

        public static Coroutine TxtCountTimeGO(MonoBehaviour go, TMP_Text txt, long currValue, Action callback = null,
            TxtTimeFormat format = TxtTimeFormat.Smart)
        {
            if (go == null) return null;
            return go.StartCoroutine(CountTime(txt, currValue, format, callback));
        }

        private static IEnumerator CountTime(TMP_Text txt, long value, TxtTimeFormat format = TxtTimeFormat.Smart,
            Action callback = null)
        {
            while (value > 0)
            {
                txt.text = GetTimeByFormat(value, format);
                yield return new WaitForSecondsRealtime(1);
                value--;
            }

            callback?.Invoke();
        }

        public static string GetTimeByFormat(long value, TxtTimeFormat format = TxtTimeFormat.Smart)
        {
            switch (format)
            {
                case TxtTimeFormat.Smart:
                    return FormatTimeSmartNoUnit(value);
                case TxtTimeFormat.SmartFull:
                    return FormatTimeSmartFullNoUnit(value);
                case TxtTimeFormat.Full:
                    return FormatTimeFromSecD(value);
                case TxtTimeFormat.ShortDay_FullTime:
                    return FormatTimeShortDayFullTime(value);
                default:
                    return FormatTimeSmartNoUnit(value);
            }
        }

        private static string FormatTimeShortDayFullTime(long value)
        {
            var time = TimeSpan.FromSeconds(value);
            if (time.Days > 0) return $"{(int)time.TotalDays}d {time.Hours}h";
            return $"{(int)time.TotalHours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";
        }

        public static string FormatTimeFromSec(int sec)
        {
            var result = "";
            var m = sec / 60;
            var s = sec % 60;
            //result = m.ToString("00") + ":" + s.ToString("00");
            result = $"{m:D2}:{s:D2}";
            return result;
        }

        public static string FormatTimeSmart(long sec)
        {
            var time = TimeSpan.FromSeconds(sec);
            if (time.Days > 0) return $"{(int)time.TotalDays}D {time.Hours}H";

            if (time.Hours > 0) return $"{(int)time.TotalHours}H:{time.Minutes}M";

            return $"{(int)time.TotalMinutes}M:{time.Seconds}S";
        }

        public static string FormatTimeSmartNoUnit(long sec)
        {
            var time = TimeSpan.FromSeconds(sec);
            if (time.Days > 0) return $"{(int)time.TotalDays:D2}D {time.Hours:D2}H";

            if (time.Hours > 0) return $"{(int)time.TotalHours:D2}:{time.Minutes:D2}";

            return $"{(int)time.TotalMinutes:D2}:{time.Seconds:D2}";
        }

        public static string FormatTimeSmartFull(long sec)
        {
            var time = TimeSpan.FromSeconds(sec);
            if (time.Days > 0) return $"{(int)time.TotalDays}D {time.Hours}H:{time.Minutes}M:{time.Seconds}S";

            if (time.Hours > 0) return $"{(int)time.TotalHours}H:{time.Minutes}M:{time.Seconds}S";

            return $"{(int)time.TotalMinutes}M:{time.Seconds}S";
        }

        public static string FormatTimeSmartFullNoUnit(long sec)
        {
            var time = TimeSpan.FromSeconds(sec);
            if (time.Days > 0) return $"{(int)time.TotalDays:D2}:{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";

            if (time.Hours > 0) return $"{(int)time.TotalHours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";

            return $"{(int)time.TotalMinutes:D2}:{time.Seconds:D2}";
        }

        public static string FomatTimeFromSecH(long sec)
        {
            //System.DateTimeOffset time = System.DateTimeOffset.FromUnixTimeSeconds(sec);
            var time = TimeSpan.FromSeconds(sec);
            return $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";
        }

        public static string FomatTimeFromSecHFull(long sec)
        {
            //System.DateTimeOffset time = System.DateTimeOffset.FromUnixTimeSeconds(sec);
            var time = TimeSpan.FromSeconds(sec);
            return $"{(int)time.TotalHours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";
        }

        public static string FomatTimeFromSecM(long sec)
        {
            //System.DateTimeOffset time = System.DateTimeOffset.FromUnixTimeSeconds(sec);
            var time = TimeSpan.FromSeconds(sec);
            return $"{(int)time.TotalMinutes:D2}:{time.Seconds:D2}";
        }


        public static string FormatTimeFromSecD(long sec)
        {
            //System.DateTimeOffset time = System.DateTimeOffset.FromUnixTimeSeconds(sec);
            var time = TimeSpan.FromSeconds(sec);

            return $"{time.Days}D {time.Hours}H:{time.Minutes}M:{time.Seconds}S";
        }

        public static string FomatTimeFromSecDSort(long sec)
        {
            //System.DateTimeOffset time = System.DateTimeOffset.FromUnixTimeSeconds(sec);
            var time = TimeSpan.FromSeconds(sec);
            if (sec > SEC_IN_HOUR)
                return $"{time.Days}D {time.Hours}H";
            return $"{time.Minutes}M:{time.Seconds}S";
        }

        public static string GetReportTimeString(DateTime servertime)
        {
            //TimeSpan delta = HTTP.Instance.ServerTime - servertime;

            //if (delta.TotalSeconds < 60)
            //{
            //    return Localization.Get("ChatTimeMess1");
            //}
            //else if (delta.TotalMinutes < 60)
            //{
            //    return string.Format(Localization.Get("ChatTimeMess2"), Mathf.CeilToInt((float)delta.TotalMinutes));
            //}
            //else if (delta.TotalHours < 24)
            //{
            //    return string.Format(Localization.Get("ChatTimeMess3"), Mathf.CeilToInt((float)delta.TotalHours));
            //}
            //else if (delta.TotalDays < 4)
            //{
            //    return string.Format(Localization.Get("ChatTimeMess4"), Mathf.CeilToInt((float)delta.TotalDays));
            //}
            //else
            //{
            //    //return servertime.ToShortDateString();
            //    return servertime.ToString("dd/MM/yyyy");
            //}

            //return time.ToString("dd/MM/yyyy");
            return "";
        }

        public static string ToLogString(this string input)
        {
            return ConvertToLogParam(input);
        }


        public static string ToLogString(this GameResource input)
        {
            return input.ToString().ToLogString();
        }

        public static string ToLogString(this GameResourceType input)
        {
            return input.ToString().ToLogString();
        }

        public static string ConvertToLogParam(string input)
        {
            var result = "";

            input = Regex.Replace(input, @"\s*\([^)]*\)", "");

            for (var i = 0; i < input.Length; i++)
            {
                if (char.IsUpper(input[i]))
                    if (i > 0 && input[i - 1] != '_')
                        result += "_";

                //result += ' ';
                result += char.ToLower(input[i]);
            }

            return result;
        }

        public static void SetActiveAll<T>(this IList<T> list, bool value) where T : Component
        {
            foreach (var x1 in list)
                x1.gameObject.SetActive(value);
        }

        public static Vector3 ConvertWorldPosBetweenTwoCam(Vector3 pos, Camera cam1, Camera cam2)
        {
            var v = cam1.WorldToScreenPoint(pos);
            return cam2.ScreenToWorldPoint(v);
        }

        public static async UniTask PlayTweens(TweenData[] tweenDatas, Action callback)
        {
            if (tweenDatas == null)
            {
                callback?.Invoke();
                return;
            }

            float maxTime = 0;
            for (var i = 0; i < tweenDatas.Length; i++)
            {
                UITween.Play(tweenDatas[i]).Forget();
                if (tweenDatas[i].config.delay + tweenDatas[i].config.duration > maxTime)
                    maxTime = tweenDatas[i].config.delay + tweenDatas[i].config.duration;
            }

            await UniTask.Delay(TimeSpan.FromSeconds(maxTime));
            callback?.Invoke();
        }

        public static Vector3 WorldPointToLocalRectPoint(Vector3 worldPos, RectTransform rect, Camera worldCam = null, Camera camUI = null)
        {
            Vector3 screenPosition = worldCam.WorldToScreenPoint(worldPos);
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPosition, camUI, out localPoint);
            return localPoint;
        }

        public static void ExecuteNextFrame(Action action, int frameCount = 1)
        {
            _ExecuteNextFrame(action, frameCount).Forget();
        }

        private static async UniTaskVoid _ExecuteNextFrame(Action action, int frameCount = 1)
        {
            await UniTask.DelayFrame(frameCount);
            action?.Invoke();
        }

        public static Coroutine DelayCall(float delay, Action action, MonoBehaviour obj = null)
        {
            if (obj == null) return SonatSystem.Instance.StartCoroutine(IEDelayCall(delay, action));
            else return obj.StartCoroutine(IEDelayCall(delay, action));
        }

        private static IEnumerator IEDelayCall(float delay, Action action)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }
    }

    public enum TxtTimeFormat
    {
        Smart = 0,
        SmartFull = 1,
        Full = 3,
        ShortDay_FullTime = 4
    }
}