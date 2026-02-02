using UnityEngine;
using UnityEngine.UI;

namespace MyGame.Modules.CardCollection
{
    public class AlbumBackground : MonoBehaviour
    {
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image borderImage;
        [SerializeField] private Image exitImage;

        public void Setup(AlbumConfig albumConfig)
        {
            backgroundImage.sprite = albumConfig.albumBackground;
            borderImage.sprite = albumConfig.albumBorder;
            exitImage.sprite = albumConfig.albumExit;
        }
    }
}