using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIStarRate : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    private int id;
    private PopupRate popupRate;
    
    public void Setup(int id, PopupRate popupRate)
    {
        this.id = id;
        this.popupRate = popupRate;
        transform.localScale = Vector3.zero;
        transform.DOScale(1, 0.3f).SetDelay(id * 0.1f);
    }
    
    public void SetStar(bool isOn)
    {
        fillImage.gameObject.SetActive(isOn);
    }

    public void OnClickStar()
    {
        popupRate.OnSelectStar(id);
    }
}
