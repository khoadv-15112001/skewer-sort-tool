using Cysharp.Threading.Tasks;
using DG.Tweening;
using MyGame.SkewerJam.Gameplay;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using Spine;
using Spine.Unity;
using System;
using UnityEngine;
using Sirenix.OdinInspector;
public class PopupPreWin_SkewerJam : PopupPreWin
{
    [SerializeField] private SkeletonGraphic animationBack;
    [SerializeField] private SkeletonGraphic animationFront;
    [SerializeField] private ParticleSystem psSpawn;

    public override void Open(UIData uiData)
    {
        base.Open(uiData);

        var num = GameController.Instance.GameLogicHandler.Pumpkin;

        MySonatFramework.audioService.PlaySound(AudioId.Pre_win_HLW_sound_Grill_sort);
        PlayAppear();
    }
  
    [Button("Play Animation")]
    private void PlayAppear()
    {
        var back = animationBack.AnimationState.SetAnimation(0, "Appear", false);
        var front = animationFront.AnimationState.SetAnimation(0, "Appear", false);

        back.Complete += _ => PlayDrop();
    }

    private void PlayDrop()
    {
        psSpawn.gameObject.SetActive(true);
        psSpawn.Play();

        var back = animationBack.AnimationState.SetAnimation(0, "Drop", true);
        var front = animationFront.AnimationState.SetAnimation(0, "Drop", true);

        back.Complete += _ => PlayCollectDone();
    }

    private void PlayCollectDone()
    {
        psSpawn.Stop();
        var back = animationBack.AnimationState.SetAnimation(0, "Collect_Done2", false);
        var front = animationFront.AnimationState.SetAnimation(0, "Collect_Done2", false);

        back.Complete += _ => PlayOut();
    }

    private void PlayOut()
    {
        var back = animationBack.AnimationState.SetAnimation(0, "Out", false);
        var front = animationFront.AnimationState.SetAnimation(0, "Out", false);
    }

}
