using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using SonatFramework.Systems;
using SonatFramework.Systems.AudioManagement;
using SonatFramework.Systems.SettingsManagement.Vibation;

namespace GrillSort.LavaQuest
{
    public class AvatarSpawner : MonoBehaviour
    {
        [SerializeField]
        private List<UIAvatarLavaQuest> uiAvatars = new();
        [SerializeField]
        private TMP_Text playerTxt;

        [SerializeField] private AudioClip countAudio;
        [SerializeField] private AudioClip lastAudio;

        [SerializeField] private ParticleSystem txtEffect;

        public AnimationCurve curve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        public float timeScale = 0.3f;
        public float timeGap = 0.1f;

        public void ResetText()
        {
            foreach (var uiAvatar in uiAvatars)
            {
                uiAvatar.transform.localScale = Vector3.zero;
            }

            playerTxt.text = "0/100";
        }

        [Button("Spawn")]
        public void Spawn()
        {
            int total = 100;
            int firstFew = Mathf.Min(4, uiAvatars.Count); // 3–4 người đầu
            int fixedIncrement = 1;
            int currentValue = 0;

            int remainingAvatars = uiAvatars.Count - firstFew;
            int remainingValue = total - firstFew * fixedIncrement;
            float stepPerAvatar = remainingAvatars > 0 ? (float)remainingValue / remainingAvatars : 0f;

            for (int i = uiAvatars.Count - 1; i >= 0; i--)
            {
                var uiAvatar = uiAvatars[i];
                var intervalIndex = uiAvatars.Count - i - 1;

                uiAvatar.Init(LavaQuestService.Instance.players.Value.datas[intervalIndex]);
                uiAvatar.transform.localScale = Vector3.zero;

                // target pos
                var targetPos = uiAvatar.transform.localPosition;
                var startPos = targetPos + Vector3.up * 100f; // nhảy từ trên xuống

                uiAvatar.transform.localPosition = startPos;

                // tính delay theo curve
                float t = (float)intervalIndex / (uiAvatars.Count - 1);
                float delay = curve.Evaluate(t) * timeGap * uiAvatars.Count;

                // Sequence scale + move
                Sequence seq = DOTween.Sequence();
                seq.SetDelay(delay);
                seq.Append(
                    uiAvatar.transform.DOScale(1f, timeScale).SetEase(Ease.OutBack)
                );
                seq.Join(
                    uiAvatar.transform.DOLocalMoveY(targetPos.y, timeScale).SetEase(Ease.OutCubic)
                );

                int index = i;

                seq.OnComplete(() =>
                {
                    if (intervalIndex < firstFew)
                        currentValue += fixedIncrement;
                    else
                        currentValue += Mathf.RoundToInt(stepPerAvatar);

                    if (currentValue > total) currentValue = total;
                    playerTxt.text = $"{currentValue}/{total}";

                    SonatSystem.GetService<VibrationService>().Vibrate(5);

                    if (index == 0)
                    {
                        playerTxt.DOScale(1.1f, 0.25f).SetLoops(2, LoopType.Yoyo)
                        .OnComplete(() =>
                        {
                            SonatSystem.GetService<VibrationService>().Vibrate(5);
                        });
                        txtEffect.Play();
                    }
                    SonatSystem.GetService<AudioService>().PlayAudio("", currentValue == total ? lastAudio : countAudio);

                    uiAvatar.transform.DOPunchScale(new Vector3(0, -0.1f, 0), 0.2f, 1, 0);
                });
            }
        }

    }
}