using SonatFramework.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Entities.Orders
{
    public class UIOrderListView : MonoBehaviour
    {
        [SerializeField] private Transform container;
        [SerializeField] private TMP_Text txtTime;
        [SerializeField] private Slider timeSlider;
 
        public void SetData(OrderList.Data data)
        {
            MySonatFramework.poolingContainer.CleanContainer(container);
            foreach (var orderItem in data.orderItems)
            {
                var item = MySonatFramework.poolingContainer.CreateObject<UIOrderItemView>(container);
                item.SetData(orderItem);
            }
            
            timeSlider.value = data.timeRemaining / data.maxTime;
            txtTime.text = SonatUtils.FormatTimeFromSec((int)data.timeRemaining);
        }
    }
}