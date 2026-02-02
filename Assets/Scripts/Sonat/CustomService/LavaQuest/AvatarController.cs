using DG.Tweening;
using Facebook.Unity;
using SonatFramework.Systems;
using SonatFramework.Systems.AudioManagement;
using SonatFramework.Systems.SettingsManagement.Vibation;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GrillSort.LavaQuest
{
    public class AvatarController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private List<UIAvatarLavaQuest> avatars = new();
        [SerializeField] private List<UIBoxLavaQuest> clouds = new();

        [SerializeField] private AudioClip jumpAudio;
        [SerializeField] private AudioClip sinkAudio;

        private int initStep;

        public void Hide()
        {
            //canvasGroup.alpha = 0;
        }

        public void Init(int step)
        {
            initStep = step;

            var cloud = clouds[step];

            cloud.Show(false);

            var playerDatas = LavaQuestService.Instance.GetXPlayersLive(avatars.Count);

            for (int i = avatars.Count - 1; i >= 0; i--)
            {
                var avatar = avatars[i];

                var intervalIndex = avatars.Count - i - 1;

                if (intervalIndex >= playerDatas.Count)
                {
                    avatar.gameObject.SetActive(false);
                }
                else
                {
                    avatar.Init(playerDatas[intervalIndex]);
                    avatar.gameObject.SetActive(true);
                    avatar.transform.position = cloud.GetPlaceHolder(i).position;
                    avatar.transform.SetParent(cloud.boxBone);
                    avatar.transform.SetAsFirstSibling();
                }
            }

            cloud.Show(true);
        }

        public void HideBox(int step)
        {
            for (int i = 0; i < clouds.Count; i++)
            {
                if (i < step && !clouds[i].isStartPlace)
                {
                    clouds[i].gameObject.SetActive(false);
                }
                else
                {
                    clouds[i].gameObject.SetActive(true);
                }
            }
        }

        public void SinkBox()
        {
            if (clouds[initStep].isStartPlace) return;

            clouds[initStep].Sink();
            SonatSystem.GetService<AudioService>().PlayAudio("", sinkAudio);
        }

        public float timeGap = 0.1f;
        public float jumpPower = 1f;
        public float jumpDuration = 0.3f;
        public float jumpMissDuration = 2f;
        [SerializeField] private AnimationCurve delayCurve = AnimationCurve.Linear(0, 0, 1, 1);

        public void Jump(int step, bool canFallLastAvatar = false)
        {
            var cloud = clouds[step];
            int avatarCount = avatars.Count;

            bool playedAnim = false;

            int lostPlayers = Mathf.Max(0, LavaQuestService.CacheCountLastPlayer - LavaQuestService.CacheCountCurPlayer);

            int fallAvatarCount;
            if (LavaQuestService.CacheCountCurPlayer > avatarCount)
            {
                fallAvatarCount = Mathf.Min(3, lostPlayers);
            }
            else
            {
                fallAvatarCount = lostPlayers;
            }

            if (clouds[step - 1].isStartPlace)
            {
                fallAvatarCount = 0;
            }

            List<int> candidateIndexes = new List<int>();
            for (int i = avatarCount - 1; i >= 0; i--)
            {
                if (i == avatarCount - 1 && !canFallLastAvatar)
                    continue;

                candidateIndexes.Add(i);
            }

            // Shuffle và lấy số lượng cần
            //candidateIndexes = candidateIndexes.OrderBy(x => UnityEngine.Random.value).ToList();
            var fallIndexes = candidateIndexes.Take(fallAvatarCount).ToList();

            int start = 0;
            int startInterval = avatarCount - 1;

            for (int i = avatarCount - 1; i >= 0; i--)
            {
                var avatar = avatars[i];

                if (!avatar.gameObject.activeSelf) continue;

                var target = cloud.GetPlaceHolder(startInterval);

                float t = (float)(start) / (avatarCount - 1);
                float curveValue = delayCurve.Evaluate(t);
                float delay = curveValue * timeGap * avatarCount;

                if (fallIndexes.Contains(i))
                {

                }
                else
                {
                    start++;
                    startInterval--;

                    Sequence seq = DOTween.Sequence();
                    seq.AppendInterval(delay);

                    seq.Append(avatar.transform.DOJump(target.position, jumpPower, 1, jumpDuration)
                        .SetEase(Ease.OutQuad)
                        .OnStart(() =>
                        {
                            SonatSystem.GetService<VibrationService>().Vibrate(5);

                            if (!playedAnim)
                            {
                                playedAnim = true;
                                clouds[step - 1].PlayShake();
                            }

                            avatar.transform.SetParent(cloud.boxBone);
                            avatar.transform.SetAsFirstSibling();
                            SonatSystem.GetService<AudioService>().PlayAudio("", jumpAudio);
                        })
                    );

                    seq.Append(avatar.transform.DOPunchScale(
                        new Vector3(0, -0.2f, 0),
                        0.25f,
                        1,
                        0
                    ));
                }
            }
        }
    }
}