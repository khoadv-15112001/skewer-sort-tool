using Sonat.Enums;
using SonatFramework.Scripts.UIModule.SpriteService;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems;
using TMPro;
using UnityEngine;

namespace SonatFramework.Scripts.UIModule.UIElements
{
    public class UIRewardItem : MonoBehaviour
    {
        [SerializeField] private FixedImageRatio icon;
        [SerializeField] private TMP_Text txtQuantity;
        [SerializeField] private bool customIcon;
        [SerializeField] private string quantityFormat = "x{0}";
        [SerializeField] private string iconNameFormat = "ico_{0}";
        public GameResource Resource { get; private set; }
        public int Quantity { get; private set; }

        public void Init(GameResource resource, int quantity)
        {
            this.Resource = resource;
            this.Quantity = quantity;

            if (!customIcon && icon != null)
            {
                string name = resource.ToString();
                var atlas = Service<SpriteAtlasService>.Get();

                string jpName = $"{string.Format(iconNameFormat, name)}_jp";
                string defName = string.Format(iconNameFormat, name);

                var sprite = resource.HasLocalizeJapan() ? atlas.GetSprite(jpName) : null;
                if (sprite == null)
                {
                    if (resource.HasLocalizeJapan())
                        Debug.LogError($"Missing JP sprite: {jpName}, fallback to {defName}");
                    sprite = atlas.GetSprite(defName);
                }

                if (sprite == null)
                    Debug.LogError($"Missing default sprite: {defName}");

                icon.sprite = sprite;
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
    }
}