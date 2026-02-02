using SonatFramework.Scripts.UIModule;
using UnityEngine;

namespace GrillSort.QuestEvent
{
    public class UIToolTip : MonoBehaviour
    {
        public void OnClick()
        {
            PanelManager.Instance.OpenPanel<PopupGoldenSlice>();
        }
    }
}

