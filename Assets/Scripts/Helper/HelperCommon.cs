using System;
using System.Collections.Generic;
using Gameplay.LevelData;
using SonatFramework.Scripts.SonatSDKAdapterModule;
using UnityEngine;

namespace Helper
{
    public static class HelperCommon
    {

        public static GrillType ValidateGrillType(this GrillType grillType)
        {
            if (Enum.IsDefined(typeof(GrillType), (byte)grillType)) return grillType;
            switch ((int)grillType)
            {
                case 11:
                    return GrillType.Lock;
                case 12:
                    return GrillType.LockAds;
                case 13:
                    return GrillType.LockAndKey;
                case 14:
                    return GrillType.LockAndKey2;
                case 15:
                    return GrillType.Ice;
                case 16:
                    return GrillType.Lid;
                default:
                    return GrillType.Normal;
            }
        }
    }
}