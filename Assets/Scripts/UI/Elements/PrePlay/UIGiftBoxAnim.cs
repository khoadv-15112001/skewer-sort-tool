using System;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.ConsecutiveWin
{
    public class UIGiftBoxAnim : MonoBehaviour
    {
        [SerializeField] private SkeletonGraphic _skeletonGraphic;
        [SerializeField] private TMP_Text txtBonusTime;

        // [SerializeField] private Image _image;
        [SerializeField] private float _durationClose = 0.5f;
        private int _animIndex;
        private readonly Service<ConsecutiveWinService> _consecutiveWinService = new();

        private void OnEnable()
        {
            _animIndex = _consecutiveWinService.Instance.GetConsecutiveWins();
            _animIndex = Mathf.Min(3, _animIndex);
            if (txtBonusTime)
                txtBonusTime.gameObject.SetActive(_animIndex > 0);

            if (_animIndex == 0)
            {
                // _image.gameObject.SetActive(false);
                _skeletonGraphic.gameObject.SetActive(true);
                _skeletonGraphic.AnimationState.ClearTracks();
                _skeletonGraphic.AnimationState.SetAnimation(0, $"Box_{1}_{State.Close_Idle}", true);
                ;
            }
            else
            {
                // _image.gameObject.SetActive(false);
                _skeletonGraphic.gameObject.SetActive(true);
                _skeletonGraphic.AnimationState.ClearTracks();
                _skeletonGraphic.AnimationState.SetAnimation(0, $"Box_{_animIndex}_{State.Open}", false).Complete += (track) =>
                {
                    _skeletonGraphic.AnimationState.SetAnimation(0, $"Box_{_animIndex}_{State.Idle}", true);
                };
                if (txtBonusTime)
                    txtBonusTime.text = $"+{_consecutiveWinService.Instance.Config.GetTimeToAdd(_animIndex)}s";
            }
        }

        public void Close(Action onComplete)
        {
            if (gameObject.activeSelf == false || _animIndex == 0)
            {
                onComplete?.Invoke();
                return;
            }

            _skeletonGraphic.AnimationState.ClearTracks();
            _skeletonGraphic.AnimationState.SetAnimation(0, $"Box_{_animIndex}_{State.Close}", false);

            SonatUtils.DelayCall(_durationClose, () => { onComplete?.Invoke(); });
        }

        private void OnDisable()
        {
        }

        public enum State
        {
            Idle,
            Open,
            Close,
            Close_Idle
        }
    }
}