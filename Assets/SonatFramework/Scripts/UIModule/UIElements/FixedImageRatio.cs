using System;
using UnityEngine;
using UnityEngine.UI;

namespace SonatFramework.Scripts.UIModule.UIElements
{
    [RequireComponent(typeof(Image))]
    public class FixedImageRatio : MonoBehaviour
    {
        public enum Type
        {
            None,
            WidthControlHeight,
            HeightControlWidth,
            ForceInRect
        }

        public Type type;

        [HideInInspector] public Image image;

        private Vector2 preferSize;
        private RectTransform rectTransform;

        public Sprite sprite
        {
            set => SetSprite(value);
            get => image.sprite;
        }

        public void Setup()
        {
            if (image == null)
            {
                image = GetComponent<Image>();
                rectTransform = GetComponent<RectTransform>();
                preferSize = rectTransform.sizeDelta;
            }
        }

        private void Awake()
        {
            Setup();
        }

        public void SetSprite(Sprite sprite)
        {
            if (image == null)
                image = GetComponent<Image>();
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
                preferSize = rectTransform.sizeDelta;
            }

            image.color = Color.white;
            image.sprite = sprite;
            if (sprite == null)
            {
                Debug.Log("Sprite is empty!");
                return;
            }

            var navWidth = sprite.rect.width;
            var navHeigh = sprite.rect.height;
            switch (type)
            {
                case Type.WidthControlHeight:
                    var rectW = rectTransform.sizeDelta.x;
                    var newHeigh = rectW * navHeigh / navWidth;
                    rectTransform.sizeDelta = new Vector2(rectW, newHeigh);
                    break;
                case Type.HeightControlWidth:
                    var rectH = rectTransform.sizeDelta.y;
                    var newWidth = rectH * navWidth / navHeigh;
                    rectTransform.sizeDelta = new Vector2(newWidth, rectH);
                    break;
                case Type.ForceInRect:
                    var orgSize = sprite.rect.size;
                    var imageRatio = orgSize.x / orgSize.y;
                    var preferRatio = preferSize.x / preferSize.y;

                    if (preferRatio < imageRatio)
                    {
                        var newHeight2 = preferSize.x / imageRatio;
                        rectTransform.sizeDelta = new Vector2(preferSize.x, newHeight2);
                    }
                    else
                    {
                        var newWidth2 = preferSize.y * imageRatio;
                        rectTransform.sizeDelta = new Vector2(newWidth2, preferSize.y);
                    }

                    break;
            }
        }
    }
}