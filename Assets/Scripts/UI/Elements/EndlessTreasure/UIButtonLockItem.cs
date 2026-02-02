using DG.Tweening;
using GrillSort.Winstreak;
using Sonat.Enums;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using SonatFramework.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.EndlessTreasure
{
    public class UIButtonLockItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text[] txtsCost;
        [SerializeField] private TMP_Text txtFree;
        [SerializeField] private Image imgLock;
        private readonly Service<EndlessTreasureService> endlessTreasureService = new Service<EndlessTreasureService>();

        internal bool isCooldown = false;
        public void SetData(ShopItemKey key)
        {
            if (key == ShopItemKey.None)
            {
                foreach (var txt in txtsCost)
                {
                    txt.gameObject.SetActive(false);
                }
                txtFree.gameObject.SetActive(true);
            }
            else
            {
                foreach (var txt in txtsCost)
                {
                    txt.gameObject.SetActive(true);
                    txt.text = $"{SonatSDKAdapter.GetProductPrice(key)}";
                }
                txtFree.gameObject.SetActive(false);
            }
        }

        public void OnClick()
        {
            if (isCooldown)
            {
                var cooldown = endlessTreasureService.Instance.GetEndCooldown();
                PopupToast.Cretate("Comeback after", $" {cooldown}");
            }
            else
            {
                PopupToast.Cretate("Unlock the previous offer first!");
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            imgLock.transform.localScale = Vector3.one;
            gameObject.SetActive(true);
        }

        public void PlayUnlockEffect()
        {
            isCooldown = false;

            imgLock.transform.localScale = Vector3.one; // Reset scale ban đầu

            // Tạo chuỗi tween
            Sequence seq = DOTween.Sequence();

            // 1. Rung lắc (rotation)
            seq.Append(imgLock.transform.DOShakeRotation(0.75f, new Vector3(0, 0, 20), 30, 90, false))
                    // Scale phóng to rồi thu nhỏ đồng thời
                    .Join(imgLock.transform.DOScale(1.4f, 1f).SetEase(Ease.OutQuad));

            seq.Append(imgLock.transform.DOLocalMoveY(imgLock.transform.localPosition.y + 60f, 0.4f).SetEase(Ease.OutQuad));

            seq.Append(
                 DOTween.Sequence()
                    // Hạ xuống
                    .Append(imgLock.transform.DOLocalMoveY(imgLock.transform.localPosition.y, 0.3f).SetEase(Ease.InQuad))
                    .Join(imgLock.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack))
            );

            seq.Play().SetDelay(0.25f);
            seq.OnComplete(() =>
            {
                Hide();
            });
        }
    }
}
