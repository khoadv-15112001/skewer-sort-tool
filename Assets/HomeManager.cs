using Cysharp.Threading.Tasks;
using DG.Tweening;
using MyGame.Modules.CardCollection;
using SonatFramework.Systems;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HomeManager : SingletonSimple<HomeManager>
{
    public UINavigateBarSlide uINavigateBar;

    private readonly Service<CardCollectionService> _cardCollectionService = new();

    private bool running = false;

    private void Awake()
    {
        if (uINavigateBar == null)
            uINavigateBar = GetComponentInChildren<UINavigateBarSlide>();
        //OnCompleteAlbum();
    }

    public async UniTask SwitchTab(Sonat.Enums.NavigationType navigation, float delay = 0)
    {
        await UniTask.WaitForSeconds(delay);

        uINavigateBar.SwitchTab(navigation);
    }

    private void OnEnable()
    {
        _cardCollectionService.Instance.OnCompleteAlbum += OnCompleteAlbum;
        running = false;
    }

    private void OnDisable()
    {
        _cardCollectionService.Instance.OnCompleteAlbum -= OnCompleteAlbum;
        running = false;

    }

    private void OnCompleteAlbum()
    {
        if (running) return;
        running = true;
        RunQueueCompleteAlbum().Forget();
    }

    private async UniTask RunQueueCompleteAlbum()
    {
        await _cardCollectionService.Instance.RunQueueCompleteAlbum();
        running = false;
    }
}
