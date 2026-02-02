using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MyGame.Modules.CardCollection
{
    public class UICardStar : MonoBehaviour
    {
        [SerializeField] private Image imgStar;
        [SerializeField] private TMP_Text txtNum;
        [SerializeField] private AnimationCurve curveHide = AnimationCurve.Linear(0, 0, 1, 0);
        [SerializeField] private float durationHide = 0.1f;
        [SerializeField] private float delayHide = 0.1f;

        private int quantity;
        public void SetData(int num)
        {
            this.quantity = 0;
            this.quantity = num;
            txtNum.text = $"x{num}";
            txtNum.gameObject.SetActive(true);
        }

        public int GetQuantity()
        {
            return quantity;
        }

        public void Hide(Action onComplete)
        {
            imgStar.gameObject.SetActive(false);
            txtNum.transform.DOScale(0, durationHide).SetEase(curveHide).SetDelay(delayHide).OnComplete(() =>
            {
                txtNum.gameObject.SetActive(false);
                onComplete?.Invoke();
            });
        }

        internal void Resset()
        {
            quantity = 0;
            txtNum.text = "";
        }
    }
}
