using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GrillSort.Rail
{
    public class UIRailMilestoneTitleController : MonoBehaviour
    {
        private List<UIRailMilestoneTitle> uIRailMilestoneTitles = new();

        public void Setup()
        {
            uIRailMilestoneTitles.Clear();
            uIRailMilestoneTitles = GetComponentsInChildren<UIRailMilestoneTitle>(true).ToList();

            for (int i = 0; i < uIRailMilestoneTitles.Count; i++)
            {
                uIRailMilestoneTitles[i].Bind(i + 1);
            }
        }
    }
}