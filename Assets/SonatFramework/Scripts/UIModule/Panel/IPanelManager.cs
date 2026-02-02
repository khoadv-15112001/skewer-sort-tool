using UnityEngine;

namespace SonatFramework.Scripts.UIModule
{
    public interface IPanelManager
    {
        public T OpenPanelByName<T>(string panelName, UIData uiData = null, Transform container = null) where T : View;

        public T OpenPanel<T>(UIData uiData = null, Transform container = null) where T : View;

        public void ClosePanel<T>(bool immediately = false) where T : View;

        public void ReleasePanel(View panelClosed);
    }
}