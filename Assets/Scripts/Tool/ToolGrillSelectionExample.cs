using System.Collections.Generic;
using Gameplay.LevelData;
using UnityEngine;

namespace Tool
{
    /// <summary>
    /// Example script showing how to use the ToolGrillSelector system
    /// This script demonstrates common operations you can perform on selected grills
    /// </summary>
    public class ToolGrillSelectionExample : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private ToolGrillSelector grillSelector;

        [Header("Example Operations")] [SerializeField]
        private KeyCode deleteSelectedKey = KeyCode.Delete;

        [SerializeField] private KeyCode duplicateSelectedKey = KeyCode.D;
        [SerializeField] private KeyCode moveSelectedKey = KeyCode.M;

        private void Start()
        {
            // Find the selector if not assigned
            if (grillSelector == null)
            {
                grillSelector = FindObjectOfType<ToolGrillSelector>();
            }
        }

        private void Update()
        {
            HandleExampleOperations();
        }

        private void HandleExampleOperations()
        {
            if (grillSelector == null) return;

            List<ToolGrill> selectedGrills = grillSelector.SelectedGrills;


            // Delete selected grills
            if (Input.GetKeyDown(deleteSelectedKey))
            {
                DeleteSelectedGrills(selectedGrills);
            }

            if (Input.GetKey(KeyCode.LeftControl))
            {
                // Duplicate selected grills
                if (Input.GetKeyDown(duplicateSelectedKey))
                {
                    DuplicateSelectedGrills(selectedGrills);
                }
            }
        }

        private void DeleteSelectedGrills(List<ToolGrill> grills)
        {
            if (grills.Count == 0) return;

            Debug.Log($"Deleting {grills.Count} selected grill(s)");

            foreach (ToolGrill grill in grills)
            {
                if (grill != null)
                {
                    UIToolPanel.Instance.RemoveGrill(grill);
                }
            }

            // Clear selection after deletion
            grillSelector.ClearSelection();
        }

        private void DuplicateSelectedGrills(List<ToolGrill> grills)
        {
            if (grills.Count == 0) return;

            Debug.Log($"Duplicating {grills.Count} selected grill(s)");

            List<ToolGrill> newGrills = new List<ToolGrill>();

            foreach (ToolGrill originalGrill in grills)
            {
                if (originalGrill != null)
                {
                    // Create a new grill at a slightly offset position
                    ToolGrill newGrill = ToolManager.Instance.CreateToolGrill(GrillType.Normal, originalGrill.NumberSlot);
                    newGrill.transform.position = originalGrill.transform.position + new Vector3(1f, 1f, 0);

                    // Copy the grill data if it exists
                    if (originalGrill.GrillData != null)
                    {
                        GrillData newData = new GrillData();
                        newData.id = ToolManager.Instance.GetAvailableGrillId();
                        newData.position = new Vector3Data(newGrill.transform.position);
                        newData.layer = originalGrill.GrillData.layer; // Copy layer data
                        newData.isLock = originalGrill.GrillData.isLock;
                        newData.slotCount = originalGrill.GrillData.slotCount;

                        newGrill.SetData(newData);
                        UIToolPanel.Instance.LevelData.grillData.Add(newData);
                        UIToolPanel.Instance.UpdateItemCount();
                    }

                    newGrills.Add(newGrill);
                    
                }
            }

            // Select the newly created grills
            grillSelector.ClearSelection();
            foreach (ToolGrill newGrill in newGrills)
            {
                grillSelector.SelectGrill(newGrill);
            }
        }

        private void MoveSelectedGrills(List<ToolGrill> grills)
        {
            if (grills.Count == 0) return;

            Debug.Log($"Moving {grills.Count} selected grill(s)");

            // Example: Move all selected grills up by 1 unit
            Vector3 moveOffset = new Vector3(0, 1f, 0);

            foreach (ToolGrill grill in grills)
            {
                if (grill != null)
                {
                    grill.transform.position += moveOffset;
                    grill.UpdatePosition();
                }
            }
        }

        /// <summary>
        /// Example method to get all selected grills and perform a custom operation
        /// </summary>
        public void PerformCustomOperationOnSelected()
        {
            if (grillSelector == null) return;

            List<ToolGrill> selectedGrills = grillSelector.SelectedGrills;

            if (selectedGrills.Count == 0)
            {
                Debug.Log("No grills selected");
                return;
            }

            // Example: Change the layer of all selected grills
            foreach (ToolGrill grill in selectedGrills)
            {
                if (grill != null)
                {
                    // Perform your custom operation here
                    Debug.Log($"Performing custom operation on grill: {grill.name}");
                }
            }
        }

        /// <summary>
        /// Example method to select grills by type or other criteria
        /// </summary>
        public void SelectGrillsByCriteria()
        {
            if (grillSelector == null) return;

            // Clear current selection
            grillSelector.ClearSelection();

            // Example: Select all grills that are locked
            foreach (ToolGrill grill in ToolManager.Instance.AllGrills)
            {
                if (grill != null && grill.GrillData != null && grill.GrillData.isLock)
                {
                    grillSelector.SelectGrill(grill);
                }
            }

            Debug.Log($"Selected {grillSelector.SelectedGrills.Count} locked grill(s)");
        }

        /// <summary>
        /// Example method to select grills in a specific area
        /// </summary>
        public void SelectGrillsInArea(Vector3 center, float radius)
        {
            if (grillSelector == null) return;

            // Clear current selection
            grillSelector.ClearSelection();

            // Select all grills within the specified radius
            foreach (ToolGrill grill in ToolManager.Instance.AllGrills)
            {
                if (grill != null)
                {
                    float distance = Vector3.Distance(grill.transform.position, center);
                    if (distance <= radius)
                    {
                        grillSelector.SelectGrill(grill);
                    }
                }
            }

            Debug.Log($"Selected {grillSelector.SelectedGrills.Count} grill(s) in area");
        }
    }
}