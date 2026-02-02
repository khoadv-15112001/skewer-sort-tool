using UnityEngine;

namespace MyGame.SkewerJam.Objects.Entities
{
    public class WaitingGrillVisual : MonoBehaviour
    {
        [SerializeField] private GameObject activeObject;
        [SerializeField] private GameObject inactiveObject;

        public void SetActive(bool isActive)
        {
            activeObject.SetActive(isActive);
            inactiveObject.SetActive(!isActive);
        }
    }
}