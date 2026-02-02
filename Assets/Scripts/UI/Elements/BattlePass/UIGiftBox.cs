using SonatFramework.Systems.InventoryManagement.GameResources;
using UnityEngine;

namespace GrillSort.BattlePass
{
    public class UIGiftBox : MonoBehaviour
    {
        [SerializeField] private UIBubbleReward bubbleReward;

        public void SetData(RewardData reward)
        {            

            bubbleReward.SetReward(reward);
            bubbleReward.Unselect();
        }
    }
}
