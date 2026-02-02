using System;
using System.Collections.Generic;
using Gameplay.LevelData;
using SonatFramework.Systems.ObjectPooling;
using TMPro;
using UnityEngine;

namespace Tool
{
    public class ToolLockObstacle: ToolObstacleBase
    {
        //private IceObstacleData iceObstacleData;
        [SerializeField] private TMP_InputField inputFieldProgress;

        public override void SetGrills(List<ToolGrill> toolGrills)
        {
            base.SetGrills(toolGrills);
            toolGrills[0].GrillData.isLock = true;
            inputFieldProgress.text = "4";
            transform.SetParent(toolGrills[0].transform);
        }
        
        private void OnEnable()
        {
            
            inputFieldProgress.onSubmit.AddListener(OnSubmit);
        }

        private void OnDisable()
        {
            inputFieldProgress.onSubmit.RemoveListener(OnSubmit);
        }

        private void OnSubmit(string arg0)
        {
            if (int.TryParse(arg0, out int result))
            {
                //iceObstacleData.progress = result;
            }
        }
        
        // private void OnMouseDown()
        // {
        //     if (!ToolManager.selectAvailable) return;
        //
        //     if (Input.GetKey(KeyCode.X))
        //     {
        //         UIToolPanel.Instance.RemoveObstacle(this);
        //         base.toolGrill.GrillData.isLock = false;
        //         return;
        //     }
        // }
    }
}