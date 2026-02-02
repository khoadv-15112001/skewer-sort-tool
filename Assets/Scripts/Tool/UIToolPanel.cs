using System.Collections.Generic;
using System.Linq;
using Gameplay.LevelData;
using Manager;
using MyGame.SkewerJam.Level;
using Sirenix.Utilities;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Scripts.Utils;
using SonatFramework.Systems.LevelManagement;
using SonatFramework.Systems.SceneManagement;
using SonatFramework.Systems.UserData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LevelData = Gameplay.LevelData.LevelData;
using LevelData_SkewerJam = MyGame.SkewerJam.Level.LevelData_SkewerJam;
using ToolDropColumn = Tool.ToolDropColumn;

namespace Tool
{
    public class UIToolPanel : SingletonSimple<UIToolPanel>
    {
        [SerializeField] private UIToolGrill uiToolGrill;
        [SerializeField] private UIToolConveyor uiToolConveyor;
        [SerializeField] private UIToolOrder uiToolOrder;
        [SerializeField] private UIToolSheetData uiToolSheetData;
        private ToolGrill toolGrill;
        private ToolConveyor toolConveyor;
        private ToolDropColumn toolDropColumn;
        [SerializeField] private SonatLevelService levelService;
        [SerializeField] private LevelService levelServiceFlower;

        private LevelData_SkewerJam levelData = new LevelData_SkewerJam();

        [Header("UI Level Settings")]
        [SerializeField] private DropdownLevelDifficulty levelDifficultyDropdown;
        [SerializeField] private DropdownLevelDifficultyValue levelDifficultyValueDropdown;
        [SerializeField] private DropdownLevelType levelTypeDropdown;
        [SerializeField] private DropdownLevelMode levelModeDropdown;
        [SerializeField] private TMP_InputField txtLevel;
        [SerializeField] private TMP_InputField txtLevelFolder;
        [SerializeField] private TMP_InputField txtTime;
        [SerializeField] private TMP_InputField txtMove;
        [SerializeField] private TMP_InputField txtSnapGrid;
        [SerializeField] private TMP_InputField txtTableX, txtTableY;
        [SerializeField] private TMP_InputField txtRow, txtColumn;
        [SerializeField] private TMP_InputField txtSpaceX, txtSpaceY;
        [SerializeField] private TMP_InputField txtLayer;
        [SerializeField] private UIToolAnalytic uiToolAnalytic;
        [SerializeField] private LevelDataItemGenerator itemGenerator;
        [SerializeField] private Toggle lockPosition;
        [SerializeField] private Toggle convertItemIdToggle;
        [SerializeField] private Toggle dropLevelToggle;
        [SerializeField] private LevelScreenshotTool screenshotTool;

        [SerializeField] private UITargetController uiTargetController;

        public Toggle DropLevelToggle => dropLevelToggle;
        public LevelData_SkewerJam LevelData => levelData;
        public SonatLevelService LevelService => levelService;

        private void Start()
        {
            ShowRightPanel(null);
            txtSnapGrid.text = ToolManager.Instance.gridSnap.ToString();
            txtSnapGrid.onValueChanged.AddListener((s) =>
            {
                if (float.TryParse(txtSnapGrid.text, out float result))
                {
                    ToolManager.Instance.gridSnap = result;
                }
            });

            // Initialize spacing fields with current values
            txtSpaceX.text = ToolManager.Instance.spaceX.ToString();
            txtSpaceY.text = ToolManager.Instance.spaceY.ToString();

            txtLayer.text = "0";
            txtLayer.onSubmit.AddListener(UpdateLayer);

            // Find item generator if not assigned
            if (itemGenerator == null)
                itemGenerator = FindObjectOfType<LevelDataItemGenerator>();

            levelService = Instantiate(levelService);
            lockPosition.onValueChanged.RemoveAllListeners();
            lockPosition.onValueChanged.AddListener(SetLockPosition);
            lockPosition.gameObject.SetActive(false);
            txtLevel.onSubmit.AddListener(OnLevelInputChange);
            txtLevelFolder.onSubmit.AddListener(OnLevelFolderInputChange);

            dropLevelToggle.onValueChanged.RemoveAllListeners();
            dropLevelToggle.onValueChanged.AddListener(OnChangeDropLevelToggle);

            AutoFillFolderPath();
        }

        private void OnDestroy()
        {
            txtLevel.onSubmit.RemoveListener(OnLevelInputChange);
            txtLevelFolder.onSubmit.RemoveListener(OnLevelFolderInputChange);
            dropLevelToggle.onValueChanged.RemoveListener(OnChangeDropLevelToggle);
        }

        private void OnLevelFolderInputChange(string path)
        {
            PlayerPrefs.SetString("LastLevelFolderPath", path);
        }

        private void AutoFillFolderPath()
        {
            txtLevelFolder.text = PlayerPrefs.GetString("LastLevelFolderPath", "");
        }

        private void OnLevelInputChange(string level)
        {
            DisplaySheetDataForCurrentLevel();
        }


        public void OnSelectToolGrill(ToolGrill _toolGrill)
        {
            this.toolGrill = _toolGrill;
            ShowRightPanel(uiToolGrill.gameObject);
            uiToolGrill.Setup(toolGrill.GrillData);
        }

        public void OnSelectToolConveyor(ToolConveyor _toolConveyor)
        {
            ShowRightPanel(uiToolConveyor.gameObject);
            this.toolConveyor = _toolConveyor;
            uiToolConveyor.SetData(_toolConveyor);
        }

        public void OnSelectToolDropColumn(ToolDropColumn _toolDropColumn)
        {
            ShowRightPanel(_toolDropColumn.gameObject); // không show gì
            this.toolDropColumn = _toolDropColumn;
            // uiToolDropColumn.SetData(_toolDropColumn);
        }

        #region Grill

        public void UpdateGrill()
        {
            this.toolGrill.OnGrillUpdate();
            UpdateItemCount();
        }

        public void AddGrill()
        {
            CreateGrill(GrillType.Normal, 3);
        }

        public void AddGrillSingle()
        {
            CreateGrill(GrillType.Normal, 1);
        }

        public void AddGrillLock()
        {
            CreateGrill(GrillType.Lock, 3);
        }

        public void AddGrillLockAndKey()
        {
            CreateGrill(GrillType.LockAndKey, 3);
        }

        public void AddGrillLockAndKey2()
        {
            CreateGrill(GrillType.LockAndKey2, 3);
        }

        public void AddGrillIce()
        {
            CreateGrill(GrillType.Ice, 3);
        }

        public void AddGrillIceMain()
        {
            CreateGrill(GrillType.IceMain, 3);
        }

        public void AddGrillIceNeighor()
        {
            CreateGrill(GrillType.IceNeighbor, 3);
        }

        public void AddGrillLockAds()
        {
            CreateGrill(GrillType.LockAds, 3);
        }

        public void AddGrillLid()
        {
            CreateGrill(GrillType.Lid, 3);
        }

        public void AddGrillDrop5()
        {
            CreateGrill(GrillType.Normal, 5);
        }

        public void AddGrillDrop6()
        {
            CreateGrill(GrillType.Normal, 6);
        }

        public void AddGrillDrop7()
        {
            CreateGrill(GrillType.Normal, 7);
        }

        public void AddGrillVending()
        {
            CreateGrill(GrillType.Vending, 1);
        }

        public void AddGrillDropLock()
        {
            CreateGrill(GrillType.Lock, 7);
        }

        public void AddGrillDropLockAds()
        {
            CreateGrill(GrillType.LockAds, 7);
        }

        public void AddGrillDropIce()
        {
            CreateGrill(GrillType.Ice, 7);
        }

        public void AddGrillDropLockAndKey()
        {
            CreateGrill(GrillType.LockAndKey, 7);
        }

        public void AddGrillDropLockAndKey2()
        {
            CreateGrill(GrillType.LockAndKey2, 7);
        }

        public void AddGrillDropLid()
        {
            CreateGrill(GrillType.Lid, 7);
        }

        public void AddOctoChef()
        {
            CreateOctochef();
        }

        public void AddGrillSingleMin()
        {
            var conveyor = ToolManager.Instance.GetConveyor(ConveyorType.HorizontalMin);
            if (conveyor == null) return;

            ToolGrill toolGrill = CreateGrill(GrillType.SingleMin, 1);
            toolGrill.transform.position = conveyor.transform.position + (conveyor.conveyData.grillIds.Count - 6) * Vector3.right * 1.5f;
            toolGrill.UpdatePosition();
            conveyor.CheckGrill();
        }

        public void AddGrillSpicy()
        {
            CreateGrill(GrillType.Spicy, 3);
        }

        public void AddGrillBroken()
        {
            CreateGrill(GrillType.Broken, 3);
        }

        public void AddGrillOvercooked()
        {
            CreateGrill(GrillType.Overcooked, 3);
        }

        public void AddGrillBomb()
        {
            CreateGrill(GrillType.Bomb, 3);
        }

        public void AddGrillShutter()
        {
            CreateGrill(GrillType.Shutter, 3);
        }

        public void AddGrillVending3()
        {
            CreateGrill(GrillType.Vending, 3);
        }


        public ToolGrill CreateGrill(GrillType grillType, int numberSlot)
        {
            GrillData refData = null;
            if (ToolManager.Instance.toolGrillSelector.SelectedGrills.Count == 1)
            {
                var selectedGrill = ToolManager.Instance.toolGrillSelector.SelectedGrills[0];
                refData = selectedGrill.GrillData;
                RemoveGrill(selectedGrill);
            }

            GrillData grillData = CreateGrillData(grillType, refData);
            grillData.slotCount = numberSlot;

            ToolGrill toolGrill = ToolManager.Instance.CreateToolGrill(grillType, numberSlot);
            grillData.grillType = grillType;
            if (refData == null)
            {
                grillData.id = ToolManager.Instance.GetAvailableGrillId();
                grillData.position = new Vector3Data(toolGrill.transform.position);
            }

            levelData.grillData ??= new();
            levelData.grillData.Add(grillData);

            // Add undo for adding grill
            ToolManager.Instance.undoController.AddGrillOperation(grillData, GrillOperationType.Add);

            toolGrill.SetData(grillData);
            OnSelectToolGrill(toolGrill);
            ToolManager.Instance.toolGrillSelector.ClearSelection();
            UpdateItemCount();
            return toolGrill;
        }

        private GrillData CreateGrillData(GrillType grillType, GrillData refData = null)
        {
            switch (grillType)
            {
                case GrillType.Lid:
                    GrillLidData grillLidData = new GrillLidData();
                    if (refData != null)
                    {
                        grillLidData.id = refData.id;
                        grillLidData.position = refData.position;
                        grillLidData.layer = refData.layer;
                        grillLidData.slotCount = refData.SlotCount;
                        grillLidData.time = refData.time;
                        grillLidData.move = refData.move;
                        //grillLidData.isLock = refData.isLock;
                    }

                    return grillLidData;
                case GrillType.Lock:
                    GrillLockData grillLockData = new GrillLockData();
                    if (refData != null)
                    {
                        grillLockData.id = refData.id;
                        grillLockData.position = refData.position;
                        grillLockData.layer = refData.layer;
                        grillLockData.slotCount = refData.SlotCount;
                        grillLockData.time = refData.time;
                        grillLockData.move = refData.move;
                    }

                    return grillLockData;
                case GrillType.Ice:
                    GrillIceData grillIceData = new GrillIceData();
                    if (refData != null)
                    {
                        grillIceData.id = refData.id;
                        grillIceData.position = refData.position;
                        grillIceData.layer = refData.layer;
                        grillIceData.slotCount = refData.SlotCount;
                        grillIceData.time = refData.time;
                        grillIceData.move = refData.move;
                    }

                    return grillIceData;
                case GrillType.Spicy:
                    GrillSpicyData grillSpicyData = new GrillSpicyData();
                    if (refData != null)
                    {
                        grillSpicyData.id = refData.id;
                        grillSpicyData.position = refData.position;
                        grillSpicyData.layer = refData.layer;
                        grillSpicyData.slotCount = refData.SlotCount;
                        grillSpicyData.time = refData.time;
                        grillSpicyData.move = refData.move;
                    }

                    return grillSpicyData;
                case GrillType.Broken:
                    GrillBrokenData grillBrokenData = new GrillBrokenData();
                    if (refData != null)
                    {
                        grillBrokenData.id = refData.id;
                        grillBrokenData.position = refData.position;
                        grillBrokenData.layer = refData.layer;
                        grillBrokenData.slotCount = refData.SlotCount;
                        grillBrokenData.time = refData.time;
                        grillBrokenData.move = refData.move;
                    }

                    return grillBrokenData;
                case GrillType.IceMain:
                    GrillIceMainData grillIceMainData = new GrillIceMainData();
                    if (refData != null)
                    {
                        grillIceMainData.id = refData.id;
                        grillIceMainData.position = refData.position;
                        grillIceMainData.layer = refData.layer;
                        grillIceMainData.slotCount = refData.SlotCount;
                        grillIceMainData.mainGrillId = refData.mainGrillId;
                        grillIceMainData.time = refData.time;
                        grillIceMainData.move = refData.move;
                    }
                    return grillIceMainData;
                case GrillType.IceNeighbor:
                    GrillIceNeighborData grillIceNeighborData = new GrillIceNeighborData();
                    if (refData != null)
                    {
                        grillIceNeighborData.id = refData.id;
                        grillIceNeighborData.position = refData.position;
                        grillIceNeighborData.layer = refData.layer;
                        grillIceNeighborData.slotCount = refData.SlotCount;
                        grillIceNeighborData.mainGrillId = refData.mainGrillId;
                        grillIceNeighborData.time = refData.time;
                        grillIceNeighborData.move = refData.move;
                    }
                    return grillIceNeighborData;
                case GrillType.Overcooked:
                    GrillOvercookedData grillOvercookedData = new GrillOvercookedData();
                    if (refData != null)
                    {
                        grillOvercookedData.id = refData.id;
                        grillOvercookedData.position = refData.position;
                        grillOvercookedData.layer = refData.layer;
                        grillOvercookedData.slotCount = refData.SlotCount;
                        grillOvercookedData.time = refData.time;
                        grillOvercookedData.move = refData.move;
                    }
                    return grillOvercookedData;
                default:
                    GrillData grillData = new GrillData();
                    if (refData != null)
                    {
                        grillData.id = refData.id;
                        grillData.position = refData.position;
                        grillData.layer = refData.layer;
                        grillData.slotCount = refData.SlotCount;
                        grillData.time = refData.time;
                        grillData.move = refData.move;
                        //grillData.isLock = refData.isLock;
                    }

                    return grillData;
            }
        }


        public void RemoveGrill(ToolGrill toolGrill)
        {
            // Store the grill data for undo before removing
            var grillData = toolGrill.GrillData;

            var obstacle = ToolManager.Instance.GetObstacle(toolGrill.GrillData.id);
            if (obstacle != null)
            {
                levelData.obstacleData.RemoveAll(x => x.id == obstacle.id);
                ToolManager.Instance.RemoveObstacle(obstacle);
            }

            //RemoveObstacles(toolGrill.GrillData.id);
            levelData.grillData.Remove(toolGrill.GrillData);
            uiToolGrill.gameObject.SetActive(false);
            ToolManager.Instance.RemoveToolGrill(toolGrill);
            this.toolGrill = null;

            // Add undo for deleting grill
            ToolManager.Instance.undoController.AddGrillOperation(grillData, GrillOperationType.Delete, grillData);

            UpdateItemCount();
        }

        public void SwapGrill()
        {
            if (ToolManager.Instance.toolGrillSelector.SelectedGrills.Count != 2)
            {
                PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = "Select 2 grills" });
                return;
            }

            var grill1 = ToolManager.Instance.toolGrillSelector.SelectedGrills[0];
            var grill2 = ToolManager.Instance.toolGrillSelector.SelectedGrills[1];

            // Store original positions for undo
            var pos1 = new Vector3Data(grill1.transform.position);
            var pos2 = new Vector3Data(grill2.transform.position);

            ToolManager.Instance.undoController.AddGrillOperation(grill1.GrillData, GrillOperationType.Move, pos1);
            ToolManager.Instance.undoController.AddGrillOperation(grill2.GrillData, GrillOperationType.Move, pos2);

            (grill1.transform.position, grill2.transform.position) = (grill2.transform.position, grill1.transform.position);

            // Update positions in data
            grill1.UpdatePosition();
            grill2.UpdatePosition();

            ShowRightPanel(null);
            ToolManager.Instance.toolGrillSelector.ClearSelection();
        }

        #endregion

        #region Conveyor

        public void AddConveyorHorizontal()
        {
            ToolConveyor toolConveyor = CreateConveyor(MoveType.Horizontal);
            toolConveyor.conveyData.conveyorType = ConveyorType.Horizontal;
        }

        public void AddConveyorVertical()
        {
            ToolConveyor toolConveyor = CreateConveyor(MoveType.Vertical);
            toolConveyor.conveyData.conveyorType = ConveyorType.Vertical;
        }

        public void AddConveyorHorizontalMin()
        {
            ToolConveyor toolConveyor = CreateConveyor(MoveType.Horizontal);
            toolConveyor.conveyData.conveyorType = ConveyorType.HorizontalMin;
        }

        public void AddConveyorHorizontalSimple()
        {
            ToolConveyor toolConveyor = CreateConveyor(MoveType.Horizontal);
            toolConveyor.conveyData.conveyorType = ConveyorType.HorizontalSimple;
        }

        private ToolConveyor CreateConveyor(MoveType moveType)
        {
            ConveyorData conveyorData = new ConveyorData();
            conveyorData.moveType = moveType;
            conveyorData.speed = 1;
            ToolConveyor toolConveyor = ToolManager.Instance.CreateToolConveyor(moveType);
            conveyorData.id = ToolManager.Instance.GetAvailableConveyorId();
            conveyorData.position = new Vector3Data(toolConveyor.transform.position);

            levelData.conveyorData ??= new();
            levelData.conveyorData.Add(conveyorData);

            toolConveyor.SetData(conveyorData);
            OnSelectToolConveyor(toolConveyor);
            return toolConveyor;
        }

        public void RemoveConveyor(ToolConveyor toolConveyor)
        {
            foreach (var grillId in toolConveyor.conveyData.grillIds)
            {
                ToolGrill toolGrill = ToolManager.Instance.GetToolGrill(grillId);
                if (toolGrill != null)
                {
                    //RemoveObstacles(toolGrill.GrillData.id);
                    levelData.grillData.Remove(toolGrill.GrillData);
                    ToolManager.Instance.RemoveToolGrill(toolGrill);
                }
            }

            levelData.conveyorData.Remove(toolConveyor.conveyData);
            ShowRightPanel(null);
            ToolManager.Instance.RemoveToolConveyor(this.toolConveyor);
            this.toolConveyor = null;
            UpdateItemCount();
        }

        public void RemoveDropColumn(ToolDropColumn toolDropColumn)
        {
            foreach (var grillId in toolDropColumn.dropColumnData.grillIds)
            {
                ToolGrill toolGrill = ToolManager.Instance.GetToolGrill(grillId);
                if (toolGrill != null)
                {
                    levelData.grillData.Remove(toolGrill.GrillData);
                    ToolManager.Instance.RemoveToolGrill(toolGrill);
                }
            }

            levelData.listDropColumnData.Remove(toolDropColumn.dropColumnData);
            ShowRightPanel(null);
            ToolManager.Instance.RemoveToolDropColumn(this.toolDropColumn);
            this.toolDropColumn = null;
            UpdateItemCount();
        }

        #endregion


        #region Obstacle

        public ToolObstacleBase CreateObstacle(ObstacleType obstacleType)
        {
            var obstacleId = 0;
            levelData.obstacleData ??= new();

            if (levelData.obstacleData.Count > 0)
            {
                obstacleId = levelData.obstacleData.Max(x => x.id) + 1;
            }

            ObstacleData obstacleData = new ObstacleData()
            {
                id = (byte)obstacleId,
                obstacleType = obstacleType,
                position = new Vector3Data(Vector3.zero)
            };

            levelData.obstacleData.Add(obstacleData);

            ToolObstacleBase toolObstacle = ToolManager.Instance.CreateToolObstacle(obstacleType);
            toolObstacle.SetData(obstacleData);
            ToolManager.Instance.toolGrillSelector.ClearSelection();
            return toolObstacle;
        }

        public void RemoveObstacle(ToolObstacleBase obstacle)
        {
            if (obstacle.toolGrills != null)
            {
                foreach (var grill in obstacle.toolGrills)
                {
                    grill.transform.SetParent(null);
                }
            }

            levelData.obstacleData.RemoveAll(x => x.id == obstacle.id);
            ToolManager.Instance.RemoveObstacle(obstacle);
        }

        private void CreateOctochef()
        {
            if (ToolManager.Instance.toolGrillSelector.SelectedGrills.Count == 1)
            {
                var selectedGrill = ToolManager.Instance.toolGrillSelector.SelectedGrills[0];

                ToolObstacleBase toolObstacle = CreateObstacle(ObstacleType.OctoChef);
                toolObstacle.obstacleData.grillIds = new List<int>() { selectedGrill.GrillData.id };
                toolObstacle.SetGrills(new List<ToolGrill>() { selectedGrill });

                OnSelectToolGrill(selectedGrill);
                UpdateItemCount();
            }
        }

        public void RemoveOctochef()
        {
            if (ToolManager.Instance.toolGrillSelector.SelectedGrills.Count == 1)
            {
                var selectedGrill = ToolManager.Instance.toolGrillSelector.SelectedGrills[0];
                var grillId = selectedGrill.GrillData.id;

                var obstacle = ToolManager.Instance.GetObstacle(grillId);
                if (obstacle != null)
                {
                    RemoveObstacle(obstacle);
                }
            }
        }

        public void AddLockAreaObstacleHorizontal()
        {
            CreateObstacle(ObstacleType.LockAreaHorizontal);
        }

        public void AddLockAreaObstacleVertical()
        {
            CreateObstacle(ObstacleType.LockAreaVertical);
        }

        public void AddDropColumn()
        {
            CreateDropColumn();
        }

        #endregion


        public ToolDropColumn CreateDropColumn()
        {
            var dropColumnId = 0;
            levelData.listDropColumnData ??= new();

            if (levelData.listDropColumnData.Count > 0)
            {
                dropColumnId = levelData.listDropColumnData.Max(x => x.id) + 1;
            }

            DropColumnData dropColumnData = new DropColumnData()
            {
                id = (byte)dropColumnId,
                grillIds = new List<int>()
            };

            levelData.listDropColumnData.Add(dropColumnData);

            ToolDropColumn toolDropColumn = ToolManager.Instance.CreateToolDropColumn();
            toolDropColumn.SetData(dropColumnData);
            // ToolManager.Instance.toolGrillSelector.ClearSelection();
            return toolDropColumn;
        }
        // public void AddObstacles(ObstacleType obstacleType, ToolGrill toolGrill)
        // {
        //     var obstacle = ToolManager.Instance.CreateObstacle(obstacleType, toolGrill);
        //     obstacle.SetData(toolGrill);
        // }

        // public void RemoveObstacle(ToolObstacleBase toolObstacle)
        // {
        //     ToolManager.Instance.RemoveObstacle(toolObstacle);
        // }
        //
        // private void RemoveObstacles(byte id)
        // {
        //     ToolManager.Instance.RemoveObstacles(id);
        // }


        public void LoadLevel()
        {
            if (int.TryParse(txtLevel.text, out int level))
            {
                LevelType levelType = levelTypeDropdown.GetLevelTypeOpt();
                // if (levelType == LevelType.Food)
                // {
                levelService.SetFolder(txtLevelFolder.text);
                // }
                // else
                // {
                //     levelService.SetFolder($"{levelType}/{txtLevelFolder.text}");
                // }

                levelData = levelService.GetLevelData<LevelData_SkewerJam>(level, GameMode.Classic, true);
                if (levelData == null)
                {
                    PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = $"Not found level {level}" });
                    return;
                }

                // Apply item ID swapping if the service is available and loaded
                if (convertItemIdToggle.isOn && ItemIdSwapService.Instance != null && ItemIdSwapService.Instance.IsDataLoaded)
                {
                    int swappedCount = ItemIdSwapHelper.SwapItemIdsInLevel(levelData);
                    if (swappedCount > 0)
                    {
                        Debug.Log($"Applied item ID swapping to level {level}. Swapped {swappedCount} items.");
                    }
                }

                GenerateLevel();

                // Also display Google Sheets data for this level if available
                if (uiToolSheetData != null && uiToolSheetData.IsDataLoaded())
                {
                    uiToolSheetData.DisplayLevelFromInput(level);
                }
            }
        }

        public void NextLevel()
        {
            if (int.TryParse(txtLevel.text, out int level))
            {
                level++;
                txtLevel.text = level.ToString();
                LoadLevel();
            }
        }

        public void PreviousLevel()
        {
            if (int.TryParse(txtLevel.text, out int level) && level > 1)
            {
                level--;
                txtLevel.text = level.ToString();
                LoadLevel();
            }
        }

        public void SaveLevel()
        {
            if (!ValidateLevel())
            {
                return;
            }

            if (int.TryParse(txtLevel.text, out int level) && levelData != null
                && ushort.TryParse(txtTime.text, out ushort time)
                && ushort.TryParse(txtMove.text, out ushort move))
            {
                levelData.level = level;
                if (time < 10) time = 180;
                levelData.time = time;
                levelData.move = move;
                levelData.difficulty = levelDifficultyDropdown.GetDifficultyOpt();
                levelData.difficultyValue = levelDifficultyValueDropdown.GetDifficultyValueOpt();
                LevelType levelType = levelTypeDropdown.GetLevelTypeOpt();
                LevelMode levelMode = levelModeDropdown.GetLevelModeOpt();
                levelData.targetData = new List<TargetData>();
                levelData.targetData.AddRange(uiTargetController.targetList);
                levelData.levelMode = levelMode;
                levelData.levelType = levelType;

                levelData.listDropColumnData = ValidateDropColumnData(levelData.listDropColumnData);
                //if (levelType == LevelType.Food)
                //{
                levelService.SetFolder(txtLevelFolder.text);
                // }
                // else
                // {
                //     levelService.SetFolder($"{levelType}/{txtLevelFolder.text}");
                // }

                levelService.SaveLevel(levelData);

                // Take screenshot after saving the level
                TakeLevelScreenshot(level);

                PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = $"Saved level {level}!" });
            }
        }

        public int GetLevel()
        {
            return int.TryParse(txtLevel.text, out int level) ? level : 0;
        }

        public void ClearBoard()
        {
            ClearLevel();
            levelData = new LevelData_SkewerJam();
        }

        public void ClearLevel()
        {
            ToolManager.Instance.ClearTable();
            ShowRightPanel(null);
            txtLayer.text = "0";
            uiToolAnalytic.Clear();
            uiToolGrill.Clear();
            uiToolOrder.Clear();
            uiTargetController.Clear();
            dropLevelToggle.SetIsOnWithoutNotify(false);
        }


        private void GenerateLevel()
        {
            ClearLevel();
            dropLevelToggle.SetIsOnWithoutNotify(levelData.isDropMode);
            txtTime.text = levelData.time.ToString();
            txtMove.text = levelData.move.ToString();
            uiTargetController.SetData(levelData.targetData);
            levelModeDropdown.SetLevelModeOpt(levelData.levelMode);
            levelDifficultyDropdown.SetDifficultyOpt(levelData.difficulty);
            levelDifficultyValueDropdown.SetDifficultyValueOpt(levelData.difficultyValue);
            levelTypeDropdown.SetLevelTypeOpt(levelData.levelType);
            byte numberZero = 0;
            if (levelData.grillData != null)
                foreach (var _grillData in levelData.grillData.ToList())
                {
                    GrillData grillData = _grillData;
                    if (grillData.isLock && grillData.grillType != GrillType.Shutter)
                    {
                        grillData.grillType = GrillType.Lock;
                        grillData.isLock = false;
                    }


                    if ((grillData.grillType == GrillType.Lock && grillData is not GrillLockData) ||
                        (grillData.grillType == GrillType.Ice && grillData is not GrillIceData))
                    {
                        GrillData newGrillData = CreateGrillData(grillData.grillType, grillData);
                        newGrillData.grillType = grillData.grillType;
                        int index = levelData.grillData.IndexOf(grillData);
                        levelData.grillData[index] = newGrillData;
                        grillData = newGrillData;
                    }

                    // if (grillData.layer != null)
                    // {
                    //     foreach (var layerData in grillData.layer)
                    //     {
                    //         if (layerData.itemData != null)
                    //         {
                    //             foreach (var itemData in layerData.itemData)
                    //             {
                    //                 if (itemData != null && (itemData.id > 410 || itemData.id == 358) && itemData.id != 1500)
                    //                 {
                    //                     itemData.id = itemData.id % 410;
                    //                 }
                    //             }
                    //         }
                    //     }
                    // }

                    ToolGrill toolGrill = ToolManager.Instance.CreateToolGrill(grillData.grillType, grillData.SlotCount);
                    toolGrill.transform.position = grillData.position.ToVector3();
                    toolGrill.SetData(grillData);
                    if (grillData.id == 0)
                    {
                        numberZero++;
                        if (numberZero > 1)
                        {
                            grillData.id = ToolManager.Instance.GetAvailableGrillId();
                        }
                    }
                }

            if (levelData.conveyorData != null)
                foreach (var conveyorData in levelData.conveyorData)
                {
                    if (conveyorData.conveyorType == ConveyorType.None)
                    {
                        conveyorData.conveyorType = conveyorData.moveType switch
                        {
                            MoveType.Horizontal => ConveyorType.Horizontal,
                            MoveType.Vertical => ConveyorType.Vertical,
                            _ => conveyorData.conveyorType
                        };
                    }

                    ToolConveyor toolConveyor = ToolManager.Instance.CreateToolConveyor(conveyorData.moveType);
                    toolConveyor.transform.position = conveyorData.position.ToVector3();
                    toolConveyor.SetData(conveyorData);
                }

            if (levelData.obstacleData != null)
                foreach (var grillObstacleData in levelData.obstacleData)
                {
                    ToolObstacleBase toolObstacle = ToolManager.Instance.CreateToolObstacle(grillObstacleData.obstacleType);
                    toolObstacle.SetData(grillObstacleData);
                    toolObstacle.SetGrills(ToolManager.Instance.GetToolGrills(grillObstacleData.grillIds));
                }

            if (levelData.listDropColumnData != null)
                foreach (var dropColumnData in levelData.listDropColumnData)
                {
                    ToolDropColumn toolDropColumn = ToolManager.Instance.CreateToolDropColumn();
                    toolDropColumn.SetData(dropColumnData);
                    // toolDropColumn.UpdateGrillPosition();
                }

            UpdateItemCount();
        }
        public void CreateTableWithSize()
        {
            if (int.TryParse(txtTableX.text, out int x) && int.TryParse(txtTableY.text, out int y))
            {
                ToolManager.Instance.CreateTableWithSize(x, y, levelData);
            }
        }

        public void SortSelectedToGrid()
        {
            // Get the grid parameters from input fields
            if (!int.TryParse(txtColumn.text, out int gridX) ||
                !int.TryParse(txtRow.text, out int gridY) ||
                !float.TryParse(txtSpaceX.text, out float spaceX) ||
                !float.TryParse(txtSpaceY.text, out float spaceY))
            {
                PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = "Please enter valid grid parameters!" });
                return;
            }

            // Get selected grills from the selector
            ToolGrillSelector selector = FindObjectOfType<ToolGrillSelector>();
            if (selector == null)
            {
                PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = "ToolGrillSelector not found!" });
                return;
            }

            List<ToolGrill> selectedGrills = selector.SelectedGrills;
            if (selectedGrills.Count == 0)
            {
                PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = "No grills selected!" });
                return;
            }

            if (selectedGrills.Count > gridX * gridY)
            {
                PanelManager.Instance.OpenForget<NotifyPanel>(
                    new NotifyPanel.Data() { content = $"Too many grills! Grid can only fit {gridX * gridY} grills." });
                return;
            }

            // Sort selected grills to grid positions
            SortGrillsToGrid(selectedGrills, gridX, gridY, spaceX, spaceY);

            Debug.Log($"Sorted {selectedGrills.Count} grills to {gridX}x{gridY} grid");
        }

        public void CenterSelectedToOrigin()
        {
            // Get selected grills from the selector
            ToolGrillSelector selector = FindObjectOfType<ToolGrillSelector>();
            if (selector == null)
            {
                PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = "ToolGrillSelector not found!" });
                return;
            }

            List<ToolGrill> selectedGrills = selector.SelectedGrills;
            if (selectedGrills.Count == 0)
            {
                PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = "No grills selected!" });
                return;
            }

            // Store original positions for undo
            foreach (var grill in selectedGrills)
            {
                if (grill != null)
                {
                    var originalPos = new Vector3Data(grill.transform.position);
                    ToolManager.Instance.undoController.AddGrillOperation(grill.GrillData, GrillOperationType.Move, originalPos);
                }
            }

            // Calculate the center of the selected objects
            Vector3 selectionCenter = CalculateSelectionCenter(selectedGrills);

            // Calculate the offset needed to move center to origin
            Vector3 offsetToOrigin = Vector3.zero - selectionCenter;

            // Move all selected grills by the offset
            foreach (ToolGrill grill in selectedGrills)
            {
                if (grill != null)
                {
                    Vector3 newPosition = grill.transform.position + offsetToOrigin;
                    grill.transform.position = newPosition;
                    grill.UpdatePosition();
                    ToolManager.Instance.OnGrillChanged?.Invoke(grill);
                }
            }

            // Update the selection box to reflect new positions
            if (selector != null)
            {
                selector.UpdateFinalSelectionBox();
            }

            Debug.Log($"Centered {selectedGrills.Count} grills to origin (offset: {offsetToOrigin})");
        }

        /// <summary>
        /// Generate random items for all grills using the item generator
        /// </summary>
        public void GenerateRandomItemsForLevel()
        {
            if (itemGenerator == null)
            {
                PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = "Item generator not found!" });
                return;
            }

            itemGenerator.GenerateRandomItems();
        }

        /// <summary>
        /// Generate random items with custom parameters
        /// </summary>
        public void GenerateRandomItemsWithParameters(int totalItems, string itemIdsString, int layersPerGrill)
        {
            if (itemGenerator == null)
            {
                PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = "Item generator not found!" });
                return;
            }

            itemGenerator.GenerateRandomItems(totalItems, itemIdsString, layersPerGrill);
        }

        /// <summary>
        /// Clear all items from all grills
        /// </summary>
        public void ClearAllItemsFromLevel()
        {
            if (itemGenerator == null)
            {
                PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = "Item generator not found!" });
                return;
            }

            itemGenerator.ClearAllItems();
        }

        /// <summary>
        /// Validate the current level data
        /// </summary>
        public void ValidateCurrentLevel()
        {
            if (itemGenerator == null)
            {
                PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = "Item generator not found!" });
                return;
            }

            bool isValid = itemGenerator.ValidateLevelData();
            if (isValid)
            {
                PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = "Level validation passed!" });
            }
            else
            {
                PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = "Level validation failed! Check console for details." });
            }
        }

        /// <summary>
        /// Print level statistics
        /// </summary>
        public void PrintLevelStats()
        {
            if (itemGenerator == null)
            {
                PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = "Item generator not found!" });
                return;
            }

            itemGenerator.PrintLevelStatistics();
        }

        private void SortGrillsToGrid(List<ToolGrill> grills, int gridX, int gridY, float spaceX, float spaceY)
        {
            // Store original positions for undo
            foreach (var grill in grills)
            {
                if (grill != null)
                {
                    var originalPos = new Vector3Data(grill.transform.position);
                    ToolManager.Instance.undoController.AddGrillOperation(grill.GrillData, GrillOperationType.Move, originalPos);
                }
            }

            // Calculate the center of the selected objects
            Vector3 selectionCenter = CalculateSelectionCenter(grills);

            // Calculate grid center offset
            float offsetX = (gridX - 1) * spaceX / 2f;
            float offsetY = (gridY - 1) * spaceY / 2f;

            // Create grid positions centered at the selection center
            List<Vector3> gridPositions = new List<Vector3>();
            for (int row = 0; row < gridY; row++)
            {
                for (int col = 0; col < gridX; col++)
                {
                    Vector3 gridPos = new Vector3(
                        selectionCenter.x + col * spaceX - offsetX,
                        selectionCenter.y + row * spaceY - offsetY,
                        0
                    );
                    gridPositions.Add(gridPos);
                }
            }

            // Sort grills by their current position (top-left to bottom-right)
            grills.Sort((a, b) =>
            {
                // Sort by Y first (top to bottom), then by X (left to right)
                if (Mathf.Abs(a.transform.position.y - b.transform.position.y) > 0.1f)
                {
                    return b.transform.position.y.CompareTo(a.transform.position.y); // Higher Y first
                }

                return a.transform.position.x.CompareTo(b.transform.position.x); // Lower X first
            });

            // Move grills to grid positions
            for (int i = 0; i < grills.Count && i < gridPositions.Count; i++)
            {
                if (grills[i] != null)
                {
                    grills[i].transform.position = gridPositions[i];
                    grills[i].UpdatePosition();
                    ToolManager.Instance.OnGrillChanged?.Invoke(grills[i]);
                }
            }

            // Update the selection box to reflect new positions
            ToolGrillSelector selector = FindObjectOfType<ToolGrillSelector>();
            if (selector != null)
            {
                selector.UpdateFinalSelectionBox();
            }
        }

        private Vector3 CalculateSelectionCenter(List<ToolGrill> grills)
        {
            if (grills.Count == 0) return Vector3.zero;

            Vector3 center = Vector3.zero;
            int validGrills = 0;

            foreach (ToolGrill grill in grills)
            {
                if (grill != null)
                {
                    center += grill.transform.position;
                    validGrills++;
                }
            }

            return validGrills > 0 ? center / validGrills : Vector3.zero;
        }


        // public void LoadLevelFlower()
        // {
        //     if (int.TryParse(txtLevel.text, out int level))
        //     {
        //         LevelDataFlower levelDataFlower = levelServiceFlower.GetLevelData<LevelDataFlower>(level, GameMode.Classic, true);
        //         if (levelData == null)
        //         {
        //             PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = $"Not found level {level}" });
        //             return;
        //         }

        //         levelData = levelDataFlower.ConvertToLevelData();
        //         GenerateLevel();
        //     }
        // }


        private bool ValidateLevel()
        {
            if (!ValidateGeneralData())
            {
                return false;
            }

            Dictionary<int, int> counts = new Dictionary<int, int>();

            bool hasLock = false;
            bool hasKey = false;
            int lockArea = 0;
            int keyArea = 0;
            //bool dropLevel = false;

            foreach (var grillData in levelData.grillData)
            {
                if (grillData.grillType is GrillType.LockAndKey) hasLock = true;
                //if (!dropLevel && grillData.SlotCount >= 5) dropLevel = true;
                if (grillData.layer != null)
                {
                    bool layerNull = false;
                    foreach (var layerData in grillData.layer)
                    {
                        if (layerNull)
                        {
                            PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = "Some layers are empty!" });
                            return false;
                        }

                        layerNull = true;
                        if (layerData.itemData != null)
                        {
                            bool sameId = grillData.SlotCount > 1;
                            int lastId = -1;
                            foreach (var itemData in layerData.itemData)
                            {
                                if (itemData != null && itemData.id > 0)
                                {
                                    if (itemData.itemType == ItemType.Key) hasKey = true;
                                    if (itemData.itemType == ItemType.KeyArea) keyArea++;
                                    layerNull = false;
                                    if (!counts.TryAdd(itemData.id, 1))
                                    {
                                        counts[itemData.id]++;
                                    }

                                    if (sameId)
                                    {
                                        if (lastId < 0) lastId = itemData.id;
                                        else
                                        {
                                            if (lastId != itemData.id)
                                            {
                                                sameId = false;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    sameId = false;
                                }
                            }

                            // if (sameId)
                            // {
                            //     PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data()
                            //     { content = "Has a layer contains 3 items with the same id!" });
                            //     OnSelectToolGrill(ToolManager.Instance.GetToolGrill(grillData.id));
                            //     return false;
                            // }
                        }
                    }
                }
            }

            if (levelData.obstacleData != null)
            {
                foreach (var obstacleData in levelData.obstacleData)
                {
                    if (obstacleData.obstacleType == ObstacleType.LockAreaHorizontal || obstacleData.obstacleType == ObstacleType.LockAreaVertical)
                    {
                        lockArea++;
                    }
                }
            }

            if (hasLock != hasKey)
            {
                PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = "Lock and key is not validate!" });
                return false;
            }

            if (lockArea != keyArea)
            {
                PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = "Lock Area and key area is not validate!" });
                return false;
            }


            // foreach (var count in counts)
            // {
            //     if (count.Key == 1500)
            //     {
            //         continue;
            //     }

            //     if (count.Value % ToolManager.numberSlotMax != 0)
            //     {
            //         PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data()
            //         { content = $"Item {count.Key} is not match {ToolManager.numberSlotMax}!" });
            //         return false;
            //     }
            // }

            if (levelData.orderData != null)
            {
                levelData.orderData = levelData.orderData.Where(orderData => orderData != null && orderData.OrderItem.Count > 0).ToList();
                if (levelData.orderData.Count == 0) levelData.orderData = null;
            }

            ToolManager.Instance.ValidateAllObstacles();

            return true;
        }

        private bool ValidateGeneralData()
        {

            if (txtLevel.text.IsNullOrWhitespace())
            {
                PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = "Level is required!" });
                return false;
            }
            if (int.TryParse(txtLevel.text, out int level))
            {
                if (level < 1)
                {
                    PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = "Level is less than 1!" });
                    return false;
                }
            }
            else
            {
                PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = "Level is not a number!" });
                return false;
            }

            switch (levelData.levelMode)
            {
                case LevelMode.All:
                    if (txtTime.text.IsNullOrWhitespace())
                    {
                        PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = "Time is required!" });
                        return false;
                    }
                    if (int.TryParse(txtTime.text, out int time))
                    {
                        if (time < 1)
                        {
                            PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = "Time is less than 1!" });
                            return false;
                        }
                    }
                    else
                    {
                        PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = "Time is not a number!" });
                        return false;
                    }
                    break;
                case LevelMode.Target:
                    if (txtMove.text.IsNullOrWhitespace())
                    {
                        PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = "Move is required!" });
                        return false;
                    }
                    if (int.TryParse(txtMove.text, out int move))
                    {
                        if (move < 1)
                        {
                            {
                                PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = "Move is less than 1!" });
                                return false;
                            }
                        }
                    }
                    else
                    {
                        PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = "Move is not a number!" });
                        return false;
                    }

                    if (uiTargetController.targetList.Count == 0)
                    {
                        PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = "Target is required!" });
                        return false;
                    }
                    break;
                default:
                    break;
            }



            return true;
        }

        public void UpdateItemCount()
        {
            Dictionary<int, int> counts = new Dictionary<int, int>();
            if (levelData.grillData != null)
                foreach (var grillData in levelData.grillData)
                {
                    if (grillData.layer != null)
                        foreach (var layerData in grillData.layer)
                        {
                            if (layerData.itemData != null)
                                foreach (var itemData in layerData.itemData)
                                {
                                    if (itemData != null && itemData.id > 0)
                                        if (!counts.TryAdd(itemData.id, 1))
                                        {
                                            counts[itemData.id]++;
                                        }
                                }
                        }
                }
            uiToolAnalytic.UpdateData(counts);
        }

        public void UpdateConveyorData()
        {
            if (uiToolConveyor.gameObject.activeInHierarchy)
                uiToolConveyor.UpdateGrillCount();
        }

        public void ResetCamera()
        {
            Camera mainCamera = ToolManager.Instance.mainCamera;
            mainCamera.orthographicSize = 15;
            mainCamera.transform.position = new Vector3(0, 2, mainCamera.transform.position.z);
        }

        public void ShowOrder()
        {
            ShowRightPanel(uiToolOrder.gameObject);
            uiToolOrder.SetData(levelData);
        }

        public void ShowSheetData()
        {
            // uiToolSheetData is always visible, just ensure data is loaded
            if (uiToolSheetData != null && !uiToolSheetData.IsDataLoaded())
            {
                uiToolSheetData.LoadSheetData();
            }
        }

        public void LoadSheetData()
        {
            if (uiToolSheetData != null)
            {
                uiToolSheetData.LoadSheetData();
            }
        }

        public void DisplaySheetDataForCurrentLevel()
        {
            if (int.TryParse(txtLevel.text, out int level) && uiToolSheetData != null)
            {
                uiToolSheetData.DisplayLevelFromInput(level);
            }
        }

        public void ValidateCurrentLevelWithSheet()
        {
            if (uiToolSheetData == null || !uiToolSheetData.IsDataLoaded())
            {
                PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = "Please load sheet data first" });
                return;
            }

            if (int.TryParse(txtLevel.text, out int level))
            {
                var sheetData = uiToolSheetData.GetCurrentLevelData();
                if (sheetData != null)
                {
                    bool isValid = GoogleSheetsIntegrationHelper.ValidateLevelAgainstSheet(levelData, sheetData);
                    string report = GoogleSheetsIntegrationHelper.GenerateComparisonReport(levelData, sheetData);

                    if (isValid)
                    {
                        PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = $"Level {level} validation passed!\n{report}" });
                    }
                    else
                    {
                        PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = $"Level {level} validation failed!\n{report}" });
                    }
                }
            }
        }

        public void ApplySheetDataToCurrentLevel()
        {
            if (uiToolSheetData == null || !uiToolSheetData.IsDataLoaded())
            {
                PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = "Please load sheet data first" });
                return;
            }

            if (int.TryParse(txtLevel.text, out int level))
            {
                var sheetData = uiToolSheetData.GetCurrentLevelData();
                if (sheetData != null)
                {
                    GoogleSheetsIntegrationHelper.ApplySheetDataToLevel(sheetData, levelData);
                    PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = $"Applied sheet data to level {level}" });
                }
            }
        }

        public void LoadItemIdMappingData()
        {
            if (ItemIdSwapService.Instance != null)
            {
                ItemIdSwapService.Instance.LoadIdMappingData();
                PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = "Loading item ID mapping data..." });
            }
            else
            {
                PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = "ItemIdSwapService not available" });
            }
        }

        public void ApplyItemIdSwappingToCurrentLevel()
        {
            if (ItemIdSwapService.Instance == null || !ItemIdSwapService.Instance.IsDataLoaded)
            {
                PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = "Please load item ID mapping data first" });
                return;
            }

            if (levelData != null)
            {
                int swappedCount = ItemIdSwapHelper.SwapItemIdsInLevel(levelData);
                PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = $"Applied item ID swapping. Swapped {swappedCount} items." });

                // Regenerate the level to reflect the changes
                GenerateLevel();
            }
        }

        public void ReverseItemIdSwappingToCurrentLevel()
        {
            if (ItemIdSwapService.Instance == null || !ItemIdSwapService.Instance.IsDataLoaded)
            {
                PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = "Please load item ID mapping data first" });
                return;
            }

            if (levelData != null)
            {
                int swappedCount = ItemIdSwapHelper.ReverseSwapItemIdsInLevel(levelData);
                PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data()
                { content = $"Applied reverse item ID swapping. Swapped {swappedCount} items." });

                // Regenerate the level to reflect the changes
                GenerateLevel();
            }
        }

        public void ShowItemIdMappingSummary()
        {
            string summary = ItemIdSwapHelper.GetMappingSummary();
            PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = summary });
        }

        private void ShowRightPanel(GameObject go)
        {
            uiToolGrill.gameObject.SetActive(go == uiToolGrill.gameObject);
            uiToolConveyor.gameObject.SetActive(go == uiToolConveyor.gameObject);
            uiToolOrder.gameObject.SetActive(go == uiToolOrder.gameObject);
            // uiToolSheetData is always visible, not part of the right panel system
        }

        public void NextLayer()
        {
            if (int.TryParse(txtLayer.text, out int layer))
            {
                layer = layer + 1;
                txtLayer.text = layer.ToString();
                SetLayer(layer);
            }
        }

        public void PreviousLayer()
        {
            if (int.TryParse(txtLayer.text, out int layer))
            {
                if (layer == 0) return;
                layer = layer - 1;
                txtLayer.text = layer.ToString();
                SetLayer(layer);
            }
        }

        private void UpdateLayer(string layerStr)
        {
            if (int.TryParse(layerStr, out int layer))
            {
                SetLayer(layer);
            }
        }

        private void SetLayer(int layer)
        {
            ToolManager.Instance.SetMainLayer(layer);
            uiToolGrill.UpdateLayerMain();
        }

        public static bool loadingScene = false;

        public void PlayMode()
        {
            LevelGenerator.levelService = levelService;
            SonatUtils.DelayCall(2, () => { GameRemoteConfigValue.levelForceHome = 999999; });

            if (int.TryParse(txtLevel.text, out int level))
            {
                MySonatFramework.GetService<UserDataService>().SaveLevel(level);
            }

            Screen.SetResolution(480, 920, false);
            if (!loadingScene)
            {
                MySonatFramework.GetService<SceneService>().SwitchScene(GamePlacement.PreLoading, true);
            }
            else
            {
                MySonatFramework.GetService<SceneService>().SwitchScene(GamePlacement.Gameplay, true);
            }
        }

        public LevelType GetLevelType()
        {
            return levelTypeDropdown.GetLevelTypeOpt();
        }

        public LevelMode GetLevelMode()
        {
            return levelModeDropdown.GetLevelModeOpt();
        }

        public void SetToggleLockPosition(bool active)
        {
            if (lockPosition.gameObject.activeSelf == active) return;
            lockPosition.gameObject.SetActive(active);

            if (active)
            {
                bool isLock = true;
                foreach (var grill in ToolManager.Instance.toolGrillSelector.SelectedGrills)
                {
                    if (!grill.lockPosition)
                    {
                        isLock = false;
                        break;
                    }
                }

                lockPosition.SetIsOnWithoutNotify(isLock);
            }
        }

        public void SetLockPosition(bool lockPosition)
        {
            if (ToolManager.Instance.toolGrillSelector.SelectedGrills.Count == 0) return;
            foreach (var grill in ToolManager.Instance.toolGrillSelector.SelectedGrills)
            {
                grill.lockPosition = lockPosition;
            }
        }

        public void SwitchGrillType(ToolGrill toolGrill, GrillType newType)
        {
            var oldType = toolGrill.GrillData.grillType;
            var oldData = toolGrill.GrillData;

            // Store the old data for undo
            ToolManager.Instance.undoController.AddGrillOperation(oldData, GrillOperationType.Switch, oldType);

            // Create new grill data with the new type
            var newGrillData = CreateGrillData(newType, oldData);
            newGrillData.grillType = newType;

            // Remove old grill and add new one
            levelData.grillData.Remove(oldData);
            levelData.grillData.Add(newGrillData);

            // Update the tool grill
            toolGrill.SetData(newGrillData);
            OnSelectToolGrill(toolGrill);
            UpdateItemCount();
        }

        /// <summary>
        /// Takes a screenshot of the current level and saves it to the level folder
        /// </summary>
        /// <param name="levelNumber">The level number to save</param>
        private void TakeLevelScreenshot(int levelNumber)
        {
            if (screenshotTool == null)
            {
                // Try to find the screenshot tool if not assigned
                screenshotTool = FindObjectOfType<LevelScreenshotTool>();
                if (screenshotTool == null)
                {
                    Debug.LogWarning("LevelScreenshotTool not found. Screenshot will not be taken.");
                    return;
                }
            }

            try
            {
                string screenshotPath = screenshotTool.TakeLevelScreenshot(levelNumber, txtLevelFolder.text);
                if (!string.IsNullOrEmpty(screenshotPath))
                {
                    Debug.Log($"Level screenshot saved: {screenshotPath}");
                }
                else
                {
                    Debug.LogWarning("Failed to take level screenshot");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error taking level screenshot: {ex.Message}");
            }
        }

        /// <summary>
        /// Takes a screenshot immediately (for manual use)
        /// </summary>
        public void TakeScreenshotNow()
        {
            if (screenshotTool == null)
            {
                screenshotTool = FindObjectOfType<LevelScreenshotTool>();
                if (screenshotTool == null)
                {
                    PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = "Screenshot tool not found!" });
                    return;
                }
            }

            string screenshotPath = screenshotTool.TakeScreenshotNow();
            if (!string.IsNullOrEmpty(screenshotPath))
            {
                PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = $"Screenshot saved: {screenshotPath}" });
            }
            else
            {
                PanelManager.Instance.OpenForget<NotifyPanel>(new NotifyPanel.Data() { content = "Failed to take screenshot" });
            }
        }

        private void OnChangeDropLevelToggle(bool isOn)
        {
            if (levelData != null)
            {
                levelData.isDropMode = isOn;
            }
        }

        private List<DropColumnData> ValidateDropColumnData(List<DropColumnData> dropColumnData)
        {
            if (dropColumnData == null) return null;
            return dropColumnData.Where(d => d.grillIds != null && d.grillIds.Count > 0).ToList();
        }
    }
}