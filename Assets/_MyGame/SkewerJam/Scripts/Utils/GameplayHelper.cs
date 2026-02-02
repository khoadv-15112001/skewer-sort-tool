using UnityEngine;

namespace MyGame.SkewerJam.Gameplay.Utils
{
    public static class GameplayUtils
    {
        public static void AlignObjects(Transform parent, float distance)
        {
            var num = parent.childCount;
            var startPos = -(num - 1) * distance / 2;

            for (int i = 0; i < parent.childCount; i++)
            {
                var order = parent.GetChild(i);
                var oldPos = order.transform.localPosition;
                var newPosX = startPos + distance * i;
                order.transform.localPosition = new Vector3(newPosX, 0, 0);
            }
        }
    }
}