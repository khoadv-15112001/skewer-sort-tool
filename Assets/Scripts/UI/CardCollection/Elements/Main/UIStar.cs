using UnityEngine;

namespace MyGame.Modules.CardCollection
{
    public class UIStar : MonoBehaviour
    {
        [SerializeField] private bool isOn = true;
        [SerializeField] private GameObject enableObj;
        [SerializeField] private GameObject disableObj;

        public void SetData(bool isOn)
        {
            this.isOn = isOn;
            enableObj.gameObject.SetActive(isOn);
            disableObj.gameObject.SetActive(!isOn);
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            SetData(isOn);
        }
#endif
    }
}
