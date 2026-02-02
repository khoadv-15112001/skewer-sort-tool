using Cysharp.Threading.Tasks;
using Manager;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Entities.Orders
{
    public class UIOrderItemView : MonoBehaviour
    {
        [SerializeField] private FixedImageRatio icon;
        [SerializeField] private Image bg;
        [SerializeField] private Sprite[] bgSprites;
        [SerializeField] private GameObject completeObj;
        [SerializeField] private GameObject missedObj;
        [SerializeField] private GameObject spicyObj;

        public void SetData(OrderItem.Data data)
        {
            icon.SetSpriteAsync(PathManager.ItemSprite(data.id)).Forget();
            bg.sprite = bgSprites[data.isCompleted ? 1 : data.spicy ? 2 : 0];
            completeObj.SetActive(data.isCompleted);
            if (missedObj)
                missedObj.SetActive(!data.isCompleted);
            spicyObj.SetActive(data.spicy);
        }
    }
}