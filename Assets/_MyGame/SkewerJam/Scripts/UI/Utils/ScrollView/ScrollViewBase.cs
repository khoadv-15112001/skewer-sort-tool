using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using SkewerJam.Features.Leaderboard.Service;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MyGame.UI.ScrollView
{
    public abstract class ScrollViewBase : MonoBehaviour, LoopScrollDataSource, LoopScrollPrefabSource
    {
        [SerializeField] protected LoopScrollRect scroll;

        [SerializeField] protected ItemViewBase myRank;
        [SerializeField] protected ItemViewBase leaderboardItemViewPrefab;

        [Header("Loading...")]
        [SerializeField] protected int maxElements = 100;
        [SerializeField] protected GameObject loading;


        protected Transform poolRoot;
        protected Stack<Transform> pool = new();

        protected LeaderboardResponseData responseData;

        protected readonly HashSet<int> activeItems = new();
        protected readonly Dictionary<Transform, int> transToIndex = new();

        // private readonly Service<LeaderboardService> leaderboardService = new();

        protected virtual void Awake()
        {
            poolRoot = new GameObject("PoolRoot").transform;
            poolRoot.SetParent(transform, false);
            poolRoot.gameObject.SetActive(false);

            scroll.prefabSource = this;
            scroll.dataSource = this;

            scroll.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnValueChanged(Vector2 value)
        {
            if (responseData == null)
            {
                myRank?.gameObject.SetActive(false);
                return;
            }

            myRank?.gameObject.SetActive(!IsIndexVisibleNow(responseData.position - 1));
        }

        protected virtual void OnEnable()
        {
            activeItems.Clear();
            transToIndex.Clear();

            myRank?.gameObject.SetActive(false);

        }

        protected abstract UniTaskVoid FetchData();

        /// <summary>
        /// Sau khi FetchData thành công thì nhớ gọi hàm này
        /// để set lại số item hiển thị (tối đa 20).
        /// </summary>
        protected void RefreshScroll()
        {
            if (responseData == null || responseData.list == null)
            {
                scroll.totalCount = 0;
            }
            else
            {
                scroll.totalCount = Mathf.Min(responseData.list.Count, maxElements); // Giới hạn 20 item
            }

            scroll.RefillCells();
        }

        public void ProvideData(Transform transform, int idx)
        {
            transToIndex[transform] = idx;
            activeItems.Add(idx);

            var view = transform.GetComponent<ItemViewBase>();

            if (idx + 1 != responseData.position)
                view.Bind(responseData.list[idx], idx + 1);
            else
                view.BindSelf(responseData.score, idx + 1);
        }

        public GameObject GetObject(int index)
        {
            if (pool.Count == 0)
                return Instantiate(leaderboardItemViewPrefab).gameObject;

            var t = pool.Pop();
            t.gameObject.SetActive(true);
            return t.gameObject;
        }

        public void ReturnObject(Transform trans)
        {
            if (transToIndex.TryGetValue(trans, out var idx))
            {
                activeItems.Remove(idx);
                transToIndex.Remove(trans);
            }

            trans.SendMessage("ScrollCellReturn", SendMessageOptions.DontRequireReceiver);
            trans.gameObject.SetActive(false);
            trans.SetParent(poolRoot, false);
            pool.Push(trans);
        }

        protected bool IsIndexVisibleNow(int index) => activeItems.Contains(index);

        public GameObject GetObjectByIndex(int index)
        {
            foreach (var kvp in transToIndex)
            {
                if (kvp.Value == index)
                {
                    return kvp.Key.gameObject;
                }
            }
            return null;
        }
    }
}
