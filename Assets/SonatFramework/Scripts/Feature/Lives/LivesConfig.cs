using Sonat.Enums;
using SonatFramework.Systems.ConfigManagement;
using UnityEngine;

namespace SonatFramework.Scripts.Feature.Lives
{
    [CreateAssetMenu(fileName = "LivesConfig", menuName = "Sonat Configs/LivesConfig")]
    public class LivesConfig : ConfigSo
    {
        public int maxLives => MaxLives;
        public int defaultMaxLives = 5;
        [SerializeField] private int _timeRefillLives = 1800;
        public int refillFree = 1;
        public GameResource refillPriceCurrency;
        public int refillPrice;

        private const string KEY = "max_lives";
        private const string KEY_TIME_REFILL = "cheat_time_refill_lives";

        private int singlePrice;
        public int timeRefillLives => TimeRefillLives > 0 ? TimeRefillLives : _timeRefillLives;
        public int SinglePrice { get => singlePrice; set => singlePrice = value; }

        public static int MaxLives
        {
            get
            {
                return PlayerPrefs.GetInt(KEY, 5);
            }

            set
            {
                PlayerPrefs.SetInt(KEY, value);
            }
        }

        /// <summary>
        /// Cheat time refill lives (seconds). 0 = use default value
        /// </summary>
        public static int TimeRefillLives
        {
            get
            {
                return PlayerPrefs.GetInt(KEY_TIME_REFILL, 0);
            }

            set
            {
                PlayerPrefs.SetInt(KEY_TIME_REFILL, value);
            }
        }


        /// <summary>
        /// Reset cheat time refill về mặc định
        /// </summary>
        public static void ResetTimeRefillLives()
        {
            PlayerPrefs.DeleteKey(KEY_TIME_REFILL);
        }

        public void SetSinglePrice()
        {
            singlePrice = refillPrice / defaultMaxLives;
        }
    }
}