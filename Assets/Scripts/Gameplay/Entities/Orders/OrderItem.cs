using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gameplay;
using Gameplay.Entities;
using Gameplay.LevelData;
using Manager;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class OrderItem : MonoBehaviour
{
    [SerializeField] private FixedImageRatio icon;
    [SerializeField] private Image bg;
    [SerializeField] private Sprite[] bgSprites;
    [SerializeField] private TMP_Text txtQuantity;
    [SerializeField] private Image completeTick;
    [SerializeField] private Transform spicyObject;
    private int id;
    private int quantity;
    private int collected;
    public bool IsCompleted => collected >= quantity;
    private Transform target;
    private bool spicy;
    private OrderList orderList;

    public void SetOrder(int id, bool spicy, Transform targetContainer, OrderList orderList)
    {
        this.id = id;
        this.spicy = spicy;
        this.orderList = orderList;
        this.quantity = 1;
        collected = 0;
        icon.SetSpriteAsync(PathManager.ItemSprite(id)).Forget();
        bg.sprite = spicy ? bgSprites[2] : bgSprites[0];
        spicyObject.gameObject.SetActive(spicy);
        
        txtQuantity.text = $"{collected}/{quantity}";
        completeTick.gameObject.SetActive(false);
        if (target == null)
        {
            target = new GameObject().transform;
            target.parent = targetContainer;
        }
        else
        {
            if (target.childCount > 0)
            {
                Destroy(target.GetChild(0).gameObject);
            }
        }
    }

    public void AddItem(int id, PrimaryGrill grill, float duration)
    {
        if (id != this.id || IsCompleted) return;

        if (spicy && (grill != null && grill.grillType != GrillType.Spicy))
        {
            // Stuck
            if (!GameplayController.instance.levelGenerator.CheckItemById(this.id))
            {
                orderList.StuckOrder();
            }

            return;
        }

        collected++;
        SonatUtils.DelayCall(duration + GameDefine.itemMergeDuration + 0.165f, () =>
        {
            if (collected >= quantity)
            {
                Complete();
            }

            txtQuantity.text = $"{collected}/{quantity}";
        }, this);
    }

    public void Complete()
    {
        bg.sprite = bgSprites[1];
        completeTick.transform.DOKill();
        completeTick.gameObject.SetActive(true);
        completeTick.transform.localScale = Vector3.one * 2.75f;
        completeTick.SetAlpha(0);
        completeTick.DOFade(1, 0.35f).SetEase(Ease.InCubic);
        completeTick.transform.DOScale(1, 0.35f).SetEase(Ease.InOutQuad);
    }

    public bool NeedItem(int id, PrimaryGrill grill)
    {
        if (spicy && grill != null && grill.grillType != GrillType.Spicy) return false;
        return id == this.id && collected < quantity;
    }

    public (int, int) OrderRemain()
    {
        return (id, quantity - collected);
    }

    public Transform GetIconTarget()
    {
        target.position = icon.transform.position;
        return target;
    }

    public Data GetData()
    {
        return new Data()
        {
            id = this.id,
            spicy = this.spicy,
            isCompleted = this.IsCompleted,
        };
    }

    public class Data
    {
        public int id;
        public bool spicy;
        public bool isCompleted;
    }
}