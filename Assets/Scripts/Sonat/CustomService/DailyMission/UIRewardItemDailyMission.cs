using Sonat.Enums;
using SonatFramework.Scripts.UIModule.SpriteService;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GrillSort.DailyMission
{
    public class UIRewardItemDailyMission : MonoBehaviour
    {
        [SerializeField] private FixedImageRatio icon;
        [SerializeField] private TMP_Text txtQuantity;
        [SerializeField] private List<CustomIcon> customIcons = new();
        [SerializeField] private string quantityFormat = "x{0}";
        [SerializeField] private string iconNameFormat = "ico_{0}";
        public GameResource Resource { get; private set; }
        public int Quantity { get; private set; }

        public void Init(GameResource resource, int quantity)
        {
            Resource = resource;
            Quantity = quantity;

            if (icon != null)
            {
                var customIcon = customIcons.Find(x => x.resource == resource);

                if (customIcon != null)
                {
                    icon.sprite = customIcon.sprite;
                }
                else
                {
                    icon.sprite = Service<SpriteAtlasService>.Get().GetSprite(string.Format(iconNameFormat, resource));
                }
            }

            SetQuantity(quantity);
        }

        public void SetQuantity(int quantity)
        {
            switch (Resource)
            {
                case GameResource.Lives:
                    string quantityString = "";
                    if (quantity < 3600) quantityString = $"{quantity / 60}m";
                    else
                    {
                        quantityString = $"{quantity * 1.0f / 3600}h";
                    }
                    txtQuantity.text = quantityString;
                    break;
                default:
                    txtQuantity.text = string.Format(quantityFormat, quantity);
                    break;
            }
        }

        [Serializable]
        public class CustomIcon
        {
            public GameResource resource;
            public Sprite sprite;
        }
    }
}