using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrillSort.KitchenMission
{
    public class UIKitchenMissionItemCurrency : MonoBehaviour
    {
        private void OnEnable()
        {
            if (!KitchenMissionService.Instance.isJoined.BoolValue)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
        }
    }
}