using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.EndlessTreasure
{
    public class UIEndlessTreasure : MonoBehaviour
    {
        [SerializeField] private UITimeCounter timeCounter;
        [SerializeField] private UITimeCounter timeCooldown;
        [SerializeField] private Transform container;
        [SerializeField] private Transform[] itemPositions;
        [SerializeField] private Transform startPosLast;
        [SerializeField] private Transform endPosFirst;

        [Header("Animation update layout")]
        [SerializeField] private float duration = 0.3f;
        [SerializeField] private float delay = 0.3f;
        [SerializeField] private float earlyTime = 0.1f;

        private readonly Service<EndlessTreasureService> endlessTreasureService = new();
        private readonly Service<PoolingContainerService> poolingContainerService = new();

        private Queue<UIItem> uiItemsQueue = new();
        private void OnEnable()
        {
            UpdateUI(true);
            endlessTreasureService.Instance.OnChangeData += UpdateUI;
        }

        private void OnDisable()
        {
            endlessTreasureService.Instance.OnChangeData -= UpdateUI;
        }

        private void UpdateUI(bool force)
        {
            // set data
            var currentIdx = endlessTreasureService.Instance.data.currentPackIdx;
            var numPack = endlessTreasureService.Instance.config.GetMaxPack();
            if (force)
            {
                var timeToReset = endlessTreasureService.Instance.GetTimeToReset();
                timeCounter.SetData(timeToReset, null);
                foreach (var item in uiItemsQueue)
                {
                    item.gameObject.SetActive(false);
                }
                uiItemsQueue.Clear();
                for (int i = 0; i < itemPositions.Length; i++)
                {
                    var uiItem = poolingContainerService.Instance.CreateObject<UIItem>(container);

                    int idx = endlessTreasureService.Instance.GetCurrentIdxByOrderNumber(i);
                    uiItem.SetData(idx);
                    uiItem.transform.position = itemPositions[i].position;

                    uiItemsQueue.Enqueue(uiItem);
                }
            }
            else
            {
                var newIdx = endlessTreasureService.Instance.GetCurrentIdxByOrderNumber(itemPositions.Length - 1);
                var uiItem = poolingContainerService.Instance.CreateObject<UIItem>(container);
                uiItem.SetData(newIdx);
                uiItem.transform.position = startPosLast.position;
                uiItemsQueue.Enqueue(uiItem);
            }


            foreach (var item in uiItemsQueue)
            {
                item.UpdateUI(force);
            }

            if (force == false)
            {
                var item = uiItemsQueue.Peek();
                item.SetClaimed();
            }

            if (force)
            {
                DOVirtual.DelayedCall(0.1f, () => 
                {
                    CheckCooldown(null, true);
                });
            }
        }

        private void CheckCooldown(Action action, bool force)
        {
            //Debug.Log($"anhnt: UpdateLayout currentIdx={endlessTreasureService.Instance.data.currentPackIdx}, cd={endlessTreasureService.Instance.Cooldown}");
            if (endlessTreasureService.Instance.Cooldown > 0)
            {
                var item = uiItemsQueue.Peek();

                item.ForceLock();

                item.SetInteractableText(false);

                timeCooldown.gameObject.SetActive(true);

                timeCooldown.SetData(endlessTreasureService.Instance.GetCooldownTime(), () =>
                {
                    endlessTreasureService.Instance.ResetCooldown();

                    timeCooldown.gameObject.SetActive(false);

                    item.UpdateUI(force);

                    item.SetInteractableText(true);

                    action?.Invoke();
                });
            }
            else
            {
                timeCooldown.gameObject.SetActive(false);

                action?.Invoke();
            }
        }
        public void UpdateLayout()
        {
            int i = 0;
            var currentIdx = endlessTreasureService.Instance.data.currentPackIdx;
            var d = delay;

            foreach (var item in uiItemsQueue)
            {
                var end = i == 0 ? endPosFirst.position : itemPositions[i - 1].position;
                item.transform.DOMove(end, duration).SetDelay(d).OnComplete(() =>
                {
                    if (item.PackIdx == currentIdx)
                    {
                        CheckCooldown(() => { item.PlayAnimationUnlock(); }, false);
                        item.IsCooldown = true;
                    }
                });
                i++;
                d += duration - earlyTime;
            }

            SonatUtils.DelayCall(duration + d, () =>
            {
                var uiItem = uiItemsQueue.Dequeue();
                uiItem.gameObject.SetActive(false);
            }, this);
        }
    }
}
