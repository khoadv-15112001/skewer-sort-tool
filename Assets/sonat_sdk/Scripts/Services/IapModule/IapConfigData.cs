using System.Collections.Generic;
using UnityEngine;

namespace Sonat.IapModule
{
    [CreateAssetMenu(fileName = "IapConfigData", menuName = "SonatSDK/Configs/IAP Configs")]
    public class IapConfigData : ScriptableObject
    {
        public List<StoreProductDescriptor> products = new List<StoreProductDescriptor>();
    }
}