using Cysharp.Threading.Tasks;
using Gameplay.LevelData;
using UnityEngine;

namespace MyGame.SkewerJam.Gameplay
{
    public class GameViewport : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Transform topRefPoint;
        [SerializeField] private Transform bottomRefPoint;


        [SerializeField] private Transform waitingGrillsRefPoint;
        [SerializeField] private Transform ordersRefPoint;


        public float MaxX => maxX;
        public float MaxY => maxY;
        public float MinX => minX;
        public float MinY => minY;

        private Bounds gameViewportBounds;
        private float maxX, maxY, minX, minY;
        private Vector3 orgCameraPosition;
        private Vector3 topPosition, bottomPosition, centerPosition;
        private Bounds perfectBounds;
        private float perfectCamSize;
        private float minCamSize;

        public void Init()
        {
            CalculateGameViewportBounds();
            Canvas.ForceUpdateCanvases();

            orgCameraPosition = mainCamera.transform.position;

            topPosition = topRefPoint.position;
            bottomPosition = bottomRefPoint.position;

            centerPosition = (topPosition + bottomPosition) / 2f;

            Vector3 boundSize = gameViewportBounds.size;
            boundSize.y = topRefPoint.position.y - bottomRefPoint.position.y;
            boundSize.x -= 0.55f;

            Vector3 center = gameViewportBounds.center;
            center.y = (topPosition.y + bottomPosition.y) / 2;
            perfectBounds = new Bounds(center, boundSize);
            perfectCamSize = mainCamera.orthographicSize;

            float screenRatio = Screen.width / (float)Screen.height;
            if (screenRatio < 9f / 16)
            {
                minCamSize = 12f;
            }
            else
            {
                minCamSize = 10f;
            }
        }

        public void CalculateGameViewportBounds()
        {
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

        public async UniTask CalculateViewport()
        {
            Bounds currentBound = new Bounds();

            var gameLogicHandler = MyGame.SkewerJam.Gameplay.GameController.Instance.GameLogicHandler;
            var levelGenerator = MyGame.SkewerJam.Gameplay.GameController.Instance.LevelGenerator;

            var grillManager = gameLogicHandler.GrillManager;
            var primaryGrills = grillManager.ListGrills;
            foreach (var grill in primaryGrills)
            {
                if (levelGenerator.IsStaticGrill(grill.id))
                {
                    currentBound.Encapsulate(grill.GetBounds());
                    if (grill.HasSubGrills())
                    {
                        currentBound.Encapsulate(grill.GetSubGrillPosition());
                    }
                    // else if (dropMode)
                    // {
                    //     currentBound.Encapsulate(grill.transform.position + Vector3.down * 0.5f);
                    //     currentBound.Encapsulate(grill.transform.position + Vector3.up * 1.5f);
                    // }
                }
            }

            if (levelGenerator.LevelData.conveyorData != null)
            {
                foreach (var conveyorData in levelGenerator.LevelData.conveyorData)
                {
                    Vector3 conveyorPos = conveyorData.position.ToVector3();
                    Vector3 conveyorSize = new Vector3(3f, 4f, 0);
                    if (conveyorData.moveType == MoveType.Vertical) conveyorSize.y = 19f;
                    Bounds conveyorBounds = new Bounds(conveyorPos, conveyorSize);
                    currentBound.Encapsulate(conveyorBounds);
                }
            }

            mainCamera.transform.position = orgCameraPosition;

            float widthScale = currentBound.size.x / perfectBounds.size.x;
            float heightScale = currentBound.size.y / perfectBounds.size.y;
            float scale = Mathf.Max(widthScale, heightScale);
            mainCamera.orthographicSize = Mathf.Clamp(perfectCamSize * scale, minCamSize, 17.5f);

            Canvas.ForceUpdateCanvases();

            await UniTask.DelayFrame(1);
            CalculateGameViewportBounds();

            topPosition = topRefPoint.position;
            bottomPosition = bottomRefPoint.position;
            centerPosition = new Vector3(0, (topPosition.y + bottomPosition.y) / 2f, 0);

            Vector3 offset = centerPosition - currentBound.center;
            offset.z = 0;
            Vector3 newPos = orgCameraPosition - offset;
            mainCamera.transform.position = newPos;

            Canvas.ForceUpdateCanvases();

            await UniTask.DelayFrame(1);

            CalculateObjects();
        }

        public void CalculateObjects()
        {
            var gameLogicHandler = MyGame.SkewerJam.Gameplay.GameController.Instance.GameLogicHandler;
            var waitingGrillManager = gameLogicHandler.WaitingGrillManager;
            var orderManager = gameLogicHandler.OrderManager;


            var waitingGrillPos = waitingGrillsRefPoint.position;
            waitingGrillPos.z = 0;
            waitingGrillManager.transform.position = waitingGrillPos;

            var orderPos = ordersRefPoint.position;
            orderPos.z = 0;
            orderManager.transform.position = orderPos;

            // tính hiển thị của waitingGrillManager và orderManager
            float screenRatio = Screen.width / (float)Screen.height;
            if (screenRatio < 9f / 16)
            {

            }
            else
            {
                waitingGrillManager.transform.localScale = Vector3.one * mainCamera.orthographicSize / perfectCamSize;
                orderManager.transform.localScale = Vector3.one * mainCamera.orthographicSize / perfectCamSize;
            }

        }
    }
}