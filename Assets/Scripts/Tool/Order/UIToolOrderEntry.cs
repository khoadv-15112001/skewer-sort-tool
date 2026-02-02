using Gameplay.LevelData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tool.Order
{
    public class UIToolOrderEntry: MonoBehaviour
    {
        [SerializeField] private UIToolOrderItem toolOrderItem;
        [SerializeField] private TMP_InputField inputLayer;
        [SerializeField] private TMP_InputField inputId;
        [SerializeField] private Toggle spicyToggle;
        private OrderItemData orderItemData;

        public void SetData(OrderItemData data)
        {
            orderItemData = data;
            
            inputLayer.onEndEdit.RemoveAllListeners();
            inputId.onEndEdit.RemoveAllListeners();
            spicyToggle.onValueChanged.RemoveAllListeners();
            
            inputLayer.text = orderItemData.layer.ToString();
            inputId.text = orderItemData.id.ToString();
            spicyToggle.isOn = orderItemData.spicy;
            
            inputLayer.onEndEdit.AddListener(OnEditLayer);
            inputId.onEndEdit.AddListener(OnEditId);
            spicyToggle.onValueChanged.AddListener(OnEditSpicy);
        }

        private void OnEditLayer(string newLayer)
        {
            if (int.TryParse(newLayer, out int layer))
            {
                orderItemData.layer = layer;
            }
        }
        
        private void OnEditId(string newId)
        {
            if (int.TryParse(newId, out int id))
            {
                orderItemData.id = id;
            }
        }

        private void OnEditSpicy(bool newSpicy)
        {
            orderItemData.spicy = newSpicy;
        }

        public void RemoveEntry()
        {
            toolOrderItem.RemoveOrderEntry(orderItemData);
            gameObject.SetActive(false);
        }
    }
}