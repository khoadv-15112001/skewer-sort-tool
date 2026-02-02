using MyGame.Modules.CardCollection;
using SonatFramework.Scripts.UIModule;
using UnityEngine;

public class PopupAlbum : Panel
{
    [SerializeField] private UIAlbumNavigator albumNavigator;


    public override void Open(UIData uiData)
    {
        base.Open(uiData);

        if (uiData.TryGet<AlbumType>("albumType", out var albumType))
        {
            albumNavigator.Setup(albumType);
        }
    }

    public override void Close()
    {
        base.Close();
        albumNavigator.SeeNewCard();
    }

    public void Refresh()
    {
        albumNavigator.Refresh();
    }
}
