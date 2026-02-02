using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Spine.Unity;

namespace GrillSort.Rail
{
    public class UIRailGiftBox : MonoBehaviour
    {
        [SerializeField] private SkeletonGraphic anim;
        [SerializeField] private Transform openTransform;
        [SerializeField] private ParticleSystem openBlueEffect;
        [SerializeField] private ParticleSystem openRedEffect;
        [SerializeField] private ParticleSystem openPurpleEffect;

        private int milestoneIndex;
        private int totalMilestones;
        private Skin skin;
        private bool isOpened = false;
        private bool isWaitingToOpen = false;
        private int tapCount = 0; // Đếm số lần tap (0-3)

        public enum State
        {
            Idle1,
            Idle2,
            OpenBox,
            OpenBox1,
            OpenBox2,
            OpenBox3,
            OpenBox_Idle,
            Slide,
            Idle_Open
        }

        public enum Skin
        {
            Blue,
            Purple,
            Red
        }

        public void Setup(int index, int total)
        {
            milestoneIndex = index;
            totalMilestones = total;

            DetermineSkin();

            // Check xem box này đã mở chưa (milestone index < currentMilestone nghĩa là đã mở)
            int currentMilestone = RailService.Instance.CurrentMilestone.Value;
            if (milestoneIndex < currentMilestone)
            {
                // Box đã mở
                isOpened = true;
                PlayAnimation(State.Idle_Open, true);
            }
            else
            {
                // Box chưa mở - khởi tạo với animation Idle1
                PlayAnimation(State.Idle1, true);
            }
        }

        private void DetermineSkin()
        {
            // Milestone cuối → Purple
            if (milestoneIndex == totalMilestones)
            {
                skin = Skin.Purple;
            }
            // Milestone chẵn → Blue
            else if (milestoneIndex % 2 == 1)
            {
                skin = Skin.Blue;
            }
            // Milestone lẻ → Red
            else
            {
                skin = Skin.Red;
            }

            string skinName = GetLocalizedSkinName(skin);
            anim.initialSkinName = skinName;
            anim.Initialize(true);
        }

        private string GetLocalizedSkinName(Skin skin)
        {
            string baseName = skin.ToString();

            // Nếu là JP thì thêm _JP
            if (LocalizationUtils.IsJapanese())
            {
                string jpSkinName = baseName + "_JP";

                // Kiểm tra xem skin có tồn tại không
                var jpSkin = anim.SkeletonData.FindSkin(jpSkinName);
                if (jpSkin != null)
                {
                    return jpSkinName;
                }
            }

            // Mặc định trả về tên skin gốc (global)
            return baseName;
        }

        public void PlayAnimation(State state, bool isLoop)
        {
            // Nếu box đã mở, chỉ cho phép chạy Idle_Open
            if (isOpened && state != State.Idle_Open)
            {
                return;
            }

            if (anim != null)
            {
                anim.AnimationState.SetAnimation(0, state.ToString(), isLoop);
            }
        }

        public void PlayAnimationWithCallback(State state, bool isLoop, Action onComplete)
        {
            // Nếu box đã mở, chỉ cho phép chạy Idle_Open
            if (isOpened && state != State.Idle_Open)
            {
                return;
            }

            if (anim != null)
            {
                var trackEntry = anim.AnimationState.SetAnimation(0, state.ToString(), isLoop);

                if (onComplete != null && trackEntry != null)
                {
                    trackEntry.Complete += (entry) => onComplete?.Invoke();
                }
            }
        }

        public void PlayOpenEffect()
        {
            switch (skin)
            {
                case Skin.Blue:
                    openBlueEffect.Play();
                    break;

                case Skin.Red:
                    openRedEffect.Play();
                    break;

                case Skin.Purple:
                    openPurpleEffect.Play();
                    break;

                default:
                    openBlueEffect.Play();
                    break;
            }
        }

        public Skin GetSkin()
        {
            return skin;
        }

        public void MarkAsOpened()
        {
            isOpened = true;
            isWaitingToOpen = false;
            PlayAnimation(State.Idle_Open, true);
        }

        public void SetWaitingToOpen()
        {
            if (!isOpened)
            {
                isWaitingToOpen = true;
                PlayAnimation(State.Idle2, true);
            }
        }

        public void SetIdleNormal()
        {
            if (!isOpened && !isWaitingToOpen)
            {
                PlayAnimation(State.Idle1, true);
            }
        }

        public bool IsOpened()
        {
            return isOpened;
        }

        public int GetMilestoneIndex()
        {
            return milestoneIndex;
        }

        public Vector2 GetOpenPosition()
        {
            return openTransform.position;
        }

        /// <summary>
        /// Xử lý logic tap mới (tap 4 lần để mở box)
        /// Returns true nếu đây là tap cuối cùng (tap 4)
        /// 
        /// Cách sử dụng:
        /// <code>
        /// // Khi người dùng tap vào box
        /// bool isFinalTap = box.ProcessTap(out var charState, out var boxState);
        /// 
        /// if (!isFinalTap) // Tap 1-3: Play animation rồi chuyển sang Idle
        /// {
        ///     character.PlayAnimWithCallback(charState, false, () => {
        ///         character.PlayAnim(box.GetCharacterIdleState(), true);
        ///     });
        ///     box.PlayAnimationWithCallback(boxState, false, () => {
        ///         box.PlayAnimation(State.OpenBox_Idle, true);
        ///     });
        /// }
        /// else // Tap 4: Mở hẳn box
        /// {
        ///     character.PlayAnim(charState, false);
        ///     box.PlayAnimation(boxState, false);
        ///     box.MarkAsOpened();
        /// }
        /// </code>
        /// </summary>
        public bool ProcessTap(out UIRailCharacter.State characterState, out State boxState)
        {
            if (isOpened)
            {
                characterState = UIRailCharacter.State.Idle1;
                boxState = State.Idle_Open;
                return false;
            }

            tapCount++;

            // Tap 1: OpenBox1
            if (tapCount == 1)
            {
                characterState = GetCharacterOpenState(1);
                boxState = State.OpenBox1;
                return false;
            }
            // Tap 2-3: OpenBox2
            else if (tapCount >= 2 && tapCount <= 3)
            {
                characterState = GetCharacterOpenState(2);
                boxState = State.OpenBox2;
                return false;
            }
            // Tap 4: OpenBox3 - Tap cuối cùng, mở hẳn box
            else
            {
                characterState = GetCharacterOpenState(3);
                boxState = State.OpenBox3;
                return true;
            }
        }

        /// <summary>
        /// Lấy state animation cho character dựa vào skin và tap phase
        /// </summary>
        private UIRailCharacter.State GetCharacterOpenState(int phase)
        {
            switch (skin)
            {
                case Skin.Blue:
                    if (phase == 1) return UIRailCharacter.State.OpenBox_Blue1;
                    if (phase == 2) return UIRailCharacter.State.OpenBox_Blue2;
                    if (phase == 3) return UIRailCharacter.State.OpenBox_Blue3;
                    break;

                case Skin.Red:
                    if (phase == 1) return UIRailCharacter.State.OpenBox_Red1;
                    if (phase == 2) return UIRailCharacter.State.OpenBox_Red2;
                    if (phase == 3) return UIRailCharacter.State.OpenBox_Red3;
                    break;

                case Skin.Purple:
                    if (phase == 1) return UIRailCharacter.State.OpenBox_Purple1;
                    if (phase == 2) return UIRailCharacter.State.OpenBox_Purple2;
                    if (phase == 3) return UIRailCharacter.State.OpenBox_Purple3;
                    break;
            }

            // Fallback
            return UIRailCharacter.State.OpenBox_Blue1;
        }

        /// <summary>
        /// Lấy state Idle cho character dựa vào skin (dùng khi animation OpenBox1/2/3 kết thúc)
        /// </summary>
        public UIRailCharacter.State GetCharacterIdleState()
        {
            switch (skin)
            {
                case Skin.Blue:
                    return UIRailCharacter.State.OpenBox_Blue_Idle;
                case Skin.Red:
                    return UIRailCharacter.State.OpenBox_Red_Idle;
                case Skin.Purple:
                    return UIRailCharacter.State.OpenBox_Purple_Idle;
                default:
                    return UIRailCharacter.State.OpenBox_Blue_Idle;
            }
        }

        /// <summary>
        /// Reset tap count khi cần (ví dụ khi milestone thay đổi)
        /// </summary>
        public void ResetTapCount()
        {
            tapCount = 0;
        }

        public int GetTapCount()
        {
            return tapCount;
        }
    }
}

