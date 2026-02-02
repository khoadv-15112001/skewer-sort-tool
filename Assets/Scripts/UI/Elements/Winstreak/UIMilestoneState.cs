using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class UIMilestoneState : MonoBehaviour
{
    public enum WinstreakState
    {
        normal,
        can_claim,
        clamed
    }
    [SerializeField] private GameObject pointTarget, poinCanClaim, pointClaimedObj;
    [SerializeField] private Button btnClaim;
    [SerializeField] private TMP_Text txtMilestone;
    [SerializeField] private RectTransform rectTransform;
    private WinstreakState state = WinstreakState.normal;

    //private void OnEnable()
    //{
    //    UpdateState(false, false);
    //}

    public void AddOnClick(UnityAction onClick)
    {
        btnClaim.onClick.RemoveAllListeners();
        btnClaim.onClick.AddListener(onClick);
    }
    public void UpdateState(bool complete, bool claimed, bool transition = false, float delay = 1)
    {
        if (claimed)
            state = WinstreakState.clamed;
        else
        {
            if (complete)
                state = WinstreakState.can_claim;
            else
                state = WinstreakState.normal;
        }

        if (transition && state > ((int)WinstreakState.normal))
        {
            var lastState = (WinstreakState)((int)state - 1);
            ApplyState(lastState);

            DOVirtual.DelayedCall(delay, () => { ApplyState(state); });
        }
        else
            ApplyState(state);
    }

    private void ApplyState(WinstreakState state)
    {
        pointTarget.SetActive(state == WinstreakState.normal);

        poinCanClaim.SetActive(state == WinstreakState.can_claim);

        pointClaimedObj.SetActive(state == WinstreakState.clamed);
    }

    internal void SetPos(float posY)
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();

        Vector2 pos = rectTransform.anchoredPosition;
        pos.y = posY;
        rectTransform.anchoredPosition = pos;
    }
    internal void SetText(int levelWinRequired)
    {
        if (txtMilestone)
            txtMilestone.text = $"{levelWinRequired}";
    }
}
