using System.Collections;
using System.Globalization;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using I2.Loc;
using Sonat.Enums;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.ObjectPooling;
using TMPro;

namespace SonatFramework.Scripts.Helper
{
    public static class GameHelperCommon
    {
        // public static GameResourceType ResourceType(this GameResource resource)
        // {
        //     switch (resource)
        //     {
        //         case GameResource.Coin:
        //         case GameResource.Lives:
        //             return GameResourceType.Currency;
        //     }
        //
        //     return GameResourceType.None;
        // }

        public static void ReturnObject(this IPoolingObject poolingObject)
        {
            SonatSystem.GetService<SonatPoolingService>().ReturnObj(poolingObject);
        }

        public static TweenerCore<int, int, NoOptions> DOCounterFormat(
            this TMP_Text target, int fromValue, int endValue, string format, float duration)
        {
            int v = fromValue;
            TweenerCore<int, int, NoOptions> t = DOTween.To(() => v, x =>
            {
                v = x;
                target.text = string.Format(format, v);
            }, endValue, duration);
            t.SetTarget(target);
            return t;
        }

        public static void SetLocalize(this TMP_Text target, string term)
        {
            if (target.TryGetComponent<Localize>(out var localize) && LocalizationManager.GetTermData(term) != null)
            {
                localize.SetTerm(term);
            }
            else
            {
                target.text = term;
            }
        }
        
        public static void SetLocalizeParam(this TMP_Text target, string param, string value)
        {
            if (target.TryGetComponent<LocalizationParamsManager>(out var localize))
            {
                localize.SetParameterValue(param, value);
            }
            else
            {
                target.text = param.Replace("{[VALUE]}", value);
            }
        }
    }
}