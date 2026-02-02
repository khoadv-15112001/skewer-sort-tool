using System;
using System.Collections;
using System.Collections.Generic;
using Gameplay.Entities.Obstacle;
using Gameplay.LevelData;
using Helper;
using Tool;
using UnityEngine;
using Random = UnityEngine.Random;

public class ToolManager : SingletonSimple<ToolManager>
{
    [SerializeField] private ToolGrill[] toolGrillsPrefabs;
    [SerializeField] private ToolObstacleBase[] toolObstaclesPrefabs;
    [SerializeField] private GameObject dropColumnPrefab;

    [SerializeField] private GameObject horizontalConveyorPrefab;
    [SerializeField] private GameObject verticalConveyorPrefab;

    public Camera mainCamera;

    public static bool selectAvailable = true;
    private List<ToolGrill> allGrills = new List<ToolGrill>();
    private List<ToolConveyor> allConveyors = new();
    private List<ToolDropColumn> allDropColumns = new();
    public List<ToolGrill> AllGrills => allGrills;
    private List<ToolObstacleBase> obstacles = new List<ToolObstacleBase>();

    public float gridSnap = 0.2f;
    public float spaceX, spaceY;
    public UndoController undoController;
    public Action<ToolGrill> OnGrillChanged;
    public static int currentLayer = 0;
    public ToolGrillSelector toolGrillSelector;
    private Dictionary<string, ToolGrill> toolGrills = new();
    private Dictionary<ObstacleType, ToolObstacleBase> toolObstacles = new();

    public static int numberSlotMax = 3;

    // Start is called before the first frame update
    void Start()
    {
        toolGrillSelector = FindObjectOfType<ToolGrillSelector>();
        toolGrills = new();
        foreach (var toolGrill in toolGrillsPrefabs)
        {
            toolGrills.Add($"{toolGrill.grillType}_{toolGrill.NumberSlot}", toolGrill);
        }

        foreach (var toolObstacle in toolObstaclesPrefabs)
        {
            toolObstacles.Add(toolObstacle.obstacleType, toolObstacle);
        }
    }

    public ToolGrill CreateToolGrill(GrillType grillType, int numberSlot)
    {
        var pref = toolGrills[$"{grillType.ValidateGrillType()}_{numberSlot}"];
        GameObject toolGrillObject = Instantiate(pref.gameObject, Random.insideUnitCircle, Quaternion.identity);
        ToolGrill toolGrill = toolGrillObject.GetComponent<ToolGrill>();
        allGrills.Add(toolGrill);

        if (numberSlot > numberSlotMax) numberSlotMax = numberSlot;
        return toolGrill;
    }


    public ToolConveyor CreateToolConveyor(MoveType moveType)
    {
        var pref = moveType == MoveType.Horizontal ? horizontalConveyorPrefab : verticalConveyorPrefab;
        GameObject toolConveyorObject = Instantiate(pref, Vector3.zero, Quaternion.identity);
        ToolConveyor toolConveyor = toolConveyorObject.GetComponent<ToolConveyor>();
        allConveyors.Add(toolConveyor);
        return toolConveyor;
    }

    public ToolObstacleBase CreateToolObstacle(ObstacleType obstacleType)
    {
        var pref = toolObstacles[obstacleType];
        GameObject toolObstacleObject = Instantiate(pref.gameObject, Random.insideUnitCircle, Quaternion.identity);
        ToolObstacleBase toolObstacle = toolObstacleObject.GetComponent<ToolObstacleBase>();
        obstacles.Add(toolObstacle);
        return toolObstacle;
    }

    public byte GetAvailableGrillId()
    {
        for (byte i = 0; i < allGrills.Count + 50; i++)
        {
            if (!allGrills.Exists(e => e.GrillData != null && e.GrillData.id == i)) return i;
        }

        return (byte)(allGrills.Count + 1);
    }

    public byte GetAvailableConveyorId()
    {
        for (byte i = 0; i < allConveyors.Count + 5; i++)
        {
            if (!allConveyors.Exists(e => e.conveyData != null && e.conveyData.id == i)) return i;
        }

        return (byte)(allConveyors.Count + 1);
    }

    public void RemoveToolGrill(ToolGrill toolGrill)
    {
        if (toolGrill == null) return;
        allGrills.Remove(toolGrill);
        Destroy(toolGrill.gameObject);
        OnGrillChanged?.Invoke(toolGrill);
    }

    public void RemoveToolConveyor(ToolConveyor toolConveyor)
    {
        allConveyors.Remove(toolConveyor);
        Destroy(toolConveyor.gameObject);
    }

    public void RemoveToolDropColumn(ToolDropColumn toolDropColumn)
    {
        allDropColumns.Remove(toolDropColumn);
        Destroy(toolDropColumn.gameObject);
    }

    public void ValidateAllConveyors()
    {
        foreach (var conveyor in allConveyors)
        {
            conveyor.CheckGrill();
        }
    }

    public void ValidateAllObstacles()
    {
        ValidateAllConveyors();
        foreach (var obstacle in obstacles)
        {
            obstacle.ValidateObstacle();
        }
    }

    public ToolConveyor GetConveyor(ConveyorType conveyorType)
    {
        if (allConveyors == null) return null;
        foreach (var conveyor in allConveyors)
        {
            if (conveyor.conveyData.conveyorType == conveyorType) return conveyor;
        }

        return null;
    }

    public ToolGrill GetToolGrill(int id)
    {
        return allGrills.Find(e => e.GrillData.id == id);
    }

    public List<ToolGrill> GetToolGrills(List<int> ids)
    {
        List<ToolGrill> toolGrills = new List<ToolGrill>();
        foreach (var grill in allGrills)
        {
            if (ids.Contains(grill.GrillData.id)) toolGrills.Add(grill);
        }

        return toolGrills;
    }

    // public ToolObstacleBase CreateObstacle(ObstacleType type, byte toolGrillId)
    // {
    //     ToolGrill toolGrill = allGrills.Find(e => e.GrillData.id == toolGrillId);
    //     return CreateObstacle(type, toolGrill);
    // }

    // public ToolObstacleBase CreateObstacle(ObstacleType type, ToolGrill toolGrill)
    // {
    //     switch (type)
    //     {
    //         case ObstacleType.Lock:
    //             ToolLockObstacle obstacleBase = Instantiate(iceObstaclePrefab, toolGrill.transform).GetComponent<ToolLockObstacle>();
    //             obstacleBase.transform.localPosition = Vector3.zero;
    //             return obstacleBase;
    //     }
    //
    //     return null;
    // }


    public ToolObstacleBase GetObstacle(byte id)
    {
        var obstacle = obstacles.Find(x => x.toolGrills.Find(y => y.GrillData.id == id) != null);
        if (obstacle != null)
        {
            return obstacle;
        }

        return null;
    }

    public void RemoveObstacle(ToolObstacleBase toolObstacle)
    {
        obstacles.Remove(toolObstacle);
        Destroy(toolObstacle.gameObject);
    }

    public void ClearTable()
    {
        foreach (var toolGrill in allGrills)
        {
            Destroy(toolGrill.gameObject);
        }

        foreach (var obstacle in obstacles)
        {
            Destroy(obstacle.gameObject);
        }

        foreach (var conveuor in allConveyors)
        {
            Destroy(conveuor.gameObject);
        }

        foreach (var dropColumn in allDropColumns)
        {
            Destroy(dropColumn.gameObject);
        }

        allGrills.Clear();
        obstacles.Clear();
        allConveyors.Clear();
        allDropColumns.Clear();
        currentLayer = 0;
        undoController.ClearUndo();
        numberSlotMax = 3;
    }

    private List<ToolGrill> selectedGrill = new List<ToolGrill>();

    public void SelectMultipleGrills(Vector2 start, Vector2 end)
    {
        selectedGrill.Clear();
        Rect selectionRect = new Rect(start, end - start);


        foreach (var grill in allGrills)
        {
            // Convert the object's position to screen coordinates
            Vector2 screenPos = mainCamera.WorldToScreenPoint(grill.transform.position);
            // Check if the object is within the selection rectangle
            if (selectionRect.Contains(screenPos))
            {
                selectedGrill.Add(grill);
            }
        }

        // Log the selected objects
        foreach (var obj in selectedGrill)
        {
            Debug.Log("Selected: " + obj.name);
        }

        DrawBox();
    }

    [SerializeField] private LineRenderer lineRenderer;
    private Bounds curBoundChooseObjects;

    public void DrawBox()
    {
        if (selectedGrill.Count == 0)
        {
            lineRenderer.enabled = false;
            return;
        }

        Collider2D boxCollider;
        var ld = new Vector3(10000, 10000, 0);
        var ru = new Vector3(-10000, -10000, 0);

        foreach (ToolGrill chooseObject in selectedGrill)
        {
            boxCollider = chooseObject.GetComponent<Collider2D>();
            ld.x = Mathf.Min(ld.x, boxCollider.bounds.min.x);
            ld.y = Mathf.Min(ld.y, boxCollider.bounds.min.y);
            ru.x = Mathf.Max(ru.x, boxCollider.bounds.max.x);
            ru.y = Mathf.Max(ru.y, boxCollider.bounds.max.y);
        }

        Vector3 center = new Vector3((ld.x + ru.x) / 2, (ld.y + ru.y) / 2, 0);
        Vector3 size = new Vector3(ru.x - ld.x, ru.y - ld.y, 1);

        curBoundChooseObjects = new Bounds(center, size);

        if (ld == new Vector3(10000, 10000, 0) && ru == new Vector3(-10000, -10000, 0))
        {
            lineRenderer.enabled = false;
        }
        else
        {
            lineRenderer.enabled = true;
        }

        lineRenderer.SetPosition(0, ld);
        lineRenderer.SetPosition(1, new Vector3(ld.x, ru.y, 0f));
        lineRenderer.SetPosition(2, ru);
        lineRenderer.SetPosition(3, new Vector3(ru.x, ld.y, 0f));

        lineRenderer.gameObject.transform.position = new Vector3((ld.x + ru.x) / 2, (ld.y + ru.y) / 2, 2);

        //displayBoxCollider.size = new Vector2(ru.x - ld.x + 0.3f, ru.y - ld.y + 0.3f);
    }


    public void CreateTableWithSize(int x, int y, LevelData levelData)
    {
        ClearTable();
        float offsetX = (x - 1) * spaceX / 2;
        float offsetY = (y - 1) * spaceY / 2;
        for (int i = 0; i < y; i++)
        {
            for (int j = 0; j < x; j++)
            {
                var toolGrill = CreateToolGrill(GrillType.Normal, 3);
                toolGrill.transform.position = new Vector3(j * spaceX, i * spaceY, 0) - new Vector3(offsetX, offsetY, 0);
                levelData.grillData ??= new();

                GrillData grillData = new GrillData();
                grillData.id = GetAvailableGrillId();
                grillData.position = new Vector3Data(toolGrill.transform.position);
                levelData.grillData.Add(grillData);
                toolGrill.SetData(grillData);
            }
        }
    }

    public List<ToolGrill> GetGrillsHasItem(int id)
    {
        List<ToolGrill> grills = new List<ToolGrill>();
        foreach (var toolGrill in allGrills)
        {
            if (toolGrill.GrillData.layer != null)
            {
                foreach (var layer in toolGrill.GrillData.layer)
                {
                    bool hasItem = false;
                    foreach (var itemData in layer.itemData)
                    {
                        if (itemData != null && itemData.id == id)
                        {
                            grills.Add(toolGrill);
                            hasItem = true;
                            break;
                        }
                    }

                    if (hasItem) break;
                }
            }
        }

        return grills;
    }

    public void SetMainLayer(int layer)
    {
        currentLayer = layer;
        foreach (var toolGrill in allGrills)
        {
            toolGrill.UpdateLayerMain();
        }
    }

    private void Update()
    {
        if (toolGrillSelector.IsPointerOverUI()) return;
        if (Input.mouseScrollDelta.y != 0)
        {
            mainCamera.orthographicSize -= Input.mouseScrollDelta.y * Time.deltaTime * 30;
        }

        if (Input.GetMouseButton(2))
        {
            float Camx = -Input.GetAxis("Mouse X");
            float Camy = -Input.GetAxis("Mouse Y");
            if (Camx != 0 || Camy != 0)
                mainCamera.transform.position += new Vector3(Camx, Camy, 0);
        }

#if UNITY_EDITOR
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                undoController.Undo();
            }
        }
#elif UNITY_STANDALONE
         if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                undoController.Undo();
            }
        }
#endif
    }


    public void ChangeMainGrillVisual(Sprite sprite, bool mini = false)
    {
        foreach (var toolGrill in allGrills)
        {
            if (toolGrill.GrillData.grillType == GrillType.Normal == mini) continue;
            toolGrill.ChangeMainVisual(sprite);
        }
    }

    public void ChangeSubGrillVisual(Sprite sprite)
    {
        foreach (var toolGrill in allGrills)
        {
            toolGrill.ChangSubVisual(sprite);
        }
    }

    public void SwitchItemId(int oldId, int newId)
    {
        foreach (var grill in allGrills)
        {
            grill.SwitchItemId(oldId, newId);
        }

        UIToolPanel.Instance.UpdateItemCount();
    }

    public ToolDropColumn CreateToolDropColumn()
    {
        GameObject toolDropColumnObject = Instantiate(dropColumnPrefab, Random.insideUnitCircle, Quaternion.identity);
        ToolDropColumn toolDropColumn = toolDropColumnObject.GetComponent<ToolDropColumn>();
        allDropColumns.Add(toolDropColumn);
        return toolDropColumn;
    }
}