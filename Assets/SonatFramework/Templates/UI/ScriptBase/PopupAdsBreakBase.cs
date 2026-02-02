using Spine.Unity;
using System.Collections;
using SonatFramework.Scripts.UIModule;
using TMPro;
using UnityEngine;

public class PopupAdsBreakBase : Panel
{
    [SerializeField] protected TextMeshProUGUI coinValue;
    //[SerializeField] private SkeletonGraphic charAnim;

    protected Coroutine checkTimeout;
    protected float TIME_OUT = 5f;

    public override void OnSetup()
    {
        base.OnSetup();
    }

    public override void Open(UIData uiData)
    {
        base.Open(uiData);

        //coinValue.text = "+ " + AdsBreakService.coin;

        //int rand = Random.Range(1, 3);

        //charAnim.AnimationState.SetAnimation(0, $"Ads{rand}", false);
        //charAnim.AnimationState.AddAnimation(0, "Idle_Happy", true, 0);

        checkTimeout = StartCoroutine(IeStartCheckTimeout());
    }

    public override void OnOpenCompleted()
    {
        base.OnOpenCompleted();
    }

    public override void Close()
    {
        base.Close();

        if (checkTimeout != null)
            StopCoroutine(checkTimeout);
    }

    protected override void OnCloseCompleted()
    {
        base.OnCloseCompleted();
    }

    protected IEnumerator IeStartCheckTimeout()
    {
        yield return new WaitForSeconds(TIME_OUT);
        Close();
    }
}