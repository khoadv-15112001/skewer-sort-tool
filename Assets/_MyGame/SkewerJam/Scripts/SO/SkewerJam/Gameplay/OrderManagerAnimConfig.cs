using System;
using UnityEngine;

namespace MyGame.SkewerJam.Gameplay
{
    [CreateAssetMenu(fileName = "OrderManagerAnimConfigSO", menuName = "MyGame/SkewerJam/Config/OrderManagerAnimConfigSO")]
    public class OrderEntityConfigSO : ScriptableObject
    {
        public float delayAppearNextOrder;

        public float up;

        public float durationDown;
        public float durationUp;
        public float delayMoveOut;

        public AnimationCurve downCurve = AnimationCurve.Linear(0, 0, 1, 1);
        public AnimationCurve upCurve = AnimationCurve.Linear(0, 0, 1, 1);
    }
}