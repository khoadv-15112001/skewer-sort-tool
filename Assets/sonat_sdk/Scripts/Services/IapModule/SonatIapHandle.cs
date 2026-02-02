#if using_iap
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace Sonat.IapModule
{
    public class SonatIapHandle : MonoBehaviour, IStoreListener, IDetailedStoreListener
    {
        private SonatIap sonatIap;

        public void Initialize(SonatIap sonatIap)
        {
            this.sonatIap = sonatIap;
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            sonatIap.OnInitializeFailed(error);
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            sonatIap.OnInitializeFailed(error, message);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
             return sonatIap.ProcessPurchase(purchaseEvent);
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            sonatIap.OnPurchaseFailed(product, failureReason);
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            sonatIap.OnInitializeCompleted(controller, extensions);
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            sonatIap.OnPurchaseFailed(product, failureDescription);
        }
    }
}
#endif