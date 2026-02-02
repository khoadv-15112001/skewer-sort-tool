using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gameplay.LevelData;
using Manager;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using TMPro;
using UnityEngine;

namespace Gameplay.Entities.Items
{
    public class ItemBombMove : Item
    {
        [SerializeField] private TMP_Text textBombCountdown;
        [SerializeField] private Transform bombObject;

        [Header("Effect")][SerializeField] private ParticleSystem particleExplosion;
        [SerializeField] private SpriteRenderer itemsSprite;
        [SerializeField] private Material materialExplosion;
        [SerializeField] private Material materialDefault;

        [SerializeField] private float delayTimeToEndGame = 2f;
        [SerializeField] private float scaleExplosion = 2f;
        [SerializeField] private float scaleDuration = 0.3f;
        [SerializeField] private int waitTime = 500;
        private int moveRemaining;
        private bool exploded = false;
        public bool Exploded => exploded;
        public int MoveRemaining => moveRemaining;
        private PopupWarningBomb popupWarningBomb;

        public override void SetItemData(ItemData data, SlotBase slot)
        {
            this.data = data;
            this.slot = slot;
            if (data == null)
            {
                visual.gameObject.SetActive(false);
                id = 0;
            }
            else
            {
                id = data.id;

                if (data is ItemBombData bombData)
                {
                    moveRemaining = bombData.moveLimit;
                    textBombCountdown.text = moveRemaining.ToString();
                    if (moveRemaining == 0) SkipBomb();
                }
                else
                {
                    bombData = new()
                    {
                        id = data.id,
                        itemType = data.itemType,
                        moveLimit = GameRemoteConfigValue.itemBombLimit
                    };
                    this.data = bombData;
                }

                visual.gameObject.SetActive(true);
                visual.SetVisual(data);
                matType = CheckItemType(id);
                SonatUtils.DelayCall(1.25f, CheckWarningBomb, this);
                if (isPrimary) itemState = ItemState.Idle;
            }
        }

        public override void SetPrimary(bool isPrimary)
        {
            base.SetPrimary(isPrimary);
            if (isPrimary)
            {
                StartBomb();
            }
        }

        public void StartBomb()
        {
            exploded = false;
            itemBehaviorSO.eventSystemSO.RegisterEvents_OnDropItem(OnItemDropped);
        }

        private void OnDisable()
        {
            itemBehaviorSO.eventSystemSO.UnregisterEvents_OnDropItem(OnItemDropped);
            if (popupWarningBomb != null)
            {
                popupWarningBomb.FinishWarning();
                popupWarningBomb = null;
            }
        }

        public void SkipBomb()
        {
            bombObject.gameObject.SetActive(false);
            itemBehaviorSO.eventSystemSO.UnregisterEvents_OnDropItem(OnItemDropped);
            exploded = true;
            ((ItemBombData)data).moveLimit = 0;
        }

        public override void OnComplete()
        {
            exploded = true;
            base.OnComplete();
        }

        private void OnItemDropped(Item item, bool changed)
        {
            if (!changed || exploded) return;

            ProcessBomb(item).Forget();
        }

        private async UniTask ProcessBomb(Item item)
        {
            await UniTask.Delay(300);
            if (exploded) return;
            moveRemaining--;
            if (data != null && data is ItemBombData)
            {
                ((ItemBombData)data).moveLimit = moveRemaining;
            }

            moveRemaining = Mathf.Max(moveRemaining, 0);
            textBombCountdown.text = moveRemaining.ToString();
            bombObject.DOKill();
            _ = bombObject.DOScale(1.2f, 0.1f).SetLoops(2, LoopType.Yoyo);

            if (moveRemaining <= 0)
            {
                await itemBehaviorSO.ProcessExplodeBomb(this);
                if (exploded) return;
                PreExplodeBomb();
            }
            else
            {
                MySonatFramework.audioService.PlaySound(AudioId.Obstacle_Bomb_counting_Grill_sort, 0.5f);
                CheckWarningBomb();
            }
        }

        private void CheckWarningBomb()
        {
            if (!exploded && moveRemaining <= 3 && popupWarningBomb == null)
            {
                popupWarningBomb = PanelManager.Instance.OpenPanel<PopupWarningBomb>(new UIData().Add("Bomb", this));
            }
        }

        private void PreExplodeBomb()
        {
            exploded = true;
            if (popupWarningBomb)
            {
                popupWarningBomb.FinishWarning();
                popupWarningBomb = null;
            }

            PanelManager.Instance.OpenPanelByName<PopupSkipBomb>("PopupBombExplosive",
                new UIData().Add("Bomb", this).Add("OnSkipBomb", (Action)SkipBomb).Add("OnGiveUp", (Action)ExplodeBomb));
        }

        public void ExplodeBomb()
        {
            exploded = true;

            itemBehaviorSO.OnExplodeBomb(this);
            PlayEffect().Forget();
        }

        private async UniTask PlayEffect()
        {
            _ = bombObject.DOScale(scaleExplosion, scaleDuration).OnComplete(() =>
            {
                bombObject.gameObject.SetActive(false);
            });

            await UniTask.Delay(waitTime);
            particleExplosion.Play();
            MySonatFramework.audioService.PlaySound(AudioId.Obstacle_Bomb_explosion_Grill_sort);
            itemsSprite.material = materialExplosion;
        }

        public override void OnCreateObj(params object[] args)
        {
            base.OnCreateObj(args);
            moveRemaining = GameRemoteConfigValue.itemBombLimit;
            textBombCountdown.text = moveRemaining.ToString();
            exploded = false;

            itemsSprite.material = materialDefault;
            bombObject.gameObject.SetActive(true);
            bombObject.localScale = Vector3.one;
        }

        public void SetBombCount(int bombCount)
        {
            moveRemaining = bombCount;
            textBombCountdown.text = moveRemaining.ToString();
        }
    }
}