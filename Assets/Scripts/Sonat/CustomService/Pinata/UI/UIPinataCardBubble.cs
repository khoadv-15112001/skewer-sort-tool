using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrillSort.Pinata
{
    public class UIPinataCardBubble : MonoBehaviour
    {
        [SerializeField] private GameObject[] stars;

        public void BindData(int star)
        {
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].SetActive(i + 1 <= star);
            }
        }
    }
}