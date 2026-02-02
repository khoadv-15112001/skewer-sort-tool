using Sonat.Enums;
using SonatFramework.Systems;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;

namespace GrillSort.EndlessTreasure
{
    public class UIItem : MonoBehaviour
    {
        [SerializeField, ReadOnly] private int packIdx;
        [SerializeField] private UIPack uiPack;
        [SerializeField] private UIButtonLockItem uiButtonLockItem;
        [SerializeField] private Button btnClaimed;
        [SerializeField] private GameObject bgCurrentPack;
        private readonly Service<EndlessTreasureService> endlessTreasureService = new();
        public int PackIdx => packIdx; private ShopItemKey key;

        public void SetData(int packIdx)
        {
            this.packIdx = packIdx;
            this.key = endlessTreasureService.Instance.GetPackKey(packIdx);
            //Debug.Log("key: " + key);
            uiPack.SetData(packIdx, key);

            uiButtonLockItem.SetData(key);
            uiButtonLockItem.isCooldown = packIdx == endlessTreasureService.Instance.data.currentPackIdx && endlessTreasureService.Instance.Cooldown > 0;
            SetInteractableText(true);
        }
        public bool IsCooldown
        {
            set => uiButtonLockItem.isCooldown = value;
        }
        public void UpdateUI(bool force)
        {
            var currentPackIdx = endlessTreasureService.Instance.data.currentPackIdx;
            if (currentPackIdx == packIdx)
            {
                if (force)
                {
                    if (endlessTreasureService.Instance.Cooldown <= 0)
                        SetUnlock();
                }
            }
            else
            {
                btnClaimed.gameObject.SetActive(false);
                uiButtonLockItem.Show();
                uiPack.HideButtons();
                bgCurrentPack.SetActive(false);
            }
        }
        public void SetClaimed()
        {
            btnClaimed.gameObject.SetActive(true);
            uiButtonLockItem.Hide();
            uiPack.HideButtons();
        }

        public void SetUnlock()
        {
            btnClaimed.gameObject.SetActive(false);
            uiButtonLockItem.Hide();
            uiPack.SetClaim();
            bgCurrentPack.SetActive(true);
            //Debug.Log($"anhnt: SetUnlock");

        }

        public void PlayAnimationUnlock()
        {
            btnClaimed.gameObject.SetActive(false);
            uiButtonLockItem.PlayUnlockEffect();
            uiPack.SetClaim();
            bgCurrentPack.SetActive(true);
        }

        internal void ForceLock()
        {
            btnClaimed.gameObject.SetActive(false);
            uiButtonLockItem.Show();
            uiPack.HideButtons();
            bgCurrentPack.SetActive(false);
        }
        public CanvasGroup textCG;
        internal void SetInteractableText(bool value)
        {
            //Debug.Log("anhnt: SetInteractableText " + value);
            textCG.DOFade(value ? 1.0f : 0.5f, 0.3f);
        }
    }
}