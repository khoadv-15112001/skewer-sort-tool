using System.Collections;
using Sonat.Enums;
using SonatFramework.Scripts.Utils;
using UnityEngine;

namespace SonatFramework.Scripts.Feature.ChestRewardProgress
{
    [RequireComponent(typeof(RectTransform))]
    public class ChestDisplayUIController : MonoBehaviour
    {
        [SerializeField] private ChestAnimationInterface chestAnim;
        [SerializeField] private ChestState currentState = ChestState.Locked;

        private void Awake()
        {
            UpdateChestState(currentState);
            StartCoroutine(PlaySound());
        }

        IEnumerator PlaySound()
        {
            yield return new WaitForSeconds(1f);
            MySonatFramework.audioService.PlaySound(AudioId.Chest_Level_Appear);
            // while (gameObject.activeInHierarchy)
            // {
            //     yield return new WaitForSeconds(3f);
            //     MySonatFramework.audioService.PlaySound(AudioId.Chest_Level_Idle);
            // }
            
        }

        private void OnDestroy()
        {
            chestAnim?.Cleanup();
        }

        public void UpdateChestState(ChestState newState)
        {
            currentState = newState;
            chestAnim?.SetChestState(newState);

            switch (newState)
            {
                case ChestState.Locked:
                    chestAnim?.PlayIdleAnimation();
                    break;
                case ChestState.Opening:
                    chestAnim?.PlayOpenAnimation();
                    break;
                case ChestState.Opened:
                    chestAnim?.PlayIdleOpenAnimation();
                    break;
            }
        }
    }
}