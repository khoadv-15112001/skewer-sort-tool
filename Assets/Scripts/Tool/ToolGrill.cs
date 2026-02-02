using System;
using System.Collections.Generic;
using Gameplay.LevelData;
using Manager;
using Sonat.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace Tool
{
    public class ToolGrill : MonoBehaviour
    {
        private GrillData grillData;
        public GrillData GrillData => grillData;
        public GrillType grillType;
        [SerializeField] private ToolItem[] toolItems;
        [SerializeField] private SpriteRenderer visual;
        [SerializeField] private SpriteRenderer subVisual;
        public bool lockPosition = false;
        [SerializeField] private string grillVisualName;
        public int NumberSlot => toolItems.Length;
        [SerializeField] private bool ignoreVisual;

        public virtual void SetData(GrillData grillData)
        {
            this.grillData = grillData;
            transform.position = grillData.position.ToVector3();
            UpdateVisual();

            SetVisual();
            // if (this.grillData.isLock)
            // {
            //     var obstacle = ToolManager.Instance.CreateObstacle(ObstacleType.Lock, grillData.id);
            //     obstacle.SetData(this);
            // }
        }

        protected virtual void SetVisual()
        {
            if (ignoreVisual) return;
            if (UIToolPanel.Instance.GetLevelType() is LevelType.Cake or LevelType.Fruit)
            {
                string visualName = string.IsNullOrEmpty(grillVisualName) ? "Fruit" : $"Fruit_{grillVisualName}";
                _ = visual.SetSpriteAsync(PathManager.TraySprite(visualName));
            }
            else if (grillType == GrillType.Overcooked)
            {
                string visualName = "Overcook";
                _ = visual.SetSpriteAsync(PathManager.TraySprite(visualName));
            }
            else
            {
                string visualName = string.IsNullOrEmpty(grillVisualName) ? "Normal" : $"Normal_{grillVisualName}";
                _ = visual.SetSpriteAsync(PathManager.TraySprite(visualName));
            }
        }

        public void OnSelectGrill()
        {
            UIToolPanel.Instance.OnSelectToolGrill(this);
        }

        public void OnGrillUpdate()
        {
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (grillData.layer == null || grillData.layer.Count == 0 || grillData.layer.Count <= ToolManager.currentLayer)
            {
                for (int i = 0; i < toolItems.Length; i++)
                {
                    toolItems[i].SetItemData(null);
                }
            }
            else
            {
                var layers = grillData.layer;
                var layerData = layers[ToolManager.currentLayer];
                for (int i = 0; i < toolItems.Length; i++)
                {
                    if (i < layerData.itemData.Length)
                    {
                        toolItems[i].SetItemData(layerData.itemData[i]);
                    }
                    else
                    {
                        toolItems[i].SetItemData(null);
                    }
                }
            }
        }

        private Vector3 clickOffset;
        private bool clicked = false;
        private bool dragged = false;
        private Vector3 lastClick;

        private void OnMouseDown()
        {
            if (!ToolManager.selectAvailable) return;
        
            if (Input.GetKey(KeyCode.D))
            {
                UIToolPanel.Instance.RemoveGrill(this);
                return;
            }
            // else if (Input.GetKey(KeyCode.L))
            // {
            //     UIToolPanel.Instance.AddObstacles(ObstacleType.Lock, this);
            //     return;
            // }
        
            // lastClick = Input.mousePosition;
            // OnSelectGrill();
            // clickOffset = transform.position - ToolManager.Instance.mainCamera.ScreenToWorldPoint(Input.mousePosition);
            // clickOffset.z = 0;
            // clicked = true;
            // dragged = false;
        }
        //
        // private void OnMouseDrag()
        // {
        //     if (!clicked || Vector3.Distance(Input.mousePosition, lastClick) < 0.75f || lockPosition) return;
        //     
        //     // Store the original position for undo on first drag
        //     if (!dragged)
        //     {
        //         var originalPos = new Vector3Data(transform.position);
        //         ToolManager.Instance.undoController.AddGrillOperation(grillData, GrillOperationType.Move, originalPos);
        //         dragged = true;
        //     }
        //     
        //     Vector3 target = DragPos();
        //     transform.position = target;
        // }
        //
        // private Vector3 DragPos()
        // {
        //     Vector3 pos = ToolManager.Instance.mainCamera.ScreenToWorldPoint(Input.mousePosition) + clickOffset;
        //     pos.z = 0;
        //     if (ToolManager.Instance.gridSnap == 0) return pos;
        //     float posX = Mathf.Round(pos.x / ToolManager.Instance.gridSnap) * ToolManager.Instance.gridSnap;
        //     float posY = Mathf.Round(pos.y / ToolManager.Instance.gridSnap) * ToolManager.Instance.gridSnap;
        //     pos = new Vector3(posX, posY, 0);
        //     return pos;
        // }
        //
        // private void OnMouseUp()
        // {
        //     clicked = false;
        //     UpdatePosition();
        //     ToolManager.Instance.OnGrillChanged?.Invoke(this);
        // }

        public void UpdatePosition()
        {
            grillData.position = new Vector3Data(transform.position);
        }

        public void UpdateLayerMain()
        {
            UpdateVisual();
        }

        public void ChangeMainVisual(Sprite sprite)
        {
            visual.sprite = sprite;
        }

        public void ChangSubVisual(Sprite sprite)
        {
            subVisual.sprite = sprite;
        }

        public virtual void SwitchItemId(int oldId, int newId)
        {
            bool update = false;
            if (grillData != null && grillData.layer != null)
            {
                foreach (var layerData in grillData.layer)
                {
                    if (layerData != null && layerData.itemData != null)
                    {
                        foreach (var itemData in layerData.itemData)
                        {
                            if (itemData != null && itemData.id == oldId)
                            {
                                itemData.id = newId;
                                update = true;
                            }
                        }
                    }
                }
            }

            if (update)
            {
                OnGrillUpdate();
            }
        }
    }
}