using Sirenix.OdinInspector;
using SonatFramework.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

namespace GrillSort.BattlePass
{
    public class UIMileStone : MonoBehaviour
    {
        [SerializeField, ReadOnly] public int Index;

        [SerializeField] private TMP_Text textIndex;
        [SerializeField] private UIReward premium;
        [SerializeField] private UIReward free;
        [SerializeField] private GameObject lineLock;

        [SerializeField] private Image backgroundImg;
        [SerializeField] private Sprite backgroundLockSprite;
        [SerializeField] private Sprite backgroundUnlockSprite;

        [SerializeField] private Image pointImg;
        [SerializeField] private Sprite pointLockSprite;
        [SerializeField] private Sprite pointUnlockSprite;

        private bool _isUnlocked;

        public void Init(int index)
        {
            this.Index = index;
            _isUnlocked = index <= MySonatFramework.GetService<BattlePassService>().CurrentMilestoneIdx;
            textIndex.text = (Index + 1).ToString();

            premium.Init(index);
            free.Init(index);
        }

        public void UpdateUI()
        {
            premium.UpdateUI();
            free.UpdateUI();

            if (MySonatFramework.GetService<BattlePassService>().CurrentMilestoneIdx == Index - 1)
            {
                lineLock.SetActive(true);
            }
            else
            {
                lineLock.SetActive(false);
            }

            UpdateBackground();
            UpdatePoint();
        }

        private void UpdateBackground()
        {
            if (backgroundImg == null) return;

            backgroundImg.sprite = _isUnlocked ? backgroundUnlockSprite : backgroundLockSprite;
        }

        private void UpdatePoint()
        {
            if (pointImg == null) return;

            pointImg.sprite = _isUnlocked ? pointUnlockSprite : pointLockSprite;
        }
    }
}

