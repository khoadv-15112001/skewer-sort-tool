using System;
using System.Collections.Generic;
using Gameplay.LevelData;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Tool
{
    public class UIToolGrill : MonoBehaviour
    {
        [SerializeField] private Transform container;

        private Dictionary<LayerData, UIToolLayer> layers = new();
        private GrillData grillData;
        //[SerializeField] private Toggle lockPosition;

        //[SerializeField] private TMP_InputField mainGrillIdInput;

        public void Setup(GrillData grillData)
        {
            ValidatePreviousData();
            layers.Clear();
            this.grillData = grillData;
            MySonatFramework.poolingContainer.CleanContainer(container);
            if (grillData.layer != null)
                for (int i = 0; i < grillData.layer.Count; i++)
                {
                    var item = MySonatFramework.poolingContainer.CreateObject<UIToolLayer>(container);
                    item.Setup(grillData.layer[i], this);
                    layers.Add(grillData.layer[i], item);
                }

            UpdateLayerMain();

            // if (mainGrillIdInput != null)
            // {
            //     mainGrillIdInput.text = grillData.mainGrillId.ToString();
            //     mainGrillIdInput.onEndEdit.RemoveAllListeners();
            //     mainGrillIdInput.onEndEdit.AddListener(OnMainGrillIdChanged);
            // }
            
            // lockPosition.onValueChanged.RemoveAllListeners();
            // lockPosition.isOn = toolGrill.lockPosition;
            //lockPosition.onValueChanged.AddListener((isOn) => toolGrill.lockPosition = isOn);
        }

        private void OnMainGrillIdChanged(string value)
        {
            if (byte.TryParse(value, out byte newId))
            {
                grillData.mainGrillId = newId;
                // Tự động save khi thay đổi
                UIToolPanel.Instance.UpdateGrill();
            }
        }

        public void Clear()
        {
            layers.Clear();
            this.grillData = null;
            gameObject.SetActive(false);
        }


        public void AddLayer()
        {
            int numSlot = grillData.SlotCount; //GrillSlot(grillData.grillType);
            LayerData newLayer = new LayerData(numSlot);
            var item = MySonatFramework.poolingContainer.CreateObject<UIToolLayer>(container);
            item.Setup(newLayer, this);
            grillData.layer ??= new();
            grillData.layer.Add(newLayer);
            layers.Add(newLayer, item);

            // Add undo for adding layer
            ToolManager.Instance.undoController.AddLayerOperation(newLayer, LayerOperationType.Add);

            UpdateLayerMain();
        }

        // private int GrillSlot(GrillType grillType)
        // {
        //     switch (grillType)
        //     {
        //         case GrillType.Single:
        //         case GrillType.Vending:
        //         case GrillType.SingleMin:
        //             return 1;
        //         case GrillType.Drop7:
        //         case GrillType.Drop7Ice:
        //         case GrillType.Drop7Lid:
        //         case GrillType.Drop7Lock:
        //         case GrillType.Drop7LockAds:
        //             return 7;
        //         case GrillType.Drop5:
        //             return 5;
        //         case GrillType.Drop6:
        //             return 6;
        //         default: return 3;
        //     }
        // }

        public void RemoveLayer(LayerData layer)
        {
            // Store the layer data for undo before removing
            var layerData = layer;

            layers.Remove(layer);
            grillData.layer.Remove(layer);

            // Add undo for deleting layer
            ToolManager.Instance.undoController.AddLayerOperation(layerData, LayerOperationType.Delete, layerData);

            UIToolPanel.Instance.UpdateGrill();
        }

        public void UpdateLayerMain()
        {
            if (grillData == null || grillData.layer == null) return;
            for (int i = 0; i < grillData.layer.Count; i++)
            {
                layers[grillData.layer[i]].SetMainLayer(i == ToolManager.currentLayer);
            }
        }

        private void ValidatePreviousData()
        {
            if (grillData != null)
            {
                grillData.ValidateData();
            }
        }
    }
}