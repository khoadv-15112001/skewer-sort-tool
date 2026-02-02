using DG.Tweening;
using UnityEngine;

namespace Gameplay.Entities
{
    public class SubSlot : SlotBase
    {
        protected override void DOScaleItemIntro(Item item)
        {
            item.transform.DOKill();
            item.transform.DOScale(0.55f, GameDefine.itemScaleIntro).From(0).SetEase(Ease.OutBack);
        }
    }
}