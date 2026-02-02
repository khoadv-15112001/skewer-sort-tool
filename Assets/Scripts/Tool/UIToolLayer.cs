using System.Collections.Generic;
using Gameplay.LevelData;
using UnityEngine;
using UnityEngine.UI;

namespace Tool
{
    public class UIToolLayer: MonoBehaviour
    {
        [SerializeField] private Transform itemContainer;
        [SerializeField] private Image bgr;
        [SerializeField] private Color[] colors;
        private LayerData layerData;
        //private List<UIToolItem> uiToolItems;
        private UIToolGrill uiToolGrill;

        public void Setup(LayerData layerData, UIToolGrill uiToolGrill)
        {
            this.layerData = layerData;
            this.uiToolGrill = uiToolGrill;
            MySonatFramework.poolingContainer.CleanContainer(itemContainer);
            for (int i = 0; i < layerData.itemData.Length; i++)
            {
                var item = MySonatFramework.poolingContainer.CreateObject<UIToolItem>(itemContainer);
                item.Setup(i, layerData.itemData[i], this);
                //uiToolItems.Add(item);
            }
        }

        public void OnItemDataUpdated(int index, ItemData itemData)
        {
            layerData.itemData[index] = itemData;
            UIToolPanel.Instance.UpdateGrill();
        }

        public void RemoveLayer()
        {
            uiToolGrill.RemoveLayer(layerData);
            gameObject.SetActive(false);
        }

        public void SetMainLayer(bool isMain)
        {
            bgr.color = isMain ? colors[1] : colors[0];
        }
    }
}