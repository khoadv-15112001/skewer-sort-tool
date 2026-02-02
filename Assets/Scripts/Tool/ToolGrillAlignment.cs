using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tool
{
    /// <summary>
    /// Provides alignment functions for selected ToolGrill objects
    /// Integrates with the existing Aligns system
    /// </summary>
    public class ToolGrillAlignment : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ToolGrillSelector grillSelector;
        [SerializeField] private Aligns alignsSystem;
        
        [Header("Alignment Settings")]
        [SerializeField] private KeyCode alignLeftKey = KeyCode.Alpha1;
        [SerializeField] private KeyCode alignCenterKey = KeyCode.Alpha2;
        [SerializeField] private KeyCode alignRightKey = KeyCode.Alpha3;
        [SerializeField] private KeyCode alignTopKey = KeyCode.Alpha4;
        [SerializeField] private KeyCode alignMiddleKey = KeyCode.Alpha5;
        [SerializeField] private KeyCode alignBottomKey = KeyCode.Alpha6;
        [SerializeField] private KeyCode distributeHorizontallyKey = KeyCode.Alpha7;
        [SerializeField] private KeyCode distributeVerticallyKey = KeyCode.Alpha8;
        
        private void Start()
        {
            // Find references if not assigned
            if (grillSelector == null)
                grillSelector = FindObjectOfType<ToolGrillSelector>();
            
            if (alignsSystem == null)
                alignsSystem = FindObjectOfType<Aligns>();
        }
        
        private void Update()
        {
            HandleAlignmentInput();
        }
        
        private void HandleAlignmentInput()
        {
            if (grillSelector == null || alignsSystem == null) return;
            
            List<ToolGrill> selectedGrills = grillSelector.SelectedGrills;
            if (selectedGrills.Count < 2) return; // Need at least 2 objects to align
            
            // Check for alignment key presses
            if (Input.GetKeyDown(alignLeftKey))
            {
                AlignSelectedToLeft();
            }
            else if (Input.GetKeyDown(alignCenterKey))
            {
                AlignSelectedHorizontally();
            }
            else if (Input.GetKeyDown(alignRightKey))
            {
                AlignSelectedToRight();
            }
            else if (Input.GetKeyDown(alignTopKey))
            {
                AlignSelectedToTop();
            }
            else if (Input.GetKeyDown(alignMiddleKey))
            {
                AlignSelectedVertically();
            }
            else if (Input.GetKeyDown(alignBottomKey))
            {
                AlignSelectedToBottom();
            }
            else if (Input.GetKeyDown(distributeHorizontallyKey))
            {
                DistributeSelectedHorizontally();
            }
            else if (Input.GetKeyDown(distributeVerticallyKey))
            {
                DistributeSelectedVertically();
            }
        }
        
        /// <summary>
        /// Align selected grills to the left edge of their combined bounds
        /// </summary>
        public void AlignSelectedToLeft()
        {
            if (!ValidateSelection()) return;
            
            List<ToolGrill> selectedGrills = grillSelector.SelectedGrills;
            Bounds combinedBounds = CalculateCombinedBounds(selectedGrills);
            List<GameObject> grillObjects = ConvertToGameObjects(selectedGrills);
            
            alignsSystem.AlignObjectsToLeftBounds(grillObjects, combinedBounds);
            UpdateGrillPositions(selectedGrills);
            
            Debug.Log($"Aligned {selectedGrills.Count} grills to left");
        }
        
        /// <summary>
        /// Align selected grills to the center horizontally
        /// </summary>
        public void AlignSelectedHorizontally()
        {
            if (!ValidateSelection()) return;
            
            List<ToolGrill> selectedGrills = grillSelector.SelectedGrills;
            Bounds combinedBounds = CalculateCombinedBounds(selectedGrills);
            List<GameObject> grillObjects = ConvertToGameObjects(selectedGrills);
            
            alignsSystem.AlignObjectsHorizontally(grillObjects, combinedBounds);
            UpdateGrillPositions(selectedGrills);
            
            Debug.Log($"Aligned {selectedGrills.Count} grills horizontally to center");
        }
        
        /// <summary>
        /// Align selected grills to the right edge of their combined bounds
        /// </summary>
        public void AlignSelectedToRight()
        {
            if (!ValidateSelection()) return;
            
            List<ToolGrill> selectedGrills = grillSelector.SelectedGrills;
            Bounds combinedBounds = CalculateCombinedBounds(selectedGrills);
            List<GameObject> grillObjects = ConvertToGameObjects(selectedGrills);
            
            alignsSystem.AlignObjectsToRightBounds(grillObjects, combinedBounds);
            UpdateGrillPositions(selectedGrills);
            
            Debug.Log($"Aligned {selectedGrills.Count} grills to right");
        }
        
        /// <summary>
        /// Align selected grills to the top edge of their combined bounds
        /// </summary>
        public void AlignSelectedToTop()
        {
            if (!ValidateSelection()) return;
            
            List<ToolGrill> selectedGrills = grillSelector.SelectedGrills;
            Bounds combinedBounds = CalculateCombinedBounds(selectedGrills);
            List<GameObject> grillObjects = ConvertToGameObjects(selectedGrills);
            
            alignsSystem.AlignObjectsToTopBounds(grillObjects, combinedBounds);
            UpdateGrillPositions(selectedGrills);
            
            Debug.Log($"Aligned {selectedGrills.Count} grills to top");
        }
        
        /// <summary>
        /// Align selected grills to the center vertically
        /// </summary>
        public void AlignSelectedVertically()
        {
            if (!ValidateSelection()) return;
            
            List<ToolGrill> selectedGrills = grillSelector.SelectedGrills;
            Bounds combinedBounds = CalculateCombinedBounds(selectedGrills);
            List<GameObject> grillObjects = ConvertToGameObjects(selectedGrills);
            
            alignsSystem.AlignObjectsVertically(grillObjects, combinedBounds);
            UpdateGrillPositions(selectedGrills);
            
            Debug.Log($"Aligned {selectedGrills.Count} grills vertically to center");
        }
        
        /// <summary>
        /// Align selected grills to the bottom edge of their combined bounds
        /// </summary>
        public void AlignSelectedToBottom()
        {
            if (!ValidateSelection()) return;
            
            List<ToolGrill> selectedGrills = grillSelector.SelectedGrills;
            Bounds combinedBounds = CalculateCombinedBounds(selectedGrills);
            List<GameObject> grillObjects = ConvertToGameObjects(selectedGrills);
            
            alignsSystem.AlignObjectsToBottomBounds(grillObjects, combinedBounds);
            UpdateGrillPositions(selectedGrills);
            
            Debug.Log($"Aligned {selectedGrills.Count} grills to bottom");
        }
        
        /// <summary>
        /// Distribute selected grills horizontally within their combined bounds
        /// </summary>
        public void DistributeSelectedHorizontally()
        {
            if (!ValidateSelection()) return;
            
            List<ToolGrill> selectedGrills = grillSelector.SelectedGrills;
            Bounds combinedBounds = CalculateCombinedBounds(selectedGrills);
            List<GameObject> grillObjects = ConvertToGameObjects(selectedGrills);
            
            alignsSystem.DistributeObjectsAlongAxis(grillObjects, combinedBounds, true);
            UpdateGrillPositions(selectedGrills);
            
            Debug.Log($"Distributed {selectedGrills.Count} grills horizontally");
        }
        
        /// <summary>
        /// Distribute selected grills vertically within their combined bounds
        /// </summary>
        public void DistributeSelectedVertically()
        {
            if (!ValidateSelection()) return;
            
            List<ToolGrill> selectedGrills = grillSelector.SelectedGrills;
            Bounds combinedBounds = CalculateCombinedBounds(selectedGrills);
            List<GameObject> grillObjects = ConvertToGameObjects(selectedGrills);
            
            alignsSystem.DistributeObjectsAlongAxis(grillObjects, combinedBounds, false);
            UpdateGrillPositions(selectedGrills);
            
            Debug.Log($"Distributed {selectedGrills.Count} grills vertically");
        }
        
        /// <summary>
        /// Validate that we have enough selected objects for alignment
        /// </summary>
        private bool ValidateSelection()
        {
            if (grillSelector == null)
            {
                Debug.LogWarning("ToolGrillSelector not found");
                return false;
            }
            
            if (alignsSystem == null)
            {
                Debug.LogWarning("Aligns system not found");
                return false;
            }
            
            List<ToolGrill> selectedGrills = grillSelector.SelectedGrills;
            if (selectedGrills.Count < 2)
            {
                Debug.LogWarning("Need at least 2 selected grills for alignment");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Calculate the combined bounds of all selected grills
        /// </summary>
        private Bounds CalculateCombinedBounds(List<ToolGrill> grills)
        {
            Bounds combinedBounds = new Bounds();
            bool firstGrill = true;
            
            foreach (ToolGrill grill in grills)
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
                            combinedBounds = totalBounds;
                            firstGrill = false;
                        }
                        else
                        {
                            combinedBounds.Encapsulate(totalBounds);
                        }
                    }
                    else
                    {
                        // Fallback to transform position if no renderers
                        if (firstGrill)
                        {
                            combinedBounds = new Bounds(grill.transform.position, Vector3.zero);
                            firstGrill = false;
                        }
                        else
                        {
                            combinedBounds.Encapsulate(grill.transform.position);
                        }
                    }
                }
            }
            
            return combinedBounds;
        }
        
        /// <summary>
        /// Convert ToolGrill list to GameObject list for the Aligns system
        /// </summary>
        private List<GameObject> ConvertToGameObjects(List<ToolGrill> grills)
        {
            List<GameObject> gameObjects = new List<GameObject>();
            foreach (ToolGrill grill in grills)
            {
                if (grill != null)
                {
                    gameObjects.Add(grill.gameObject);
                }
            }
            return gameObjects;
        }
        
        /// <summary>
        /// Update grill positions and trigger change events
        /// </summary>
        private void UpdateGrillPositions(List<ToolGrill> grills)
        {
            foreach (ToolGrill grill in grills)
            {
                if (grill != null)
                {
                    grill.UpdatePosition();
                    ToolManager.Instance.OnGrillChanged?.Invoke(grill);
                }
            }
            
            // Update the selection box to reflect new positions
            if (grillSelector != null)
            {
                grillSelector.UpdateFinalSelectionBox();
            }
        }
        
        /// <summary>
        /// Get the current selection bounds for external use
        /// </summary>
        public Bounds GetSelectionBounds()
        {
            if (grillSelector == null) return new Bounds();
            
            List<ToolGrill> selectedGrills = grillSelector.SelectedGrills;
            return CalculateCombinedBounds(selectedGrills);
        }
        
        /// <summary>
        /// Get the number of currently selected grills
        /// </summary>
        public int GetSelectedCount()
        {
            if (grillSelector == null) return 0;
            return grillSelector.SelectedGrills.Count;
        }
        
        /// <summary>
        /// Check if alignment is possible (at least 2 objects selected)
        /// </summary>
        public bool CanAlign()
        {
            return GetSelectedCount() >= 2;
        }
        
        /// <summary>
        /// Sort selected grills to a grid layout
        /// </summary>
        public void SortSelectedToGrid(int gridX, int gridY, float spaceX, float spaceY)
        {
            if (grillSelector == null) return;
            
            List<ToolGrill> selectedGrills = grillSelector.SelectedGrills;
            if (selectedGrills.Count == 0)
            {
                Debug.LogWarning("No grills selected for grid sorting");
                return;
            }
            
            if (selectedGrills.Count > gridX * gridY)
            {
                Debug.LogWarning($"Too many grills! Grid can only fit {gridX * gridY} grills.");
                return;
            }
            
            // Calculate the center of the selected objects
            Vector3 selectionCenter = CalculateSelectionCenter(selectedGrills);
            
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
            selectedGrills.Sort((a, b) => 
            {
                // Sort by Y first (top to bottom), then by X (left to right)
                if (Mathf.Abs(a.transform.position.y - b.transform.position.y) > 0.1f)
                {
                    return b.transform.position.y.CompareTo(a.transform.position.y); // Higher Y first
                }
                return a.transform.position.x.CompareTo(b.transform.position.x); // Lower X first
            });
            
            // Move grills to grid positions
            for (int i = 0; i < selectedGrills.Count && i < gridPositions.Count; i++)
            {
                if (selectedGrills[i] != null)
                {
                    selectedGrills[i].transform.position = gridPositions[i];
                    selectedGrills[i].UpdatePosition();
                    ToolManager.Instance.OnGrillChanged?.Invoke(selectedGrills[i]);
                }
            }
            
            // Update the selection box to reflect new positions
            grillSelector.UpdateFinalSelectionBox();
            
            Debug.Log($"Sorted {selectedGrills.Count} grills to {gridX}x{gridY} grid");
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
        
        /// <summary>
        /// Move all selected grills so their center is at Vector3.zero
        /// </summary>
        public void CenterSelectedToOrigin()
        {
            if (grillSelector == null) return;
            
            List<ToolGrill> selectedGrills = grillSelector.SelectedGrills;
            if (selectedGrills.Count == 0)
            {
                Debug.LogWarning("No grills selected for centering");
                return;
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
            grillSelector.UpdateFinalSelectionBox();
            
            Debug.Log($"Centered {selectedGrills.Count} grills to origin (offset: {offsetToOrigin})");
        }
        
        /// <summary>
        /// Move all selected grills to center at a specific position
        /// </summary>
        public void CenterSelectedAt(Vector3 targetCenter)
        {
            if (grillSelector == null) return;
            
            List<ToolGrill> selectedGrills = grillSelector.SelectedGrills;
            if (selectedGrills.Count == 0)
            {
                Debug.LogWarning("No grills selected for centering");
                return;
            }
            
            // Calculate the center of the selected objects
            Vector3 selectionCenter = CalculateSelectionCenter(selectedGrills);
            
            // Calculate the offset needed to move center to target
            Vector3 offsetToTarget = targetCenter - selectionCenter;
            
            // Move all selected grills by the offset
            foreach (ToolGrill grill in selectedGrills)
            {
                if (grill != null)
                {
                    Vector3 newPosition = grill.transform.position + offsetToTarget;
                    grill.transform.position = newPosition;
                    grill.UpdatePosition();
                    ToolManager.Instance.OnGrillChanged?.Invoke(grill);
                }
            }
            
            // Update the selection box to reflect new positions
            grillSelector.UpdateFinalSelectionBox();
            
            Debug.Log($"Centered {selectedGrills.Count} grills at {targetCenter} (offset: {offsetToTarget})");
        }
    }
} 