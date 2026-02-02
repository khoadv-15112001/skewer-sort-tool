using System.Collections.Generic;
using System.Linq;
using Gameplay.LevelData;
using MyGame.SkewerJam.Level;
using SonatFramework.Scripts.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace Tool
{
    public class ToolDropColumn : MonoBehaviour
    {

        [SerializeField] private Transform direction;
        [SerializeField] private Transform container;
        [SerializeField] private Transform bottomPointRef;
        [SerializeField] private float distanceBetweenGrill = 2.5f;
        public DropColumnData dropColumnData { get; private set; }

        // Start is called before the first frame update
        void Start()
        {
            ToolManager.Instance.OnGrillChanged += OnToolChanged;
        }

        void OnDestroy()
        {
            if (ToolManager.Instance != null)
                ToolManager.Instance.OnGrillChanged -= OnToolChanged;
        }

        public void SetData(DropColumnData dropColumnData)
        {
            this.dropColumnData = dropColumnData;


            if (dropColumnData.grillIds != null && dropColumnData.grillIds.Count > 0)
            {
                var grill = ToolManager.Instance.GetToolGrill(dropColumnData.grillIds[0]);
                transform.position = new Vector3(grill.transform.position.x, 0, 0);
                if (dropColumnData.grillIds != null && dropColumnData.grillIds.Count > 0)
                {
                    foreach (var grillId in dropColumnData.grillIds)
                    {
                        ToolGrill toolGrill = ToolManager.Instance.GetToolGrill(grillId);
                        toolGrill.transform.SetParent(container);
                        toolGrill.transform.SetLocalPositionZ(0);
                    }
                }
            }
        }

        private void OnToolChanged(ToolGrill toolGrill)
        {
            CheckGrill();
        }

        public void CheckGrill()
        {
            var allGrill = ToolManager.Instance.AllGrills;
            foreach (var toolGrill in allGrill)
            {
                if (toolGrill == null) continue;
                if (CheckGrillOnDropColumn(toolGrill))
                {
                    toolGrill.transform.parent = container;
                    toolGrill.transform.SetLocalPositionZ(0);
                    toolGrill.transform.SetLocalPositionX(0);

                    if (!dropColumnData.grillIds.Contains(toolGrill.GrillData.id))
                    {
                        dropColumnData.grillIds.Add(toolGrill.GrillData.id);
                    }
                }
                else
                {
                    if (CheckGrillPosition(toolGrill) == false && dropColumnData.grillIds.Contains(toolGrill.GrillData.id))
                    {
                        dropColumnData.grillIds.Remove(toolGrill.GrillData.id);
                        toolGrill.transform.parent = null;
                        toolGrill.transform.SetLocalPositionZ(0);
                    }
                }
            }

            SonatUtils.ExecuteNextFrame(() =>
            {
                UpdateGrillPosition();
                UpdatePosition();
            });
        }

        private bool CheckGrillOnDropColumn(ToolGrill toolGrill)
        {
            // nếu tool grill này đang không thuộc dropcolumn nào
            var listDropColumn = UIToolPanel.Instance.LevelData.listDropColumnData;
            if (listDropColumn.Any(x => x.grillIds.Contains(toolGrill.GrillData.id)))
            {
                return false;
            }
            return CheckGrillPosition(toolGrill);
        }

        private bool CheckGrillPosition(ToolGrill toolGrill)
        {
            return Mathf.Abs(transform.position.x - toolGrill.transform.position.x) < 1f;
        }


        private Vector3 clickOffset;
        private bool clicked = false;
        private Vector3 lastClick;

        private void OnMouseDown()
        {
            if (!ToolManager.selectAvailable) return;

            if (Input.GetKey(KeyCode.D))
            {
                UIToolPanel.Instance.RemoveDropColumn(this);
                return;
            }

            ToolManager.Instance.toolGrillSelector.SetBlockDraw(true);
            lastClick = Input.mousePosition;
            OnSelectDropColumn();
            clickOffset = transform.position - ToolManager.Instance.mainCamera.ScreenToWorldPoint(Input.mousePosition);
            clickOffset.z = 0;
            clicked = true;
        }

        public void OnSelectDropColumn()
        {
            UIToolPanel.Instance.OnSelectToolDropColumn(this);
        }

        private void OnMouseDrag()
        {
            if (!clicked || Vector3.Distance(Input.mousePosition, lastClick) < 0.75f) return;
            Vector3 target = DragPos();
            transform.position = target;
        }

        private Vector3 DragPos()
        {
            Vector3 pos = ToolManager.Instance.mainCamera.ScreenToWorldPoint(Input.mousePosition) + clickOffset;
            pos.z = 0;
            if (ToolManager.Instance.gridSnap == 0) return pos;
            float posX = Mathf.Round(pos.x / ToolManager.Instance.gridSnap) * ToolManager.Instance.gridSnap;
            float posY = 0;
            pos = new Vector3(posX, posY, 0);
            return pos;
        }

        private void OnMouseUp()
        {
            clicked = false;
            UpdatePosition();
            ToolManager.Instance.toolGrillSelector.SetBlockDraw(false);
            CheckGrill();
        }

        public void UpdatePosition()
        {
            foreach (var grillId in dropColumnData.grillIds.ToList())
            {
                var toolGrill = ToolManager.Instance.GetToolGrill(grillId);
                if (toolGrill == null)
                {
                    dropColumnData.grillIds.Remove(grillId);
                    continue;
                }

                toolGrill.UpdatePosition();
            }
        }

        public void UpdateGrillPosition()
        {
            var listDropColumn = UIToolPanel.Instance.LevelData.listDropColumnData;
            var maxCountGrillInDropColumn = listDropColumn.Max(x => x.grillIds.Count);

            dropColumnData.grillIds.Sort((a, b) =>
            {
                var toolGrillA = ToolManager.Instance.GetToolGrill(a);
                var toolGrillB = ToolManager.Instance.GetToolGrill(b);
                if (toolGrillA == null || toolGrillB == null) return 1;
                return toolGrillA.transform.position.y.CompareTo(toolGrillB.transform.position.y);
            });

            int index = 0;
            var startPos = new Vector3(0, -maxCountGrillInDropColumn * distanceBetweenGrill / 2.0f, 0);
            foreach (var grillId in dropColumnData.grillIds.ToList())
            {
                var toolGrill = ToolManager.Instance.GetToolGrill(grillId);
                if (toolGrill == null) continue;
                toolGrill.transform.parent = container;
                toolGrill.transform.localPosition = startPos + new Vector3(0, index * distanceBetweenGrill, 0);
                index++;
            }
        }
    }
}