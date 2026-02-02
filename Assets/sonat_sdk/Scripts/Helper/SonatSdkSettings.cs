using System.Collections.Generic;
using Sonat.Debugger;
using UnityEngine;

namespace Sonat
{
    [CreateAssetMenu(fileName = "SonatSdkSettings", menuName = "SonatSDK/Settings")]
    public class SonatSdkSettings : ScriptableObject
    {
        public string appID_Android;
        public string appID_IOS;

        public int timeout = 10;
        
        public string iapKeyEnum = "ShopItemKey";
        public List<SonatDebugType> logTypes = new() { SonatDebugType.All };
        public bool internetConnection = true;
    }
}
