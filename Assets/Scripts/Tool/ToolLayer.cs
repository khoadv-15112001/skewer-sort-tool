using System.Collections;
using System.Collections.Generic;
using Gameplay.LevelData;
using Tool;
using UnityEngine;

public class ToolLayer : MonoBehaviour
{
        [SerializeField] private ToolItem[] toolItems;
        private LayerData layerData;
        private ToolGrill grill;

        public void SetData(LayerData layerData, ToolGrill grill)
        {
            this.layerData = layerData;
            this.grill = grill;
            UpdateVisual();

        }

        public void OnSelectLayer()
        {
            //UIToolPanel.Instance.OnSelectToolGrill(this);
        }

        public void OnLayerUpdate()
        {
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (layerData == null)
            {
                for (int i = 0; i < toolItems.Length; i++)
                {
                    toolItems[i].SetItemData(null);
                }
            }
            else
            {
                for (int i = 0; i < toolItems.Length; i++)
                {
                    toolItems[i].SetItemData(layerData.itemData[i]);
                }
            }
        }
}
