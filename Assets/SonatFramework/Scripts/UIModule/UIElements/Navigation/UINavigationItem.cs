using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sonat.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UINavigationItem : MonoBehaviour
{
    public NavigationType type;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text txtName;
    [SerializeField] private LayoutElement layoutElement;
    [SerializeField] private float selectFlexibleWidth = 1.8f;
    [SerializeField] private float scaleSelected = 1.35f;
    private UINavigateBarBase navigateBar;

    public void InitData(UINavigateBarBase navigateSwipeBar)
    {
        this.navigateBar = navigateSwipeBar;
        txtName.gameObject.SetActive(false);
    }

    public void OnClickThis()
    {
        navigateBar.SwitchTab(this.type);
    }

    public void OnSelected()
    {
        icon.transform.DOKill();
        layoutElement.DOKill();
        icon.transform.DOScale(scaleSelected, 0.2f);
        layoutElement.DOFlexibleSize(new Vector2(selectFlexibleWidth, 1), 0.2f);
        txtName.gameObject.SetActive(true);
    }

    public void OnDeselected()
    {
        icon.transform.DOKill();
        layoutElement.DOKill();
        icon.transform.DOScale(1, 0.2f);
        layoutElement.DOFlexibleSize(Vector3.one, 0.2f);
        txtName.gameObject.SetActive(false);
    }
}
