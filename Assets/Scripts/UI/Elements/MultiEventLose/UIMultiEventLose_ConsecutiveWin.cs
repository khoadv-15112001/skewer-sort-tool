using GrillSort.ConsecutiveWin;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIMultiEventLose_ConsecutiveWin : UIMultiEventLoseBase
{
    [SerializeField] private SkeletonGraphic boxAnim;
    [SerializeField] private TMP_Text txtTimeBonus;

    public override bool Setup()
    {
        var service = MySonatFramework.GetService<ConsecutiveWinService>();
        var level = MySonatFramework.userDataService.GetLevel();
        if (!service.CheckStart(level)) return false;

        SetupBoxAnim();

        return true;
    }

    private void SetupBoxAnim()
    {
        var service = MySonatFramework.GetService<ConsecutiveWinService>();
        int _animIndex = service.GetConsecutiveWins();
        _animIndex = Mathf.Min(3, _animIndex);
        txtTimeBonus.gameObject.SetActive(_animIndex > 0);
        if (_animIndex == 0)
        {
            // _image.gameObject.SetActive(false);
            boxAnim.gameObject.SetActive(true);
            boxAnim.AnimationState.ClearTracks();
            boxAnim.AnimationState.SetAnimation(0, $"Box_{1}_{UIGiftBoxAnim.State.Close_Idle}", true);
            ;
        }
        else
        {
            // _image.gameObject.SetActive(false);
            boxAnim.gameObject.SetActive(true);
            boxAnim.AnimationState.ClearTracks();
            boxAnim.AnimationState.SetAnimation(0, $"Box_{_animIndex}_{UIGiftBoxAnim.State.Idle}", true);
            txtTimeBonus.text = $"+{service.Config.GetTimeToAdd(_animIndex)}s";
        }
    }
}
