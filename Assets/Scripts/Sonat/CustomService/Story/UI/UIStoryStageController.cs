using SonatFramework.Systems.ObjectPooling;
using SonatFramework.Systems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

namespace GrillSort.Story
{
    public class UIStoryStageController : MonoBehaviour
    {
        private readonly Service<PoolingContainerService> poolingService = new();

        [SerializeField] private Transform container;
        [SerializeField] private ScrollRect scrollRect;

        private RectTransform maxStoryUnlock;

        private void Awake()
        {
            poolingService.Instance.CleanContainer(container);
            InitStory();
        }

        private void OnEnable()
        {
            ScrollToMaxStory();
        }

        private void InitStory()
        {
            for (int i = 0; i < StoryService.Instance.AllStories.Count; i++)
            {
                var story = StoryService.Instance.AllStories[i];

                var uiStory = poolingService.Instance.CreateObject<UIStoryStage>(container);

                uiStory.BindData(story, i == StoryService.Instance.AllStories.Count - 1);

                if (StoryService.Instance.IsUnlockedStory(story) || i == 0)
                    maxStoryUnlock = uiStory.GetComponent<RectTransform>();
            }
        }

        public async UniTaskVoid ScrollToMaxStory()
        {
            await UniTask.Yield();
            ScrollToElement(maxStoryUnlock);
        }

        public void ScrollToElement(RectTransform target)
        {
            scrollRect.StopMovement();
            scrollRect.velocity = Vector2.zero;

            Canvas.ForceUpdateCanvases();

            RectTransform content = scrollRect.content;
            RectTransform viewport = scrollRect.viewport;

            float contentHeight = content.rect.height;
            float viewportHeight = viewport.rect.height;
            float targetY = Mathf.Abs(target.anchoredPosition.y) - target.rect.height * 0.5f;

            float scrollY = Mathf.Clamp01((targetY - viewportHeight * 0.5f + target.rect.height * 0.5f) / (contentHeight - viewportHeight));

            scrollRect.normalizedPosition = new Vector2(scrollRect.normalizedPosition.x, 1 - scrollY);
        }
    }
}