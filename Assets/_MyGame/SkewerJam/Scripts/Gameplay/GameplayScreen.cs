using System;
using Cysharp.Threading.Tasks;
using MyGame.SkewerJam.Gameplay.Helpers;
using SonatFramework.Scripts.UIModule.UIElements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MyGame.SkewerJam.Gameplay
{
    public class GameplayScreen : MonoBehaviour
    {
        [SerializeField] private TMP_Text txtLevel;
        [SerializeField] private UICurrency[] currencies;

        // [Header("Cheat")]
        // [SerializeField] private TMP_InputField inputLevel;
        // [SerializeField] private TMP_InputField inputStepGap;

        // [SerializeField] private Toggle toggleForward;

        void Start()
        {
            // toggleForward.isOn = OrderHelper.forward;
            // toggleForward.onValueChanged.AddListener(OnClickForward);
        }

        public void InitLevel(int level)
        {
            txtLevel.text = $"Level {level}";
            foreach (var currency in currencies)
            {
                currency.gameObject.SetActive(true);
                currency.UpdateValueView(false);
            }
        }

        public void HideCurrencies()
        {
            foreach (var currency in currencies)
            {
                currency.gameObject.SetActive(false);
            }
        }

        // public void OnClickReplay()
        // {
        //     GameController.Instance.Replay();
        // }

        // public void OnClickNextLevel()
        // {
        //     if (int.TryParse(inputLevel.text, out int level))
        //     {
        //         GameController.Instance.PlayLevel(level, true).Forget();
        //     }
        // }

        // public void OnClickStepGap()
        // {
        //     if (int.TryParse(inputStepGap.text, out int stepGap))
        //     {
        //         OrderHelper.maxStep2Gap = stepGap;
        //     }
        // }

        // public void OnClickForward(bool isOn)
        // {
        //     OrderHelper.forward = isOn;
        // }
    }
}
