using System;
using Sonat.Enums;
using UnityEngine;

namespace SonatFramework.Scripts.UIModule.UIElements
{
    [CreateAssetMenu(fileName = "FormatRewardSO", menuName = "Sonat Configs Custom/FormatRewardSO", order = 0)]
    public class FormatRewardSO : ScriptableObject
    {
        public string GetRewardText(GameResource resource, int quantity)
        {
            switch (resource)
            {
                case GameResource.None:
                    break;
                case GameResource.Lives:
                case GameResource.Coin:
                    return quantity.ToString();
                // break;
                // case GameResource.InfLive:
                //     return FormatShortTime(quantity);
                //     break;
                // case GameResource.Double_Star:
                // case GameResource.Booster_Compass:
                // case GameResource.Booster_Freeze:
                // case GameResource.Booster_Hint:
                //     return $"x{quantity}";
                case GameResource.MAX:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(resource), resource, null);
            }

            return quantity.ToString();
        }

        public static string FormatShortTime(int seconds)
        {
            if (seconds <= 0)
                return "0m";

            // How many whole hours?
            int hours = seconds / 3600;
            if (hours > 0)
                return $"{hours}h";

            // Otherwise, round up to the next full minute
            int minutes = (seconds + 59) / 60;
            return $"{minutes}m";
        }

        public static string FormatTime(long seconds)
        {
            var remainingTime = TimeSpan.FromSeconds(seconds);
            if ((int) remainingTime.TotalHours > 0)
                return $"{(int)remainingTime.TotalHours:D2}h:{remainingTime.Minutes:D2}m";
            else
                return $"{remainingTime.Minutes:D2}m:{remainingTime.Seconds:D2}s";
        }
    }
}