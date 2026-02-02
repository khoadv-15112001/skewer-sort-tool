using System.Collections.Generic;
using Gameplay.LevelData;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Tool
{
    public class ToolGrillSelector : MonoBehaviour
    {
        [Header("Selection Settings")] [SerializeField]
        private LayerMask selectableLayerMask = -1;

        [SerializeField] private float dragThreshold = 5f; // Minimum distance to start drag selection

        [Header("Visual Feedback")] [SerializeField]
        private Color selectionBoxColor = new Color(0.2f, 0.6f, 1f, 0.3f);

        [SerializeField] private Color selectionBoxBorderColor = new Color(0.2f, 0.6f, 1f, 1f);
        [SerializeField] private float selectionBoxBorderThickness = 2f;
        [SerializeField] private Color selectedGrillHighlightColor = Color.yellow;
        [SerializeField] private Color finalSelectionBoxColor = Color.green; // Green color for final selection box

        [Header("References")] [SerializeField]
        private LineRenderer selectionBoxRenderer;

        [SerializeField] private LineRenderer finalSelectionBoxRenderer; // Separate renderer for final green box
        [SerializeField] private Material selectionBoxMaterial;

        // Selection state
        private List<ToolGrill> selectedGrills = new List<ToolGrill>();
        private List<ToolGrill> previouslySelectedGrills = new List<ToolGrill>();

        // Drag selection state
        private bool isDragging = false;
        private bool isMovingSelected = false; // New flag for moving selected objects
        private Vector3 dragStartPosition;
        private Vector3 dragCurrentPosition;
        private Vector3 dragStartWorldPosition; // World position when drag started
        private Vector3[] selectedGrillsOriginalPositions; // Store original positions for movement
        private Camera mainCamera;

        // Visual feedback
        private Texture2D selectionBoxTexture;
        private Texture2D selectionBoxBorderTexture;

        public List<ToolGrill> SelectedGrills => selectedGrills;
        private bool blockDraw;

        private void Start()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
                mainCamera = FindObjectOfType<Camera>();

            InitializeSelectionBoxRenderer();
            CreateSelectionTextures();
        }

        private void InitializeSelectionBoxRenderer()
        {
            // Initialize drag selection box renderer
            if (selectionBoxRenderer == null)
            {
                GameObject lineRendererObj = new GameObject("SelectionBoxRenderer");
                lineRendererObj.transform.SetParent(transform);
                selectionBoxRenderer = lineRendererObj.AddComponent<LineRenderer>();
            }

            selectionBoxRenderer.material = selectionBoxMaterial != null ? selectionBoxMaterial : new Material(Shader.Find("Sprites/Default"));
            selectionBoxRenderer.startWidth = 0.05f;
            selectionBoxRenderer.endWidth = 0.05f;
            selectionBoxRenderer.startColor = selectionBoxBorderColor;
            selectionBoxRenderer.endColor = selectionBoxBorderColor;
            selectionBoxRenderer.positionCount = 5; // 4 corners + back to start
            selectionBoxRenderer.useWorldSpace = true;
            selectionBoxRenderer.enabled = false;

            // Initialize final selection box renderer (green box)
            if (finalSelectionBoxRenderer == null)
            {
                GameObject finalLineRendererObj = new GameObject("FinalSelectionBoxRenderer");
                finalLineRendererObj.transform.SetParent(transform);
                finalSelectionBoxRenderer = finalLineRendererObj.AddComponent<LineRenderer>();
            }

            finalSelectionBoxRenderer.material = selectionBoxMaterial != null ? selectionBoxMaterial : new Material(Shader.Find("Sprites/Default"));
            finalSelectionBoxRenderer.startWidth = 0.08f; // Slightly thicker for final selection
            finalSelectionBoxRenderer.endWidth = 0.08f;
            finalSelectionBoxRenderer.startColor = finalSelectionBoxColor;
            finalSelectionBoxRenderer.endColor = finalSelectionBoxColor;
            finalSelectionBoxRenderer.positionCount = 5; // 4 corners + back to start
            finalSelectionBoxRenderer.useWorldSpace = true;
            finalSelectionBoxRenderer.enabled = false;
        }

        private void CreateSelectionTextures()
        {
            // Create filler texture
            selectionBoxTexture = new Texture2D(1, 1);
            selectionBoxTexture.SetPixel(0, 0, selectionBoxColor);
            selectionBoxTexture.Apply();
            selectionBoxTexture.wrapMode = TextureWrapMode.Clamp;
            selectionBoxTexture.filterMode = FilterMode.Point;

            // Create border texture
            selectionBoxBorderTexture = new Texture2D(1, 1);
            selectionBoxBorderTexture.SetPixel(0, 0, selectionBoxBorderColor);
            selectionBoxBorderTexture.Apply();
            selectionBoxBorderTexture.wrapMode = TextureWrapMode.Clamp;
            selectionBoxBorderTexture.filterMode = FilterMode.Point;
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            // Check if mouse is over UI - if so, don't process selection
            if (IsPointerOverUI())
            {
                return;
            }

            // Single click selection
            if (Input.GetMouseButtonDown(0))
            {
                dragStartPosition = Input.mousePosition;
                dragStartWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                dragStartWorldPosition.z = 0;
                isDragging = false;
                isMovingSelected = false;
                // Check if we clicked on a ToolGrill
                ToolGrill clickedGrill = GetGrillAtMousePosition();
                if (clickedGrill != null)
                {
                    HandleSingleClickSelection(clickedGrill);
                }
                else
                {
                    // Check if we clicked within the selection box area
                    if (selectedGrills.Count > 0 && IsClickInSelectionBox(dragStartWorldPosition))
                    {
                        // Don't clear selection, just prepare for movement
                        Debug.Log("Clicked within selection box - preparing for movement");
                    }
                    else if (selectedGrills.Count > 0)
                    {
                        // Clicked outside selection box and outside any grill - clear selection
                        ClearSelection();
                        Debug.Log("Clicked outside selection box - cleared selection");
                    }
                }
            }

            // Drag selection or movement
            if (Input.GetMouseButton(0))
            {
                if (blockDraw)
                {
                    if (isDragging)
                    {
                        isDragging = false;
                        isMovingSelected = false;
                        ClearSelection();
                    }
                    return;
                }

                dragCurrentPosition = Input.mousePosition;
                float dragDistance = Vector3.Distance(dragStartPosition, dragCurrentPosition);

                if (!isDragging && dragDistance > dragThreshold)
                {
                    isDragging = true;

                    // Check if we're clicking on a selected grill or within selection box - if so, move all selected
                    ToolGrill clickedGrill = GetGrillAtMousePosition();
                    if ((clickedGrill != null && selectedGrills.Contains(clickedGrill)) ||
                        (selectedGrills.Count > 0 && IsClickInSelectionBox(dragStartWorldPosition)))
                    {
                        isMovingSelected = true;
                        StartMovingSelected();
                    }
                    else
                    {
                        StartDragSelection();
                    }
                }

                if (isDragging)
                {
                    if (isMovingSelected)
                    {
                        UpdateMovingSelected();
                    }
                    else
                    {
                        UpdateDragSelection();
                    }
                }
            }

            // End drag selection or movement
            if (Input.GetMouseButtonUp(0))
            {
                if (isDragging)
                {
                    if (isMovingSelected)
                    {
                        EndMovingSelected();
                    }
                    else
                    {
                        EndDragSelection();
                    }
                }

                isDragging = false;
                isMovingSelected = false;
                blockDraw = false;
            }

            // Clear selection with right click or escape
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                ClearSelection();
            }
        }

        private ToolGrill GetGrillAtMousePosition()
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;

            Collider2D hitCollider = Physics2D.OverlapPoint(mouseWorldPos, selectableLayerMask);
            if (hitCollider != null)
            {
                return hitCollider.GetComponent<ToolGrill>();
            }

            return null;
        }

        private void HandleSingleClickSelection(ToolGrill clickedGrill)
        {
            // If holding Ctrl/Shift, add to selection
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ||
                Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                if (selectedGrills.Contains(clickedGrill))
                {
                    // Remove from selection
                    selectedGrills.Remove(clickedGrill);
                    UpdateGrillVisual(clickedGrill, false);
                }
                else
                {
                    // Add to selection
                    selectedGrills.Add(clickedGrill);
                    UpdateGrillVisual(clickedGrill, true);
                }
            }
            else
            {
                // Check if the clicked grill is already selected
                if (selectedGrills.Contains(clickedGrill))
                {
                    // If clicking on an already selected grill, don't clear selection
                    // This allows for movement without clearing
                    Debug.Log("Clicked on already selected grill - keeping selection for movement");
                }
                else
                {
                    // Clear previous selection and select only this one
                    ClearSelection();
                    selectedGrills.Add(clickedGrill);
                    UpdateGrillVisual(clickedGrill, true);
                    UIToolPanel.Instance.OnSelectToolGrill(clickedGrill);
                }
            }

            UpdateSelectionBox();
            UpdateFinalSelectionBox(); // Update green box for single click selection too
        }

        private void StartDragSelection()
        {
            // Clear previous selection if not holding Ctrl/Shift
            if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl) &&
                !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
            {
                ClearSelection();
            }
        }

        private void UpdateDragSelection()
        {
            // Update visual selection box
            DrawSelectionBox();

            // Update selected objects
            UpdateDragSelectedObjects();
        }

        private void EndDragSelection()
        {
            // Final update of selected objects
            UpdateDragSelectedObjects();

            // Hide drag selection box
            selectionBoxRenderer.enabled = false;

            // Show final green selection box if we have selected objects
            if (selectedGrills.Count > 0)
            {
                UpdateFinalSelectionBox();
            }
        }

        private void StartMovingSelected()
        {
            // Store original positions of all selected grills
            selectedGrillsOriginalPositions = new Vector3[selectedGrills.Count];
            for (int i = 0; i < selectedGrills.Count; i++)
            {
                if (selectedGrills[i] != null)
                {
                    ToolManager.Instance.undoController.AddGrillOperation(selectedGrills[i].GrillData, GrillOperationType.Move,
                        new Vector3Data(selectedGrills[i].transform.position));
                    selectedGrillsOriginalPositions[i] = selectedGrills[i].transform.position;
                }
            }

            // Hide the blue selection box during movement
            selectionBoxRenderer.enabled = false;

            Debug.Log($"Started moving {selectedGrills.Count} selected grill(s)");
        }

        private void UpdateMovingSelected()
        {
            if (selectedGrills.Count == 0) return;

            // Calculate the movement offset
            Vector3 currentWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            currentWorldPos.z = 0;
            Vector3 movementOffset = currentWorldPos - dragStartWorldPosition;

            // Apply grid snapping if enabled
            if (ToolManager.Instance.gridSnap > 0)
            {
                movementOffset.x = Mathf.Round(movementOffset.x / ToolManager.Instance.gridSnap) * ToolManager.Instance.gridSnap;
                movementOffset.y = Mathf.Round(movementOffset.y / ToolManager.Instance.gridSnap) * ToolManager.Instance.gridSnap;
            }

            // Move all selected grills
            for (int i = 0; i < selectedGrills.Count; i++)
            {
                if (selectedGrills[i] != null && !selectedGrills[i].lockPosition && i < selectedGrillsOriginalPositions.Length)
                {
                    Vector3 newPosition = selectedGrillsOriginalPositions[i] + movementOffset;
                    selectedGrills[i].transform.position = newPosition;
                }
            }

            // Update the selection box to follow the moved objects
            UpdateFinalSelectionBox();
        }

        private void EndMovingSelected()
        {
            // Update positions in the grill data
            foreach (ToolGrill grill in selectedGrills)
            {
                if (grill != null)
                {
                    grill.UpdatePosition();
                    ToolManager.Instance.OnGrillChanged?.Invoke(grill);
                }
            }

            // Ensure the green selection box is visible after movement
            UpdateFinalSelectionBox();

            Debug.Log($"Finished moving {selectedGrills.Count} selected grill(s)");
        }

        private void UpdateDragSelectedObjects()
        {
            Vector3 startWorldPos = mainCamera.ScreenToWorldPoint(dragStartPosition);
            Vector3 endWorldPos = mainCamera.ScreenToWorldPoint(dragCurrentPosition);
            startWorldPos.z = 0;
            endWorldPos.z = 0;

            // Create selection bounds
            Bounds selectionBounds = new Bounds();
            selectionBounds.SetMinMax(
                new Vector3(Mathf.Min(startWorldPos.x, endWorldPos.x), Mathf.Min(startWorldPos.y, endWorldPos.y), 0),
                new Vector3(Mathf.Max(startWorldPos.x, endWorldPos.x), Mathf.Max(startWorldPos.y, endWorldPos.y), 0)
            );

            // Find all grills in the selection area using complete visual bounds
            List<ToolGrill> grillsInArea = new List<ToolGrill>();
            foreach (ToolGrill grill in ToolManager.Instance.AllGrills)
            {
                if (grill != null)
                {
                    // Check if the grill's complete visual bounds intersect with the selection area
                    Renderer[] renderers = grill.GetComponentsInChildren<Renderer>();

                    if (renderers.Length > 0)
                    {
                        // Calculate the complete visual bounds
                        Bounds totalBounds = renderers[0].bounds;
                        for (int i = 1; i < renderers.Length; i++)
                        {
                            totalBounds.Encapsulate(renderers[i].bounds);
                        }

                        if (selectionBounds.Intersects(totalBounds))
                        {
                            grillsInArea.Add(grill);
                        }
                    }
                    else
                    {
                        // Fallback to transform position if no renderers
                        if (selectionBounds.Contains(grill.transform.position))
                        {
                            grillsInArea.Add(grill);
                        }
                    }
                }
            }

            // Update selection
            foreach (ToolGrill grill in grillsInArea)
            {
                if (!selectedGrills.Contains(grill))
                {
                    selectedGrills.Add(grill);
                    UpdateGrillVisual(grill, true);
                }
            }

            UpdateSelectionBox();
        }

        private void DrawSelectionBox()
        {
            Vector3 startWorldPos = mainCamera.ScreenToWorldPoint(dragStartPosition);
            Vector3 endWorldPos = mainCamera.ScreenToWorldPoint(dragCurrentPosition);
            startWorldPos.z = 0;
            endWorldPos.z = 0;

            // Calculate corners
            Vector3 bottomLeft = new Vector3(Mathf.Min(startWorldPos.x, endWorldPos.x), Mathf.Min(startWorldPos.y, endWorldPos.y), 0);
            Vector3 topRight = new Vector3(Mathf.Max(startWorldPos.x, endWorldPos.x), Mathf.Max(startWorldPos.y, endWorldPos.y), 0);
            Vector3 topLeft = new Vector3(bottomLeft.x, topRight.y, 0);
            Vector3 bottomRight = new Vector3(topRight.x, bottomLeft.y, 0);

            // Set line renderer positions
            selectionBoxRenderer.SetPosition(0, bottomLeft);
            selectionBoxRenderer.SetPosition(1, topLeft);
            selectionBoxRenderer.SetPosition(2, topRight);
            selectionBoxRenderer.SetPosition(3, bottomRight);
            selectionBoxRenderer.SetPosition(4, bottomLeft);

            selectionBoxRenderer.enabled = true;
        }

        private void UpdateSelectionBox()
        {
            if (selectedGrills.Count == 0)
            {
                selectionBoxRenderer.enabled = false;
                finalSelectionBoxRenderer.enabled = false;
                UIToolPanel.Instance.SetToggleLockPosition(false);
                return;
            }

            // Calculate bounds of all selected grills
            Bounds selectionBounds = CalculateSelectionBounds();

            if (selectionBounds.size != Vector3.zero)
            {
                // Expand bounds slightly for visual padding
                selectionBounds.Expand(0.5f);

                // Set line renderer positions around the bounds
                Vector3 min = selectionBounds.min;
                Vector3 max = selectionBounds.max;

                selectionBoxRenderer.SetPosition(0, new Vector3(min.x, min.y, 0));
                selectionBoxRenderer.SetPosition(1, new Vector3(min.x, max.y, 0));
                selectionBoxRenderer.SetPosition(2, new Vector3(max.x, max.y, 0));
                selectionBoxRenderer.SetPosition(3, new Vector3(max.x, min.y, 0));
                selectionBoxRenderer.SetPosition(4, new Vector3(min.x, min.y, 0));

                selectionBoxRenderer.enabled = true;
            }

            UIToolPanel.Instance.SetToggleLockPosition(true);
        }

        public void UpdateFinalSelectionBox()
        {
            if (selectedGrills.Count == 0)
            {
                finalSelectionBoxRenderer.enabled = false;
                return;
            }

            // Calculate bounds of all selected grills
            Bounds selectionBounds = CalculateSelectionBounds();

            if (selectionBounds.size != Vector3.zero)
            {
                // Expand bounds slightly for visual padding
                selectionBounds.Expand(0.5f);

                // Set final selection box positions around the bounds
                Vector3 min = selectionBounds.min;
                Vector3 max = selectionBounds.max;

                finalSelectionBoxRenderer.SetPosition(0, new Vector3(min.x, min.y, 0));
                finalSelectionBoxRenderer.SetPosition(1, new Vector3(min.x, max.y, 0));
                finalSelectionBoxRenderer.SetPosition(2, new Vector3(max.x, max.y, 0));
                finalSelectionBoxRenderer.SetPosition(3, new Vector3(max.x, min.y, 0));
                finalSelectionBoxRenderer.SetPosition(4, new Vector3(min.x, min.y, 0));

                finalSelectionBoxRenderer.enabled = true;
            }
        }

        private Bounds CalculateSelectionBounds()
        {
            Bounds selectionBounds = new Bounds();
            bool firstGrill = true;

            foreach (ToolGrill grill in selectedGrills)
            {
                if (grill != null)
                {
                    // Get all renderers to calculate the complete visual bounds
                    Renderer[] renderers = grill.GetComponentsInChildren<Renderer>();

                    if (renderers.Length > 0)
                    {
                        // Use the first renderer as the base bounds
                        Bounds totalBounds = renderers[0].bounds;

                        // Encapsulate all other renderers to get the complete visual bounds
                        for (int i = 1; i < renderers.Length; i++)
                        {
                            totalBounds.Encapsulate(renderers[i].bounds);
                        }

                        if (firstGrill)
                        {
                            selectionBounds = totalBounds;
                            firstGrill = false;
                        }
                        else
                        {
                            selectionBounds.Encapsulate(totalBounds);
                        }
                    }
                    else
                    {
                        // Fallback to transform position if no renderers
                        if (firstGrill)
                        {
                            selectionBounds = new Bounds(grill.transform.position, Vector3.zero);
                            firstGrill = false;
                        }
                        else
                        {
                            selectionBounds.Encapsulate(grill.transform.position);
                        }
                    }
                }
            }

            return selectionBounds;
        }

        private bool IsClickInSelectionBox(Vector3 worldPosition)
        {
            if (selectedGrills.Count == 0) return false;

            Bounds selectionBounds = CalculateSelectionBounds();
            if (selectionBounds.size == Vector3.zero) return false;

            // Expand bounds slightly to make it easier to click within the selection area
            selectionBounds.Expand(0.5f);

            return selectionBounds.Contains(worldPosition);
        }

        /// <summary>
        /// Check if the mouse pointer is over any UI element
        /// </summary>
        public bool IsPointerOverUI()
        {
            // Check if we're over a UI element using EventSystem
            if (EventSystem.current != null)
            {
                return EventSystem.current.IsPointerOverGameObject();
            }

            // Fallback: check if we're over any UI element using raycast
            return IsPointerOverUIWithRaycast();
        }

        /// <summary>
        /// Fallback method to check if pointer is over UI using raycast
        /// </summary>
        private bool IsPointerOverUIWithRaycast()
        {
            // Create a ray from the mouse position
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null)
            {
                // Check if the hit object is a UI element
                Canvas canvas = hit.collider.GetComponent<Canvas>();
                if (canvas != null)
                {
                    return true;
                }

                // Check if it's a UI element by looking for Canvas components in parents
                Canvas parentCanvas = hit.collider.GetComponentInParent<Canvas>();
                if (parentCanvas != null)
                {
                    return true;
                }
            }

            return false;
        }

        private void UpdateGrillVisual(ToolGrill grill, bool isSelected)
        {
            if (grill == null) return;

            // You can add visual feedback here, such as:
            // - Changing the grill's material color
            // - Adding a highlight component
            // - Scaling the grill slightly

            // For now, we'll just log the selection state
            if (isSelected)
            {
                Debug.Log($"Selected grill: {grill.name}");
            }
            else
            {
                Debug.Log($"Deselected grill: {grill.name}");
            }
        }

        public void ClearSelection()
        {
            // Remove visual feedback from previously selected grills
            foreach (ToolGrill grill in selectedGrills)
            {
                if (grill != null)
                {
                    UpdateGrillVisual(grill, false);
                }
            }

            selectedGrills.Clear();
            selectionBoxRenderer.enabled = false;
            finalSelectionBoxRenderer.enabled = false; // Hide green box when clearing selection
            UIToolPanel.Instance.SetToggleLockPosition(false);
        }

        public void SelectGrill(ToolGrill grill)
        {
            if (grill != null && !selectedGrills.Contains(grill))
            {
                selectedGrills.Add(grill);
                UpdateGrillVisual(grill, true);
                UpdateSelectionBox();
                UpdateFinalSelectionBox(); // Update green box when programmatically selecting
            }
        }

        public void DeselectGrill(ToolGrill grill)
        {
            if (grill != null && selectedGrills.Contains(grill))
            {
                selectedGrills.Remove(grill);
                UpdateGrillVisual(grill, false);
                UpdateSelectionBox();
                UpdateFinalSelectionBox(); // Update green box when deselecting
            }
        }

        public bool IsGrillSelected(ToolGrill grill)
        {
            return selectedGrills.Contains(grill);
        }

        private void OnGUI()
        {
            // Draw selection box overlay during drag selection (not during movement)
            if (isDragging && !isMovingSelected)
            {
                DrawSelectionBoxOverlay();
            }
        }

        private void DrawSelectionBoxOverlay()
        {
            Vector2 startScreenPos = dragStartPosition;
            Vector2 endScreenPos = dragCurrentPosition;

            // Calculate rectangle
            float x = Mathf.Min(startScreenPos.x, endScreenPos.x);
            float y = Mathf.Min(startScreenPos.y, endScreenPos.y);
            float width = Mathf.Abs(endScreenPos.x - startScreenPos.x);
            float height = Mathf.Abs(endScreenPos.y - startScreenPos.y);

            // Convert to GUI coordinates (Y is inverted)
            y = Screen.height - y - height;

            // Draw filler
            GUI.DrawTexture(new Rect(x, y, width, height), selectionBoxTexture);

            // Draw border
            GUI.DrawTexture(new Rect(x, y, width, selectionBoxBorderThickness), selectionBoxBorderTexture); // Top
            GUI.DrawTexture(new Rect(x, y + height - selectionBoxBorderThickness, width, selectionBoxBorderThickness), selectionBoxBorderTexture); // Bottom
            GUI.DrawTexture(new Rect(x, y, selectionBoxBorderThickness, height), selectionBoxBorderTexture); // Left
            GUI.DrawTexture(new Rect(x + width - selectionBoxBorderThickness, y, selectionBoxBorderThickness, height), selectionBoxBorderTexture); // Right
        }

        private void OnDestroy()
        {
            if (selectionBoxTexture != null)
                DestroyImmediate(selectionBoxTexture);
            if (selectionBoxBorderTexture != null)
                DestroyImmediate(selectionBoxBorderTexture);
        }

        public void SetBlockDraw(bool blockDraw)
        {
            this.blockDraw = blockDraw;
        }
    }
}