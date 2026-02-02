using MyGame.SkewerJam.Scripts.Service;
using Sonat.CustomService;
using UnityEngine;

namespace MyGame.SkewerJam.Scripts
{
    public class ButtonTracking : MonoBehaviour
    {
        public void OnClick()
        {
            MySonatFramework.GetService<HLWEventService>().JoinEvent();
        }
    }
}
