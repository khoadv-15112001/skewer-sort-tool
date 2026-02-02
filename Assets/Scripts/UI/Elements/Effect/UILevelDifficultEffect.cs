using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sonat.Enums;
using SonatFramework.Systems;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;

public class UILevelDifficultEffect : MonoBehaviour, IPoolingObject
{
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private GameObject pHard;
    [SerializeField] private GameObject pSuperHard;
    [SerializeField] private CanvasGroup canvasGroup;

    public void Setup(LevelDifficulty levelDifficulty, Transform target, Action action = null)
    {
        pHard.SetActive(levelDifficulty == LevelDifficulty.Hard);
        pSuperHard.SetActive(levelDifficulty == LevelDifficulty.SuperHard);

        transform.DOScale(0f, duration);
        canvasGroup.DOFade(0.5f, duration);
        transform.DOJump(target.position, 1.75f, 1, duration).OnComplete(() =>
        {
            action?.Invoke();
            target.DOScale(1.2f, 0.2f).SetLoops(2, LoopType.Yoyo);
            SonatSystem.GetService<PoolingService>().ReturnObj(this);
        });
    }
    
    public void Setup()
    {
        
    }

    public void OnCreateObj(params object[] args)
    {
        transform.localScale = Vector3.one;
        canvasGroup.alpha = 1;
    }

    public void OnReturnObj()
    {
        
    }
}
