using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gameplay.Entities;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule.SpriteService;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using TMPro;
using UnityEngine;

namespace GrillSort.QuestEvent
{
    public class UIQuestItemInGame : MonoBehaviour
    {
        [SerializeField] protected FixedImageRatio icon;
        [SerializeField] protected TMP_Text txtValue;
        [SerializeField] protected ParticleSystem blastEffect;
        private bool scaleUp;
        private Coroutine collectAnim;
        public float scaleSpeed = 3f;
        public float scaleMax = 1.15f;

        protected EventBinding<LevelStartedEvent> startLevelEvent;
        protected EventBinding<LevelEndedEvent> levelEndedEvent;
        private readonly Service<QuestEventService> questEventService = new();

        private int currentItemId;

        public void OnEnable()
        {
            if (!questEventService.Instance.CheckEventActive() || questEventService.Instance.CheckCompleteAllQuest())
            {
                gameObject.SetActive(false);
                return;
            }

            // update visual
            currentItemId = questEventService.Instance.GetCurrentItemId();
            //icon.SetSpriteAsync(PathManager.ItemSprite(currentItemId)).Forget();
            icon.SetSprite(MySonatFramework.GetService<SpriteAtlasService>().GetSprite($"SpecialItem_{currentItemId}"));

            GameplayController.OnLoadLevel += OnStartLevel;
            levelEndedEvent = new EventBinding<LevelEndedEvent>(OnLevelEnded);
            GameplayController.OnSelectItem += OnSelectItem;
            CheatManager.CollectSpecialItem += OnCollectSpecialItem;

            // // Mở popup golden slice khi quest hiện tại chưa thu thập item nào
            // if (questEventService.Instance.Data.numItem == 0)
            // {
            //     SonatUtils.DelayCall(3f, () =>
            //     {
            //         var uiData = new UIData();
            //         uiData.Add("hideButtonPlay", true);
            //         PanelManager.Instance.OpenPanel<PopupGoldenSlice>(uiData);
            //     });
            // }
        }

        public void OnDisable()
        {
            GameplayController.OnLoadLevel -= OnStartLevel;
            EventBus<LevelEndedEvent>.Deregister(levelEndedEvent);
            GameplayController.OnSelectItem -= OnSelectItem;
            CheatManager.CollectSpecialItem -= OnCollectSpecialItem;
        }

        protected void OnStartLevel(int level)
        {
            questEventService.Instance.NumCollectedItemInGame = 0;
            UpdateValue(0);
        }

        private void OnLevelEnded(LevelEndedEvent @event)
        {
            if (@event.success)
            {
                Debug.Log($"UIQuestItemInGame: OnLevelEnded: {questEventService.Instance.NumCollectedItemInGame}");
            }
        }

        private void OnSelectItem(Item item)
        {
            if (item == null) return;

            if (item.id == currentItemId)
            {
                questEventService.Instance.CollectItemInGame(1);
                OnCollectItem(item.id, item.transform.position, icon.transform.position);
                item.ForceCollect();
                GameplayController.instance.SetLastTimeCollectItem(Time.time);
                GameplayController.instance.itemSelected = null;

                MySonatFramework.audioService.PlaySound(AudioId.Items_Special_Merge_Grill_sort);
            }
        }

        private void UpdateValue(int value, float duration = 0, float delay = 0)
        {
            txtValue.DOKill(true);
            int oldValue = questEventService.Instance.NumCollectedItemInGame;
            txtValue.DOCounter(oldValue, value, duration).SetDelay(delay);
            questEventService.Instance.NumCollectedItemInGame = value;
        }

        protected void OnCollectItem(int id, Vector3 startPosition, Vector3 targetPosition)
        {
            Action onComplete = () =>
            {
                UpdateValue(questEventService.Instance.NumCollectedItemInGame);
                PlayCollectEffect();
            };

            var collectEffect = new CollectEffectMultipleItemInGame()
            {
                itemId = id,
                radius = 0f,
                delayMove = 0f
            };
            collectEffect.Collect(GameResource.None, 1, startPosition, targetPosition, onComplete);

            MySonatFramework.audioService.PlaySound(AudioId.Items_Special_Collected_Grill_sort);
        }

        protected void PlayCollectEffect()
        {
            if (collectAnim != null)
            {
                //StopCoroutine(collectAnim);
                scaleUp = true;
                //blastEffect?.Play();
            }
            else
            {
                collectAnim = StartCoroutine(CollectEffect());
                //blastEffect?.gameObject.SetActive(true);
            }

            if (blastEffect)
            {
                blastEffect.Play();
            }
        }

        IEnumerator CollectEffect()
        {
            scaleUp = true;
            while (icon.transform.localScale.x < scaleMax)
            {
                icon.transform.localScale += Vector3.one * Time.deltaTime * scaleSpeed;
                yield return null;
            }

            //SettingManager.Vibrations(100);
            yield return null;
            scaleUp = false;

            while (icon.transform.localScale.x > 1)
            {
                if (scaleUp)
                {
                    collectAnim = StartCoroutine(CollectEffect());
                    yield break;
                }

                icon.transform.localScale -= Vector3.one * Time.deltaTime * scaleSpeed;
                yield return null;
            }

            icon.transform.localScale = Vector3.one;
            collectAnim = null;
        }

        #region Cheat

#if UNITY_EDITOR
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                questEventService.Instance.CollectItemInGame(1);
                OnCollectItem(currentItemId, Vector3.zero, icon.transform.position);
            }
        }
#endif


        private void OnCollectSpecialItem(int numItem)
        {
            //questEventService.Instance.NumCollectedItemInGame += numItem;
            CheatCollectItem(numItem).Forget();
        }

        private async UniTask CheatCollectItem(int numItem)
        {
            for (int i = 0; i < numItem; i++)
            {
                OnCollectItem(currentItemId, Vector3.zero, icon.transform.position);
                await UniTask.Delay(100);
            }
        }

        #endregion
    }
}