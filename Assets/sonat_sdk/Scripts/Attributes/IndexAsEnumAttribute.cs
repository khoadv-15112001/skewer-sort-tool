using System;
using System.Linq;
using UnityEngine;

namespace Sonat.Attributes
{
    public static class BuiltInEnumTypeHelper
    {
        public static int ToUniversalKey(this BuiltInEnumType builtInEnumType, int index)
        {
            return (int) builtInEnumType * 10000 + index;
        }
    }

    public enum BuiltInEnumType
    {
        Quantity = -3,
        GameMainEvent = -2,
        GameActionEvent = -1,

        // ReSharper disable once InconsistentNaming
        NONE = 0,
        EffectEnum = 1,
        ShopItemKey = 2,
        MiscEnum = 3,
        QuestEnum = 4,
        AchievementEnum = 5,
        AdsItemKey = 6,
        RewardEnum = 7,
        SortingEnum = 8,
        SubscriptionKey = 9,
        ProductSystemAction = 10,
        ProductGameAction = 11,
        ItemTypeEnum = 12,
        InScreenPopupEnum = 13,
        CustomGameCondition = 14,
        ToggleEventEnum = 15,
        SoundEnum = 16,
        MusicEnum = 17,
        GameRootButtonEnum = 18,
        GameState = 19,
        MiniPopEnum = 20,
        NavigationEnum = 21,
        NavigateDialogEnum = 22,
        ToggleLink = 23,
        ScreenEnum = 24,
        PoolItemEnum = 25,
        TextUpdateKey = 26,
        DifficultyLevel = 27,
        GameCheckerEnum = 28,
        GlobalCallBackEnum = 29,
        ParameterGameActionEnum = 30,
        CustomPlayerDataProperty = 31,
        CustomDailyDataProperty = 32,
        CustomWeeklyDataProperty = 33,
        CustomSessionDataProperty = 34,
        CustomWaveDataProperty = 35,
        GlobalError = 36,
        SkillEnum = 37,
        GameSaveKey = 38,
        TutorialActionEnum = 39,
        TutorialActionParam = 40,
        TutorialEnableFlag = 41,
        TutorialMiniAction = 42,
        LogPlacement = 43,
        LogItemType = 44,
        RewardTypeFlag = 45,
        RemoteConfigKey = 46,
        TutorialSkipType = 47,
        Shortcut = 48,
        AnimationTriggerEnum = 49,
        AnimationSequenceEnum = 50,
        CustomProductEnum = 51,
        CustomTimeSettingEnum = 52,
        QuestTabEnum = 53,
        UpgradeTabEnum = 54,
        ShopTabEnum = 55,
        TutorialButtonEnum = 56,
        EventEnum = 57,
        LocationEnum = 58,
        GameLocationEnum = 59,
        PlayMode = 60,
        RandomItem = 61,
        UnlockConditionEnum = 62,
        AnimationSpriteSheetEnum = 63,
        CurveEnum = 64,
        CustomTimeTick = 65,
        UnlockEquipDataEnum = 66,
        CongratulationEnum = 67,
        ProductIndexEventEnum = 68,
        CoolDownKey = 69,
        GcCheckerEnum = 70,
        CustomPlayerArrayIntProperty = 71,
        CustomDailyArrayIntProperty = 72,
        CustomWeeklyArrayIntProperty = 73,
        CustomSessionArrayIntProperty = 74,
        CustomWaveArrayIntProperty = 75,
        GameSaveArrayIntKey = 76,
        InformEnum = 77,
        SpriteListEnum = 78,
        TutorialPopupKey = 79,
        FrameworkSharedData = 80,
        PlayerPrefEnum = 81,
        GameSettingSharedData = 82,
        FrameworkSharedObject = 83,
        BuiltInAction = 84,
        ActionLink = 85, // in screen root
        GameConditionMode = 86, // moves, time, others....
        PlayerDataStock = 87,// block
        CustomQuantity = 88,
        SessionSharedString = 89,
        RegisterAction = 90,
        CustomDataEnumShortcut = 91,
        RandomLevelType = 92,
        BuiltInValidEnum = 93,
        ButtonPointAction = 94,
        IntroduceEnum = 95,
    }

    public enum EnumFindBy
    {
        BuiltInEnumType,
        Name
    }

    public enum BaseBuiltInAction
    {
        StartGame = 0,
        FocusUnfinishedBuilder = 1,
    }


    public abstract class DuongPropertyAttribute : PropertyAttribute
    {
        public int LabelColor = -1;
    }

    public class IndexParam : DuongPropertyAttribute
    {
        public readonly string IndexPropertyName;

        public IndexParam(string indexPropertyName)
        {
            IndexPropertyName = indexPropertyName;
        }
    }


    public class IndexAsEnumConditionBaseAttribute : DuongPropertyAttribute
    {
        public  string PropertyToCheck;
        public  object[] CompareValues;
        public  EnumList[] EnumList;
    }


    [AttributeUsage(AttributeTargets.Field)]
    public class IndexAsEnumMultipleAttribute : IndexAsEnumConditionBaseAttribute
    {


        public IndexAsEnumMultipleAttribute(object[] compareValue, string[] correspondEnumTypes, string propertyToCheck,
            int color = -1)
        {
            PropertyToCheck = propertyToCheck;
            CompareValues = compareValue;
            EnumList = correspondEnumTypes.Select(x => new EnumList(x)).ToArray();
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class IndexAsEnumMultipleNameAttribute : IndexAsEnumConditionBaseAttribute
    {
        public IndexAsEnumMultipleNameAttribute(object[] compareValue, string[] prefix, int[] take, string propertyToCheck,
            int color = -1)
        {
            PropertyToCheck = propertyToCheck;
            CompareValues = compareValue;
            EnumList = new EnumList[prefix.Length];
            for (var i = 0; i < EnumList.Length; i++)
                EnumList[i] = new EnumList(prefix[i],take[i]);
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class IndexAsEnumConditionAttribute : DuongPropertyAttribute
    {
        public readonly string PropertyToCheck;
        public readonly object[] CompareValues;
        public readonly EnumList[] EnumLists;

        public IndexAsEnumConditionAttribute(string propertyToCheck, object[] compareValue, string[] enumType,
            int color = -1)
        {
            this.PropertyToCheck = propertyToCheck;
            CompareValues = compareValue;
            EnumLists = enumType.Select(x => new EnumList(x)).ToArray();
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class IndexAsEnumFlagConditionAttribute : DuongPropertyAttribute
    {
        public readonly string PropertyToCheck;
        public readonly object[] CompareValues;
        public readonly EnumList[] EnumList;

        public IndexAsEnumFlagConditionAttribute(object[] compareValue, string[] correspondEnumTypes,
            string propertyToCheck, int color = -1)
        {
            this.PropertyToCheck = propertyToCheck;
            CompareValues = compareValue;
            EnumList = correspondEnumTypes.Select(x => new EnumList(x)).ToArray();
        }
    }


    public class IndexAsEnumFlagAttribute : DuongPropertyAttribute
    {
        public readonly EnumList EnumList;

        public IndexAsEnumFlagAttribute(string enumTypeName, int color = -1)
        {
            LabelColor = color;
            EnumList = new EnumList(enumTypeName);
        }

        public IndexAsEnumFlagAttribute(BuiltInEnumType enumTypeName, int color = -1)
        {
            LabelColor = color;
            EnumList = new EnumList(enumTypeName.ToString());
        }
    }

    public class IntRxAsEnumFlagAttribute : DuongPropertyAttribute
    {
        public readonly EnumList EnumList;

        public IntRxAsEnumFlagAttribute(string enumTypeName, int color = -1)
        {
            LabelColor = color;
            EnumList = new EnumList(enumTypeName);
        }

        public IntRxAsEnumFlagAttribute(BuiltInEnumType enumTypeName, int color = -1)
        {
            LabelColor = color;
            EnumList = new EnumList(enumTypeName.ToString());
        }
    }


    public class IntRxAsEnumAttribute : DuongPropertyAttribute
    {
        public readonly EnumList EnumList;

        public IntRxAsEnumAttribute(string enumTypeName, int color = -1)
        {
            LabelColor = color;
            EnumList = new EnumList(enumTypeName);
        }

        public IntRxAsEnumAttribute(BuiltInEnumType enumTypeName, int color = -1)
        {
            LabelColor = color;
            EnumList = new EnumList(enumTypeName.ToString());
        }
    }

    public class IndexAsEnumFromPropertyAttribute : DuongPropertyAttribute
    {
        public readonly string PropertyToCheck;

        public IndexAsEnumFromPropertyAttribute(string propertyToCheck)
        {
            this.PropertyToCheck = propertyToCheck;
        }
    }

    public class IndexAsEnumSonatSdkAttribute : DuongPropertyAttribute
    {
        public readonly EnumList[] EnumList;
        public readonly bool Disable;

        public IndexAsEnumSonatSdkAttribute(string enumTypeName, int color = -1)
        {
            LabelColor = color;
            EnumList = new[] {new EnumList(enumTypeName)};
        }


        public IndexAsEnumSonatSdkAttribute(BuiltInEnumType enumTypeName, int color = -1)
        {
            LabelColor = color;
            EnumList = new[] {new EnumList(enumTypeName.ToString())};
        }

        public IndexAsEnumSonatSdkAttribute(BuiltInEnumType[] enumTypeNames, int color = -1)
        {
            LabelColor = color;
            EnumList = new EnumList[enumTypeNames.Length];
            for (var i = 0; i < EnumList.Length; i++)
            {
                if (enumTypeNames[i] != BuiltInEnumType.NONE)
                    EnumList[i] = new EnumList(enumTypeNames[i].ToString());
            }
        }

        public IndexAsEnumSonatSdkAttribute(string[] enumTypeNames, int color = -1)
        {
            LabelColor = color;
            EnumList = new EnumList[enumTypeNames.Length];
            for (var i = 0; i < EnumList.Length; i++)
            {
                if (!string.IsNullOrEmpty(enumTypeNames[i]))
                    EnumList[i] = new EnumList(enumTypeNames[i]);
            }
        }


        public IndexAsEnumSonatSdkAttribute(string enumTypeName, bool disable, int color = -1)
        {
            LabelColor = color;
            EnumList = new[] {new EnumList(enumTypeName)};
            this.Disable = disable;
        }

        public IndexAsEnumSonatSdkAttribute(BuiltInEnumType enumTypeName, bool disable, int color = -1)
        {
            LabelColor = color;
            EnumList = new[] {new EnumList(enumTypeName.ToString())};
            this.Disable = disable;
        }
    }


    public class IndexAsEnumAttribute : DuongPropertyAttribute
    {
        public readonly EnumList[] EnumList;
        public readonly bool Disable;

        public IndexAsEnumAttribute(string enumTypeName, int color = -1)
        {
            LabelColor = color;
            EnumList = new[] {new EnumList(enumTypeName)};
        }


        public IndexAsEnumAttribute(BuiltInEnumType enumTypeName, int color = -1)
        {
            LabelColor = color;
            EnumList = new[] {new EnumList(enumTypeName.ToString())};
        }

        public IndexAsEnumAttribute(BuiltInEnumType[] enumTypeNames, int color = -1)
        {
            LabelColor = color;
            EnumList = new EnumList[enumTypeNames.Length];
            for (var i = 0; i < EnumList.Length; i++)
            {
                if (enumTypeNames[i] != BuiltInEnumType.NONE)
                    EnumList[i] = new EnumList(enumTypeNames[i].ToString());
            }
        }

        public IndexAsEnumAttribute(string[] enumTypeNames, int color = -1)
        {
            LabelColor = color;
            EnumList = new EnumList[enumTypeNames.Length];
            for (var i = 0; i < EnumList.Length; i++)
            {
                if (!string.IsNullOrEmpty(enumTypeNames[i]))
                    EnumList[i] = new EnumList(enumTypeNames[i]);
            }
        }


        public IndexAsEnumAttribute(string enumTypeName, bool disable, int color = -1)
        {
            LabelColor = color;
            EnumList = new[] {new EnumList(enumTypeName)};
            this.Disable = disable;
        }

        public IndexAsEnumAttribute(BuiltInEnumType enumTypeName, bool disable, int color = -1)
        {
            LabelColor = color;
            EnumList = new[] {new EnumList(enumTypeName.ToString())};
            this.Disable = disable;
        }
    }

    public class EditEnumPointAttribute : DuongPropertyAttribute
    {
        public readonly string PointPropertyName;
        public readonly int ValueIndex;

        public EditEnumPointAttribute(int valueIndex,string pointPropertyName = "point")
        {
            this.PointPropertyName = pointPropertyName;
            this.ValueIndex = valueIndex;
        }
    }


    public class IntStringAsEnumKeyAttribute : DuongPropertyAttribute
    {
        public IntStringAsEnumKeyAttribute()
        {
     
        }
    }


    public class IndexByBuiltInEnumAttribute : DuongPropertyAttribute
    {
        public readonly string PropertyToCheck;
        public readonly EnumFindBy EnumFindBy;

        public IndexByBuiltInEnumAttribute(string propertyToCheck, EnumFindBy findBy = EnumFindBy.BuiltInEnumType,
            int color = -1)
        {
            LabelColor = color;
            this.PropertyToCheck = propertyToCheck;
            EnumFindBy = findBy;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class DisplayStringAsLabelAttribute : DuongPropertyAttribute
    {
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class DisplayAsDisableAttribute : DuongPropertyAttribute
    {
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class InLineAttribute : PropertyAttribute
    {
        public float FirstElementWidth;

        public string[] RelativePropertyNames { get; set; }
//    public InLineAttribute(string[] names)
//    {
//        RelativePropertyNames = names;
//    }

        public InLineAttribute(string names, float firstElementWidth = -1)
        {
            FirstElementWidth = firstElementWidth;
            RelativePropertyNames = new[] {names};
        }

        public InLineAttribute(string names1, string names2, float firstElementWidth = -1)
        {
            FirstElementWidth = firstElementWidth;
            RelativePropertyNames = new[] {names1, names2};
        }

        public InLineAttribute(string names1, string names2, string names3, float firstElementWidth = -1)
        {
            FirstElementWidth = firstElementWidth;
            RelativePropertyNames = new[] {names1, names2, names3};
        }
    }

    public class LabelColorAttribute : PropertyAttribute
    {
        public float R { get; private set; }
        public float G { get; private set; }
        public float B { get; private set; }
    
        public string AddName  { get; private set; }

        public LabelColorAttribute(float r = 0.5f, float g = 0.5f, float b = 0.5f,string addNameName = null)
        {
            R = r;
            G = g;
            B = b;
            AddName = addNameName;
        }
    }

    public class LabelHeaderAttribute : PropertyAttribute
    {
        public float R { get; private set; }
        public float G { get; private set; }
        public float B { get; private set; }
        public string text { get; private set; }

        public LabelHeaderAttribute(string txt, float r = 0.5f, float g = 0.5f, float b = 0.5f)
        {
            R = r;
            G = g;
            B = b;
            text = txt;
        }
    }


    public class SplitHeaderAttribute : PropertyAttribute
    {
    }
}