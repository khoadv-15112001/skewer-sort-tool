using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GrillSort.LoseRwdService
{
    [CreateAssetMenu(fileName = "LoseRwdConfig", menuName = "Sonat Configs Custom/LoseRwdConfig")]
    [Serializable]
    public class LoseRwdConfig : ScriptableObject
    {
        public bool active = false;
        public bool loopByLevel = true;
        public int levelStart = 5;
        public int maxStage = 3;

        public List<LoseRwdData> datas = new();

        [Button("Print JSON Config")]
        private void PrintJsonConfig()
        {
            Debug.Log(JsonConvert.SerializeObject(this));
        }

        public void SetLoop(bool value)
        {
            loopByLevel = value;
        }
    }

    [Serializable]
    public class LoseRwdData
    {
        public int stage;           // Mốc ads (1, 2, 3)
        public int addTimeSeconds;  // Thời gian cộng thêm sau khi xem ads
    }
}
