using System;
using Cysharp.Threading.Tasks;
using Gameplay.Entities;
using Gameplay.Entities.Items;
using MyGame.SkewerJam.Scripts.SO.Behavior;
using UnityEngine;

namespace MyGame.SkewerJam.Scripts.SO
{
    public abstract class ItemBehaviorSO : ScriptableObject
    {
        [SerializeField] public ItemVisualSO itemVisualSO;
        [SerializeField] public EventSystemSO eventSystemSO;

        public abstract void OnMouseDown(Item item);

        public abstract void OnMouseUp(Item item);

        public abstract void OnMouseExit(Item item);

        public abstract void SwitchSlot(Item item, SlotBase slot);

        public abstract void OnExplodeBomb(ItemBombMove itemBombMove);

        public abstract void OnSelected(Item item);

        public abstract void PlayDropSound(Item item);

        public virtual async UniTask ProcessExplodeBomb(ItemBombMove itemBombMove)
        {

        }
    }
}