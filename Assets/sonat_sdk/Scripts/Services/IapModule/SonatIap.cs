using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Sonat.AdsModule;
using Sonat.Data;
using Sonat.Debugger;
using Sonat.FirebaseModule;
using Sonat.FirebaseModule.RemoteConfig;
using Sonat.TrackingModule;
using UnityEngine;
using UnityEngine.Networking;
#if using_iap
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
#endif

namespace Sonat.IapModule
{
    [CreateAssetMenu(menuName = "SonatSDK/Services/Iap Service", fileName = "SonatIap")]
    public class SonatIap : SonatService
    {
        private static SonatIap instance;
        public override SonatServiceType ServiceType => SonatServiceType.IapService;
        public override bool Ready { get; set; }

        private readonly Dictionary<string, BuyIapContent> buyIapContents = new();
        public List<StoreProductDescriptor> StoreProductDescriptors = new List<StoreProductDescriptor>();

        public event Action<int> OnInAppPurchased;

        public bool autoRestore;
        public float timeWaitStore = 10;
        [SerializeField] private string verifyUrl = "https://us-central1-sonat-arm-358507.cloudfunctions.net/verify_inapp_purchase";

        public static float sn_ltv_iap
        {
            get => PlayerPrefs.GetFloat(nameof(sn_ltv_iap));
            set
            {
                var last = PlayerPrefs.GetFloat(nameof(sn_ltv_iap));
                if (Math.Abs(last - value) > 0.0001f)
                {
                    SonatFirebase.analytic.SetUserProperty(nameof(sn_ltv_iap), value.ToString(CultureInfo.InvariantCulture));
                    PlayerPrefs.SetFloat(nameof(sn_ltv_iap), value);
                }
            }
        }


        private readonly List<string> _currencyLast = new List<string>()
        {
            "₫",
        };


#if using_iap
        public static IStoreController _mStoreController; // The Unity Purchasing system.
        public static IExtensionProvider _mStoreExtensionProvider; // The store-specific Purchasing subsystems.
#endif

        //private KeyValuePair<int, Action<bool>> _onSuccess;
        private bool _isBuy;
        private int tried;


        private static PlayerPrefListInt itemsPurchased;
#if using_iap
        private static SonatIapHandle handle;
#endif

        public override void Initialize(Action<ISonatService> onInitialized)
        {
            instance = this;
            _isBuy = false;
            base.Initialize(onInitialized);
            itemsPurchased = new PlayerPrefListInt("item_purchased", new List<int>());
#if using_iap
            handle = SonatSdkManager.instance.gameObject.AddComponent<SonatIapHandle>();
            handle.Initialize(this);
#endif
            InitializePurchasing();
        }

        public bool IsInitialized()
        {
#if using_iap
            return _mStoreController != null && _mStoreExtensionProvider != null;
#else
            return false;
#endif
        }

        private void InitializePurchasing()
        {
            SonatDebugType.Iap.Log("InitializePurchasing times: " + tried);

#if UNITY_EDITOR
            foreach (var storeProductDescriptor in StoreProductDescriptors)
            {
                if (storeProductDescriptor.StoreProductId.Contains(" "))
                    SonatDebugType.Iap.LogError("Invalid id : contains space");
            }
#endif
            if (IsInitialized())
            {
                SonatDebugType.Iap.Log("Purchaser IsInitialized return");
                return;
            }

#if using_iap
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
#endif

            foreach (var descriptor in StoreProductDescriptors)
            {
                if (string.IsNullOrEmpty(descriptor.StoreProductId))
                    SonatDebugType.Iap.LogError("Product id is empty!");
                if (StoreProductDescriptors.Count(x =>
                        x.StoreProductId == descriptor.StoreProductId) > 1)
                    SonatDebugType.Iap.LogError("Duplicate" + descriptor.StoreProductId);
#if using_iap
                if (descriptor.active)
                    builder.AddProduct(descriptor.StoreProductId, descriptor.productType);
#endif
            }

#if using_iap
            UnityPurchasing.Initialize(handle, builder);
#endif
        }

        private IEnumerator IeWaitToReload()
        {
            while (!SonatSdkManager.IsInternetConnection())
            {
                yield return new WaitForSeconds(0.5f);
            }

            yield return new WaitForSeconds(1);
            if (!IsInitialized())
                InitializePurchasing();
        }


        private void BuyProductId(string productId)
        {
#if using_iap
            if (IsInitialized())
            {
                SonatDebugType.Iap.Log($"IsInitialized {IsInitialized()} BuyProductId:{productId}");
                var product = _mStoreController.products.WithID(productId);
                if (product != null && product.availableToPurchase)
                {
                    _mStoreController.InitiatePurchase(product);
                }
                else
                {
                    SonatDebugType.Iap.LogError($"Not available: {productId}");
                }
            }
            else
            {
                SonatDebugType.Iap.LogError($"Not IsInitialized {IsInitialized()} BuyProductId: {productId}");
            }
#endif
        }

        private string _GetPriceTextDefault(int itemId)
        {
            var item = StoreProductDescriptors.Find(x => x.key == itemId);
            return "$" + item.price;
        }

        private object _GetPriceTextSaleOriginal(int shopKey, float saleRatio)
        {
            var item = StoreProductDescriptors.Find(x => x.key == shopKey);
            return "$" + (item.price / saleRatio);
        }

#if using_iap
        public static bool IsSubcribed(string storeProductId, out int expiredIn)
        {
            var subscriptionProduct = _mStoreController.products.WithID(storeProductId);
            SonatDebugType.Iap.Log("subscriptionProduct " + subscriptionProduct);
            expiredIn = 0;
            try
            {
                if (subscriptionProduct == null) return false;
                var isSubscribed = IsSubscribedTo(subscriptionProduct, out SubscriptionInfo subscriptionInfo);
                if (isSubscribed)
                    expiredIn = SonatSdkHelper.GetEpochDate(subscriptionInfo.getExpireDate()) -
                                SonatSdkHelper.GetEpochDate();
                return isSubscribed;
            }
            catch (StoreSubscriptionInfoNotSupportedException)
            {
                return false;
            }

            return false;
        }

        private static bool IsSubscribedTo(Product subscription, out SubscriptionInfo subscriptionInfo)
        {
            subscriptionInfo = null;
            SonatDebugType.Iap.Log("subscription" + subscription);
            // If the product doesn't have a receipt, then it wasn't purchased and the user is therefore not subscribed.
            if (subscription.receipt == null)
            {
                return false;
            }

            //The intro_json parameter is optional and is only used for the App Store to get introductory information.
            var subscriptionManager = new SubscriptionManager(subscription, null);
            SonatDebugType.Iap.Log("subscriptionManager" + subscriptionManager);

            // The SubscriptionInfo contains all of the information about the subscription.
            // Find out more: https://docs.unity3d.com/Packages/com.unity.purchasing@3.1/manual/UnityIAPSubscriptionProducts.html

            subscriptionInfo = subscriptionManager.getSubscriptionInfo();
            SonatDebugType.Iap.Log("subscriptionInfo" + subscriptionInfo);

            return subscriptionInfo.isSubscribed() == Result.True;
        }
#endif

        private string _GetPriceText(int itemId)
        {
            var item = StoreProductDescriptors.Find(x => x.key == itemId);

            if (item == null)
            {
                SonatDebugType.Iap.LogError($"Not available: {itemId}");
                return "Not Found";
            }
#if UNITY_EDITOR
            return "$" + item.price;
#endif
#if using_iap
            if (_mStoreController != null && _mStoreController.products != null)
            {
                var product = _mStoreController.products.WithID(item.StoreProductId);
                if (product != null && product.availableToPurchase)
                {
                    if (_currencyLast.Contains(product.metadata.isoCurrencyCode))
                        return MoveAllSc(product.metadata.localizedPriceString);
                    return product.metadata.localizedPriceString;
                }
            }
#endif

            return "$" + item.price;
        }


        private bool _IsItemAvailable(int itemId)
        {
            var item = StoreProductDescriptors.Find(x => x.key == itemId);
#if using_iap
            if (_mStoreController != null)
            {
                var product = _mStoreController.products.WithID(item.StoreProductId);
                if (product != null && product.availableToPurchase)
                    return true;
            }
#endif
            return false;
        }

        private bool _CheckHasPurchasedProductId(int id)
        {
            var product = StoreProductDescriptors.Find(x => x.key == id);
#if using_iap
            if (_mStoreController.products.WithID(product.StoreProductId).hasReceipt)
                return true;
#endif
            return false;
        }


        private class BuyIapContent
        {
            public Action<bool> OnSuccess;
            public SonatBuyLogData buyLogData;
        }


        private void _Buy(int id, Action<bool> onComplete, SonatBuyLogData buyLog)
        {
            if (_isBuy)
            {
                onComplete?.Invoke(false);
                return;
            }

            var content = new BuyIapContent()
            {
                OnSuccess = onComplete,
                buyLogData = buyLog
            };


            if (!IsInitialized())
            {
                SonatDebugType.Iap.Log("Buy Fail: Purchaser not initialized");
                onComplete?.Invoke(false);
                return;
            }

            //_onSuccess = new KeyValuePair<int, Action<bool>>(id, onComplete);
            var product = StoreProductDescriptors.Find(x => x.key == id);

            SonatDebugType.Iap.Log("Try to buy key " + product.StoreProductId);

            buyIapContents.TrySetValue(product.StoreProductId, content);
            _isBuy = true;

            SonatFirebase.analytic.LogEvent(EventNameEnum.buy_iap, new[]
            {
                new LogParameter(ParameterEnum.product_click_buy, product.StoreProductId)
            });

            SonatAds.blockOnFocusAds = true;
            BuyProductId(product.StoreProductId);
        }


        #region IAP Handle

#if using_iap
        public void OnInitializeCompleted(IStoreController controller, IExtensionProvider extensions)
        {
            _mStoreController = controller;
            _mStoreExtensionProvider = extensions;
            Ready = true;
            OnInitialized?.Invoke(this);

            if (autoRestore)
                GetItemsPurchased();

            SonatDebugType.Iap.Log("Available items: ");
            foreach (var item in controller.products.all)
            {
                if (item.availableToPurchase)
                {
                    Debug.Log(string.Join(" - ", item.metadata.localizedTitle, item.metadata.localizedDescription,
                        item.metadata.isoCurrencyCode, item.metadata.localizedPrice.ToString(CultureInfo.InvariantCulture),
                        Decimal.ToDouble(item.metadata.localizedPrice).ToString(CultureInfo.InvariantCulture).Replace(",", "."),
                        item.metadata.localizedPriceString, item.transactionID, item.receipt));
                }
            }
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            SonatDebugType.Iap.Log("PurchaseFailed " + product.transactionID + "/" + failureDescription.message);

            var specificId = product.definition.storeSpecificId;
            if (buyIapContents.TryGetValue(specificId, out var buyIapContent))
            {
                buyIapContents.Remove(specificId);
                if (failureDescription.reason == PurchaseFailureReason.UserCancelled)
                {
                    var item = StoreProductDescriptors.Find(x => x.StoreProductId == specificId);
                    new SonatLogCancelShopItem()
                    {
                        item_id = item.StoreProductId,
                        item_type = buyIapContent.buyLogData.item_type,
                        placement = buyIapContent.buyLogData.placement,
                        location = buyIapContent.buyLogData.location,
                        screen = buyIapContent.buyLogData.screen,
                        level = UserData.GetLevel(),
                    }.Post();
                }

                buyIapContent.OnSuccess.Invoke(false);
            }

            _isBuy = false;
        }


        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            SonatDebugType.Iap.Log("OnPurchaseProcessingResult " + args.purchasedProduct.metadata.localizedTitle);
            var specificId = args.purchasedProduct.definition.storeSpecificId;
            _isBuy = false;
            if (buyIapContents.TryGetValue(specificId, out var buyIapContent))
            {
                buyIapContents.Remove(specificId);

                var item = StoreProductDescriptors.Find(x => x.StoreProductId == specificId);

                SonatDebugType.Iap.Log($"{item.StoreProductId} specificId " + specificId);

                itemsPurchased.Add(item.key);
                buyIapContent.OnSuccess.Invoke(true);
                OnBuySuccessHandler(item.key);

                var log = new SonatLogBuyShopItemIap(buyIapContent.buyLogData)
                {
                    currency = args.purchasedProduct.metadata.isoCurrencyCode,
                    item_id = item.StoreProductId,
                    value = (float)args.purchasedProduct.metadata.localizedPrice,
                    value_in_usd = item.price,
                    is_first_buy = itemsPurchased.Count == 1,
                    level = UserData.GetLevel(),
                    mode = UserData.GetMode(),
                    buy_count = itemsPurchased.Count
                };
#if UNITY_ANDROID
                VerifyIap(item.productType != ProductType.Subscription ? "product" : "subscription"
                    , Application.identifier, item.StoreProductId, args.purchasedProduct.transactionID,
                    () =>
                    {
                        sn_ltv_iap += item.price;
                        log.Post();
                    });
#else
                sn_ltv_iap += item.price;
                log.Post();
#endif
            }

            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Debug.Log("OnPurchaseFailed " + product.transactionID);
            _isBuy = false;
        }


        public void OnInitializeFailed(InitializationFailureReason error)
        {
            SonatDebugType.Iap.Log("Purchaser OnInitialized failed " + error);
            if (tried < 5)
            {
                tried++;
                StartCoroutine(IeWaitToReload());
            }
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            SonatDebugType.Iap.Log("Purchaser OnInitialized failed " + error);
            SonatDebugType.Iap.Log("Purchaser OnInitialized failed " + message);

            if (tried < 5)
            {
                tried++;
                StartCoroutine(IeWaitToReload());
            }
        }
#endif

        #endregion

        private void VerifyIap(string kind, string packageName, string skuId, string purchaseToken, Action afterVerify)
        {
            WWWForm form = new WWWForm();
            form.AddField("kind", kind);
            form.AddField("package_name", packageName);
            form.AddField("sku_id", skuId);
            form.AddField("purchase_token", purchaseToken);
            StartCoroutine(IeVerify(form, afterVerify));
        }

        private IEnumerator IeVerify(WWWForm form, Action afterVerify)
        {
            yield return new WaitForSeconds(2);
            using (UnityWebRequest www = UnityWebRequest.Post(instance.verifyUrl, form))
            {
                var cert2 = new ForceAcceptAll();
                www.certificateHandler = cert2;
                yield return www.SendWebRequest();
                SonatDebugType.Iap.Log(www.responseCode.ToString());
                if (www.responseCode == 200)
                {
                    var respond = JsonConvert.DeserializeObject<RespondVerifyIap>(www.downloadHandler.text);
                    if (respond == null || respond.errorCode != 0 || respond.data == null || respond.data.purchaseType == 0)
                    {
                        SonatDebugType.Iap.LogError("not valid");
                    }
                    else
                    {
                        afterVerify.Invoke();
                    }
                }
                else
                    afterVerify.Invoke();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    SonatDebugType.Iap.LogError(www.error);
                }
                else
                {
                    SonatDebugType.Iap.Log(www.downloadHandler.text);
                }
            }
        }


        private void OnBuySuccessHandler(int id)
        {
            _isBuy = false;
            OnInAppPurchased?.Invoke(id);
            SonatDebugType.Iap.Log("OnBuySuccessHandler ShopItemKey:" + id);
        }

        private void _RestorePurchases(Action<List<int>> onRestoreComplete)
        {
#if UNITY_IOS
        if (!IsInitialized())
        {
            onRestoreComplete?.Invoke(null);
            return;
        }

#if using_iap
        var apple = _mStoreExtensionProvider.GetExtension<IAppleExtensions>();

        apple.RestoreTransactions((result, errorMsg) =>
        {
            SonatDebugType.Iap.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");

            if (!result)
            {
                SonatDebugType.Iap.LogError("Restore: " + errorMsg);
                onRestoreComplete?.Invoke(null);
            }
            else
            {
                 onRestoreComplete?.Invoke(GetItemsPurchased());
            }
        });
#endif
#else
            onRestoreComplete?.Invoke(GetItemsPurchased());
#endif
        }


        #region Static Functions

        public static void Buy(int id, Action<bool> onComplete, SonatBuyLogData buyLog)
        {
            if (instance == null)
            {
                onComplete?.Invoke(false);
                return;
            }

            instance._Buy(id, onComplete, buyLog);
        }

        public static bool PaidUser()
        {
            if (instance == null) return false;
            return itemsPurchased.Count > 0;
        }

        public static List<int> GetListItemPurchased()
        {
            if (instance == null) return null;
            return itemsPurchased.Current;
        }

        public static bool CheckItemBought(int id)
        {
            if (instance == null) return false;
            return itemsPurchased.Current != null && itemsPurchased.Current.FindIndex(e => e == id) >= 0;
        }

        public static List<int> GetPackPurchased()
        {
            if (instance == null) return null;
            return itemsPurchased.Current;
        }

        public static void RestorePurchases(Action<List<int>> onRestoreComplete)
        {
            if (instance == null) return;
            instance._RestorePurchases(onRestoreComplete);
        }

        public static List<int> GetItemsPurchased()
        {
            if (instance == null) return null;
            List<int> productPurchased = new List<int>();
#if using_iap
            foreach (var product in instance.StoreProductDescriptors)
            {
                if (!product.active) continue;
                if (_mStoreController.products.WithID(product.StoreProductId).hasReceipt)
                {
                    productPurchased.Add(product.key);
                    SonatDebugType.Iap.Log($"Has receipt {product.key}");
                }
            }
#endif

            return productPurchased;
        }

        public static string GetCurrencySymbol(string code)
        {
            RegionInfo regionInfo =
                (from culture in CultureInfo.GetCultures(CultureTypes
                        .InstalledWin32Cultures)
                    where culture.Name.Length > 0 && !culture.IsNeutralCulture
                    let region = new RegionInfo(culture.LCID)
                    where String.Equals(region.ISOCurrencySymbol, code, StringComparison.InvariantCultureIgnoreCase)
                    select region).First();

            return regionInfo.CurrencySymbol;
        }

        public static string GetPriceText(int itemId)
        {
            if (instance == null) return "";
            return instance._GetPriceText(itemId);
        }

        public static string GetPriceTextDefault(int itemId)
        {
            if (instance == null) return "";
            return instance._GetPriceTextDefault(itemId);
        }

        public static object GetPriceTextSaleOriginal(int shopKey, float saleRatio)
        {
            if (instance == null) return "";
            return instance._GetPriceTextSaleOriginal(shopKey, saleRatio);
        }

        public static bool IsItemAvailable(int itemId)
        {
            if (instance == null) return false;
            return instance._IsItemAvailable(itemId);
        }

        public static StoreProductDescriptor GetStoreProductDescriptor(int storeProductId)
        {
            if (instance == null) return null;
            return instance.StoreProductDescriptors.FirstOrDefault(e => e.key == storeProductId);
        }

        public static int ConvertProductIDToShopKey(string storeProductId)
        {
            if (instance == null) return -1;
            var storeProductDescriptor = instance.StoreProductDescriptors.FirstOrDefault(e => e.storeProductId == storeProductId);
            return storeProductDescriptor == null ? -1 : storeProductDescriptor.key;
        }

        public static bool CheckHasPurchasedProductId(int id)
        {
            if (instance == null) return false;
            return instance._CheckHasPurchasedProductId(id);
        }

        public static CultureInfo GetCultureInfoFromIsoCurrencyCode(string code)
        {
            foreach (CultureInfo ci in CultureInfo.GetCultures(
                         CultureTypes.SpecificCultures))
            {
                RegionInfo ri = new RegionInfo(ci.LCID);
                if (ri.ISOCurrencySymbol == code)
                    return ci;
            }

            return null;
        }

        #endregion


        private class ForceAcceptAll : CertificateHandler
        {
            protected override bool ValidateCertificate(byte[] certificateData)
            {
                return true;
            }
        }


        private static String MoveAllSc(String str)
        {
            // Take length of string 
            int len = str.Length;

            // regular expression to check  
            // char is special or not. 
            var regx = new Regex("[a-zA-Z0-9\\.\\,\\s+]");

            // traverse string 
            String res1 = "", res2 = "";
            for (int i = 0; i < len; i++)
            {
                char c = str[i];

                // check char at index i is a special char 
                if (regx.IsMatch(c.ToString()))
                    res1 = res1 + c;
                else
                    res2 = res2 + c;
            }

            return res1 + res2;
        }
    }

    [Serializable]
    public class RespondVerifyIap
    {
        public int errorCode;
        public VerifyData data;
    }

    [Serializable]
    public class VerifyData
    {
        public string kind;
        public string orderId;
        public int purchaseType;
        public string regionCode;
        public int purchaseState;
    }
}