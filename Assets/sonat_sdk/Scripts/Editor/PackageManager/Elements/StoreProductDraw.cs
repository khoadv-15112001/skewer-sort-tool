using Sonat.IapModule;
using UnityEditor;
using UnityEngine;
#if using_iap
using UnityEngine.Purchasing;
#endif

namespace Sonat.Editor.PackageManager.Elements
{
    public class StoreProductDraw
    {
        private IAPManagerWindow iapManagerWindow;
        private StoreProductDescriptor product;
        private bool foldOut;
        private int index;
        private int keyEnumValue;

        public StoreProductDraw(StoreProductDescriptor product, IAPManagerWindow iapManagerWindow)
        {
            this.product = product;
            this.iapManagerWindow = iapManagerWindow;

            UpdateKey();
        }

        private void UpdateKey()
        {
            if (iapManagerWindow.enumNames != null && iapManagerWindow.enumNames.Count > 0)
            {
                string value = iapManagerWindow.enumList.GetNameString(product.key);
                keyEnumValue = iapManagerWindow.enumNames.IndexOf(value);
            }
        }

        public void Draw()
        {
            GUILayout.BeginHorizontal(GUI.skin.box);
            product.active = EditorGUILayout.Toggle("", product.active, GUILayout.Width(12));
            EditorGUI.BeginDisabledGroup(!product.active);
            GUILayout.BeginVertical();
            string label = string.IsNullOrEmpty(product.StoreProductId) ? "New Product" : product.StoreProductId;
            foldOut = EditorGUILayout.Foldout(foldOut, label, true);

            if (foldOut)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.BeginVertical();
                GUILayout.Space(10);


                if (iapManagerWindow.enumNames != null && iapManagerWindow.enumNames.Count > 0)
                {
                    keyEnumValue = EditorGUILayout.Popup("Shop Item Key", keyEnumValue, iapManagerWindow.enumNames.ToArray());
                    if (keyEnumValue >= 0 && keyEnumValue <= iapManagerWindow.enumNames.Count - 1)
                    {
                        string value = iapManagerWindow.enumNames[keyEnumValue];
                        int key = iapManagerWindow.enumList.GetIntValue(value);
                        if (key >= 0) product.key = key;
                    }
                }
                else
                {
                    product.key = EditorGUILayout.IntField("Shop Item Key", product.key);
                }

                product.storeProductId = EditorGUILayout.TextField("Product Id", product.StoreProductId);
                //product.storeProductId_ios = EditorGUILayout.TextField("Product Id iOS", product.StoreProductId);
                product.price = EditorGUILayout.FloatField("Price", product.price);

#if using_iap
                product.productType = (ProductType)EditorGUILayout.EnumPopup("Product Type", product.productType, GUILayout.Width(220));
#endif

                GUILayout.Space(5);

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                iapManagerWindow.RemoveProduct(product);
            }

            GUILayout.EndHorizontal();
        }
    }
}