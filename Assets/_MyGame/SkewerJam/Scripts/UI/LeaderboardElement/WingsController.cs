using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class WingsController : MonoBehaviour
{
    [SerializeField] private SkeletonGraphic[] wingAnims;
    [SerializeField] private string animIdle = "a";
    [SerializeField] private string animFly = "Idle";

    [SerializeField] private float minRangeDelay = 5f;
    [SerializeField] private float maxRangeDelay = 10f;

    private void OnEnable()
    {
        StartCoroutine(PlayAnim());
    }

    private IEnumerator PlayAnim()
    {
        foreach (var wingAnim in wingAnims)
        {
            wingAnim.AnimationState.SetAnimation(0, animIdle, true);
        }

        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minRangeDelay, maxRangeDelay));

            foreach (var wingAnim in wingAnims)
            {
                wingAnim.AnimationState.ClearTrack(0);
                wingAnim.AnimationState.SetAnimation(0, animFly, false).Complete += (track) =>
                {
                    wingAnim.AnimationState.SetAnimation(0, animIdle, true);
                };
            }
        }
    }
}
