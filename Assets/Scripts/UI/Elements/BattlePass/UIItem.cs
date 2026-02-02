using UnityEngine;
using UnityEngine.UIElements;

namespace GrillSort.BattlePass
{
    public class UIItem : MonoBehaviour
    {
        [SerializeField] private UITicket ticket;
        [SerializeField] private UIMileStone item;

        public void Init(bool isTicket)
        {
            ticket.gameObject.SetActive(isTicket);
            item.gameObject.SetActive(!isTicket);
        }

        public UIMileStone GetMileStone()
        {
            return item;
        }
    }
}