using DG.Tweening;
using Gameplay;
using Gameplay.Entities;
using MyGame.SkewerJam.Scripts.SO;
using UnityEngine;

public abstract class SlotBase : MonoBehaviour
{
    [SerializeField] protected GrillBase grill;
    [SerializeField] protected Transform container;

    protected SlotBaseBehaviorSO slotBaseBehaviorSO
    {
        get
        {
            return grill.GrillBaseBehaviorSO.slotBaseBehaviorSO;
        }
    }

    public int id { get; private set; }

    protected Item item;
    [HideInInspector] public bool isOnConveyor;

    public Transform Container => container;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (grill == null) grill = GetComponentInParent<GrillBase>();
    }
#endif

    public virtual void SetItem(Item item)
    {
        this.item = item;
        if (item != null)
        {
            item.transform.SetParent(container);
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;
            if (item != null)
            {
                DOScaleItemIntro(item);
            }
        }
    }

    protected virtual void DOScaleItemIntro(Item item)
    {
        item.transform.DOKill();
        item.transform.DOScale(1, GameDefine.itemScaleIntro).From(0).SetEase(Ease.OutBack);
    }

    public virtual void ClearItem()
    {
        if (this.item != null)
        {
            slotBaseBehaviorSO.gameFactorySO.ReturnEntity(item);
            this.item = null;
        }
    }


    public virtual void AddItem(Item item)
    {
        this.item = item;
        item.transform.SetParent(container);
    }


    public void ItemOut()
    {
        this.item = null;
        grill.OnSlotUpdated(this);
    }

    public bool isEmpty()
    {
        return item == null || !item.gameObject.activeInHierarchy;
    }

    public Item GetItem()
    {
        return item;
    }

    public void OnItemSelected()
    {
    }

    public void OnItemDrag()
    {
        item.transform.SetParent(null);
    }

    public void OnItemIntoSlot()
    {
        grill.OnSlotUpdated(this);
    }

    public void SetConveyor(bool isOn)
    {
        this.isOnConveyor = isOn;
        if (item != null)
        {
            item.SetIsOnConveyor(isOn);
        }
    }

    public void SetId(int id)
    {
        this.id = id;
    }

    public GrillBase GetGrill()
    {
        return grill;
    }

    public void ForceRemoveItem()
    {
        this.item = null;
    }
}