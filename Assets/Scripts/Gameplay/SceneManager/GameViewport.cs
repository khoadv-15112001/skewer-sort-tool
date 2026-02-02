using UnityEngine;

namespace Gameplay.SceneManager
{
    public class GameViewport
    {
        public Bounds gameViewportBounds;
        private Camera mainCamera;
        public float maxX, maxY, minX, minY;

        public void CalculateGameViewportBounds()
        {
            mainCamera = GameplayController.instance.mainCamera;
            Vector2 viewportTopRight = mainCamera.ScreenToWorldPoint(new Vector2(mainCamera.pixelWidth, mainCamera.pixelHeight));
            Vector2 viewportBottomLeft = mainCamera.ScreenToWorldPoint(Vector2.zero);
            Vector2 size = viewportTopRight - viewportBottomLeft;
            Vector2 center = (viewportBottomLeft + viewportTopRight) / 2f;
            gameViewportBounds = new Bounds(center, size);

            maxX = viewportTopRight.x;
            maxY = viewportTopRight.y;
            minX = viewportBottomLeft.x;
            minY = viewportBottomLeft.y;
        }
    }
}