using System;
using Sonat.Attributes;
#if using_iap
using UnityEngine.Purchasing;
#endif

namespace Sonat.IapModule
{
    
    [Serializable]
    public class StoreProductDescriptor
    {
        [IndexAsEnumSonatSdk(nameof(BuiltInEnumType.ShopItemKey))]
        public int key;

        public bool active = true;

        public string StoreProductId
        {
            get
            {
#if UNITY_ANDROID
            return storeProductId;
#else
                if (string.IsNullOrEmpty(storeProductId_ios))
                    return storeProductId;
                else
                    return storeProductId_ios;
#endif
            }
        }

        public string storeProductId;
        public string storeProductId_ios;
        public float price;

        //public string description;

#if using_iap
        public ProductType productType = ProductType.Consumable;
#endif

        public void SetName(string defaultPackageName)
        {
            storeProductId = defaultPackageName;
            storeProductId_ios = defaultPackageName;
        }
    }
}