using System;
using System.Collections;
using System.Collections.Generic;
using Sonat.Enums;
using UnityEngine;

public class UIReceiveCollectEffect : MonoBehaviour
{
    private bool scaleUp;
    private Coroutine collectAnim;
    public float scaleSpeed = 3f;
    public float scaleMax = 1.15f;
    [SerializeField] private Transform target;
    [SerializeField] protected ParticleSystem blastEffect;
    [SerializeField] protected AudioId collectSound;

    private void OnEnable()
    {
        target.localScale = Vector3.one;
        scaleUp = false;
        collectAnim = null;
    }

    public void PlayCollectEffect()
    {
        if (!gameObject.activeInHierarchy) return;
        if (collectAnim != null)
        {
            //StopCoroutine(collectAnim);
            scaleUp = true;
            //blastEffect?.Play();
        }
        else
        {
            collectAnim = StartCoroutine(CollectEffect());
            //blastEffect?.gameObject.SetActive(true);
        }

        if (blastEffect)
        {
            blastEffect.Play();
        }

        if (collectSound != AudioId.None)
            MySonatFramework.audioService.PlaySound(collectSound);
    }

    IEnumerator CollectEffect()
    {
        scaleUp = true;
        while (target.localScale.x < scaleMax)
        {
            target.localScale += Vector3.one * Time.deltaTime * scaleSpeed;
            yield return null;
        }

        //SettingManager.Vibrations(100);
        yield return null;
        scaleUp = false;

        while (target.localScale.x > 1)
        {
            if (scaleUp)
            {
                collectAnim = StartCoroutine(CollectEffect());
                yield break;
            }

            target.localScale -= Vector3.one * Time.deltaTime * scaleSpeed;
            yield return null;
        }

        target.localScale = Vector3.one;
        collectAnim = null;
    }
}