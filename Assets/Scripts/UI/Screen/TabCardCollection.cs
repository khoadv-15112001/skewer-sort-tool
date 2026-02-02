using System;
using System.Collections.Generic;
using MyGame.Modules.CardCollection;
using SonatFramework.Systems;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;

public class TabCardCollection : UITabBase
{
    [Space]
    [Header("Setup Albums")]
    [SerializeField] private Transform container;
    private readonly Service<PoolingContainerService> _poolingService = new();
    private readonly Service<CardCollectionService> _cardCollectionService = new();
    private List<UIAlbum> albums = new();
    protected override void Start()
    {
        base.Start();
        _poolingService.Instance.CleanContainer(container);
        foreach (var album in _cardCollectionService.Instance.config.albums)
        {
            var albumObj = _poolingService.Instance.CreateObject<UIAlbum>(container);
            albumObj.Setup(album.type);
            albums.Add(albumObj);
        }

        OnShow();
    }

    public override void OnShow()
    {
        base.OnShow();
        foreach (var albumObj in albums)
        {
            albumObj.UpdateData();
        }
    }

    public override void OnHide()
    {
        base.OnHide();
        // foreach (var albumObj in albums)
        // {
        //     albumObj.OnLoseFocus();
        // }
    }
}