//-----------------------------------------------------------------------
// <copyright file="OdinAddressableReflection.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Reflection;
#if UNITY_EDITOR
using UnityEditor.AddressableAssets.Settings;
#endif
namespace Sirenix.OdinInspector.Modules.Addressables.Editor.Internal
{
    internal static class OdinAddressableReflection
    {
        public static FieldInfo AddressableAssetEntry_mGUID_Field;
#if UNITY_EDITOR
        static OdinAddressableReflection()
        {
            AddressableAssetEntry_mGUID_Field = typeof(AddressableAssetEntry).GetField("m_GUID", BindingFlags.Instance | BindingFlags.NonPublic);
        }
#endif
        internal static void EnsureConstructed() { }
    }
}