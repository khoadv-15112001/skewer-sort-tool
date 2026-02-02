using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MyGame.UI.ScrollView;
using Sirenix.OdinInspector;
using SkewerJam.Features.Leaderboard.Service;
using SonatFramework.Systems;
using UnityEngine;

namespace MyGame.Leaderboard.UI
{
    public class LeaderboardScrollView : ScrollViewBase
    {
        [Space(10)]
        [Header("Request config")]
        [SerializeField] protected LeaderboardGroup requestGroup;
        [SerializeField] protected LeaderboardFilterType filterType;
        [SerializeField] protected int totalRequest = 100;

        [Header("Scroll config")]
        [SerializeField] private bool playOnAwake = false;
        [SerializeField, ShowIf("playOnAwake")] private bool isHorizontal = false;
        [SerializeField, ShowIf("playOnAwake")] private float duration = 1f;

        private readonly Service<LeaderboardHLWService> leaderboardService = new();

        protected override void OnEnable()
        {
            base.OnEnable();

            loading.SetActive(true);

            FetchData().Forget();
        }

        protected override async UniTaskVoid FetchData()
        {
            scroll.gameObject.SetActive(false);

            int limitRequest;

            if (CheatManager.IsOpenCheat())
                limitRequest = LeaderboardService.limitTopLeaderboard > 0 ? LeaderboardService.limitTopLeaderboard : totalRequest;
            else
                limitRequest = totalRequest;

            responseData = await leaderboardService.Instance.FetchLeaderboard(limitRequest);
            if (responseData == null)
            {
                PopupToast.Cretate("Loading failed! Please retry later.");
                return;
            }
            
            if(!scroll.gameObject.activeSelf) return;

            scroll?.gameObject.SetActive(true);
            scroll.totalCount = responseData.list.Count;
            scroll.RefillCells();
            myRank?.BindSelf(responseData.score, responseData.position);
            myRank?.gameObject.SetActive(!IsIndexVisibleNow(responseData.position - 1));
            loading.SetActive(false);

            RefreshScroll();

            // Trượt scroll về cuối danh sách
            scroll.RefillCellsFromEnd();

            if (playOnAwake)
            {
                PlayScrollOnAwake().Forget();
            }
        }

        private async UniTaskVoid PlayScrollOnAwake()
        {
            await UniTask.DelayFrame(1);

            if (isHorizontal)
            {
                scroll.horizontalNormalizedPosition = 1;
                await DOTween.To(() => scroll.horizontalNormalizedPosition, x => scroll.horizontalNormalizedPosition = x, 0, duration).SetEase(Ease.OutSine);
            }
            else
            {
                scroll.verticalNormalizedPosition = 1;
                await DOTween.To(() => scroll.verticalNormalizedPosition, x => scroll.verticalNormalizedPosition = x, 0, duration).SetEase(Ease.OutSine);
            }

        }
    }
}