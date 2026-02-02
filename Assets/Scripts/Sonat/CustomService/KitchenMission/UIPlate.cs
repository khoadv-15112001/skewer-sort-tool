using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrillSort.KitchenMission
{
    public class UIPlate : MonoBehaviour
    {
        [SerializeField] private GameObject closeImg;
        [SerializeField] private GameObject anim;

        public void SetOpen(bool state)
        {
            closeImg.SetActive(!state);
            anim.SetActive(state);
        }
    }
}