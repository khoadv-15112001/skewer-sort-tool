using System.Collections;
using System.Collections.Generic;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

namespace MyGame.Modules.CardCollection
{
    public class UIBackground : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private Image image;

        private void OnEnable()
        {
            var screenSize = canvas.GetComponent<RectTransform>().sizeDelta;
            var imageSize = image.sprite.rect.size;

            var anchorY = image.rectTransform.anchorMin.y;
            var safeArea = new Vector2(screenSize.x, screenSize.y * (1 - anchorY));
            var maxScale = Mathf.Max(safeArea.x / imageSize.x, safeArea.y / imageSize.y);

            image.transform.localScale = Vector3.one * maxScale;
        }
    }
}
