using System;
using System.Linq;
using UnityEngine;

namespace Sonat.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public abstract class BaseButtonAttribute : PropertyAttribute
    {
        public string[] MethodNames { get; }
        public string[] ButtonNames { get; }

        protected BaseButtonAttribute(string[] methodNames, string[] buttonNames)
        {
            MethodNames = methodNames;
            ButtonNames = buttonNames;
        }

        protected BaseButtonAttribute(string[] methodNames)
        {
            MethodNames = methodNames;
            ButtonNames = methodNames;
        }

        protected BaseButtonAttribute(string methodNames)
        {
            MethodNames = new[] {methodNames};
            ButtonNames = new[] {methodNames};
        }

        protected BaseButtonAttribute(string methodNames1, string methodNames2)
        {
            MethodNames = new[] {methodNames1, methodNames2};
            ButtonNames = new[] {methodNames1, methodNames2};
        }

        protected BaseButtonAttribute(string methodNames1, string methodNames2, string methodNames3)
        {
            MethodNames = new[] {methodNames1, methodNames2, methodNames3};
            ButtonNames = new[] {methodNames1, methodNames2, methodNames3};
        }
    }



    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class MyButtonAttribute : BaseButtonAttribute
    {
        public MyButtonAttribute(string[] methodNames, string[] buttonNames) : base(methodNames,buttonNames) { }
        public MyButtonAttribute(string[] methodNames) : base(methodNames){ }
        public MyButtonAttribute(string methodNames): base(methodNames){ }
        public MyButtonAttribute(string methodNames1, string methodNames2) : base(methodNames1,methodNames2){ }
        public MyButtonAttribute(string methodNames1, string methodNames2, string methodNames3) : base(methodNames1,methodNames2,methodNames3){ }
    }



//[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
//public class MyButtonLogAttribute : BaseButtonAttribute
//{
//    public MyButtonLogAttribute(string[] methodNames, string[] buttonNames) : base(methodNames,buttonNames) { }
//    public MyButtonLogAttribute(string[] methodNames) : base(methodNames){ }
//    public MyButtonLogAttribute(string methodNames): base(methodNames){ }
//    public MyButtonLogAttribute(string methodNames1, string methodNames2) : base(methodNames1,methodNames2){ }
//    public MyButtonLogAttribute(string methodNames1, string methodNames2, string methodNames3) : base(methodNames1,methodNames2,methodNames3){ }
//}


    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class MyButtonStringAttribute : BaseButtonAttribute
    {
        public MyButtonStringAttribute(string[] methodNames, string[] buttonNames) : base(methodNames,buttonNames) { }
        public MyButtonStringAttribute(string[] methodNames) : base(methodNames){ }
        public MyButtonStringAttribute(string methodNames): base(methodNames){ }
        public MyButtonStringAttribute(string methodNames1, string methodNames2) : base(methodNames1,methodNames2){ }
        public MyButtonStringAttribute(string methodNames1, string methodNames2, string methodNames3) : base(methodNames1,methodNames2,methodNames3){ }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class MyButtonIntAttribute : BaseButtonAttribute
    {
        public readonly EnumList enumList;

        public MyButtonIntAttribute(string[] methodNames, string[] buttonNames) : base(methodNames,buttonNames) { }
        public MyButtonIntAttribute(string[] methodNames) : base(methodNames){ }

        public MyButtonIntAttribute(string[] methodNames, string enumType) : base(methodNames)
        {
            enumList = new EnumList(enumType);
        }
    
        public MyButtonIntAttribute(string methodNames): base(methodNames){ }
        public MyButtonIntAttribute(string methodNames1, string methodNames2) : base(methodNames1,methodNames2){ }
        public MyButtonIntAttribute(string methodNames1, string methodNames2, string methodNames3) : base(methodNames1,methodNames2,methodNames3){ }
    
        //
        public MyButtonIntAttribute(string[] methodNames, string[] buttonNames,BuiltInEnumType enumTypeName) : base(methodNames,buttonNames) 
        {
            enumList = new EnumList(enumTypeName.ToString());
        }

        public MyButtonIntAttribute(string[] methodNames,BuiltInEnumType enumTypeName): base(methodNames) 
        {
            enumList = new EnumList(enumTypeName.ToString());
        }

        public MyButtonIntAttribute(string methodNames,BuiltInEnumType enumTypeName): base(methodNames) 
        {
            enumList = new EnumList(enumTypeName.ToString());
        }

        public MyButtonIntAttribute(string methodNames1, string methodNames2,BuiltInEnumType enumTypeName): base(methodNames1,methodNames2) 
        {
            enumList = new EnumList(enumTypeName.ToString());
        }

        public MyButtonIntAttribute(string methodNames1, string methodNames2, string methodNames3,BuiltInEnumType enumTypeName): base(methodNames1,methodNames2,methodNames3) 
        {
            enumList = new EnumList(enumTypeName.ToString());
        }
    }


    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class MyButtonIntParameterPresetAttribute : BaseButtonAttribute
    {
        public string Prefix { get; }
        public int[] Parameter { get; }
        public string[] Names { get; }
    
        public MyButtonIntParameterPresetAttribute(string methodNames,string prefix, int[] parameters,string[] names = null) : base(methodNames)
        {
            Prefix = prefix;
            Parameter = parameters;
            Names = names;
        }
    
        public MyButtonIntParameterPresetAttribute(string methodNames,string enumType) : base(methodNames)
        {
            Prefix = "";
            var enumList = new EnumList(enumType);
            Parameter = enumList.Values;
            Names = enumList.Names.Select(x => x.text).ToArray();
        }
    
        public MyButtonIntParameterPresetAttribute(string methodNames,Type enumType) : base(methodNames)
        {
            Prefix = "";
            var enumList = new EnumList(enumType);
            Parameter = enumList.Values;
            Names = enumList.Names.Select(x => x.text).ToArray();
        }
    }



    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class MyButtonObjectAttribute : BaseButtonAttribute
    {
        public MyButtonObjectAttribute(string[] methodNames, string[] buttonNames) : base(methodNames, buttonNames)
        {
        }

        public MyButtonObjectAttribute(string[] methodNames) : base(methodNames)
        {
        }

        public MyButtonObjectAttribute(string methodNames) : base(methodNames)
        {
        }

        public MyButtonObjectAttribute(string methodNames1, string methodNames2) : base(methodNames1, methodNames2)
        {
        }

        public MyButtonObjectAttribute(string methodNames1, string methodNames2, string methodNames3) : base(methodNames1,
            methodNames2, methodNames3)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class MyButtonVector3Attribute : BaseButtonAttribute
    {
        public MyButtonVector3Attribute(string[] methodNames, string[] buttonNames) : base(methodNames, buttonNames)
        {
        }

        public MyButtonVector3Attribute(string[] methodNames) : base(methodNames)
        {
        }

        public MyButtonVector3Attribute(string methodNames) : base(methodNames)
        {
        }

        public MyButtonVector3Attribute(string methodNames1, string methodNames2) : base(methodNames1, methodNames2)
        {
        }

        public MyButtonVector3Attribute(string methodNames1, string methodNames2, string methodNames3) : base(methodNames1,
            methodNames2, methodNames3)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class MyButtonIntPageSelectAttribute : BaseButtonAttribute
    {
        public string NameOfListOrCountProperty { get; set; }

        public MyButtonIntPageSelectAttribute(string methodName1, string nameOfListOrCountProperty) : base(methodName1)
        {
            NameOfListOrCountProperty = nameOfListOrCountProperty;
        }


        public MyButtonIntPageSelectAttribute(string[] methodNames, string nameOfListOrCountProperty) : base(methodNames)
        {
            NameOfListOrCountProperty = nameOfListOrCountProperty;
        }

        public MyButtonIntPageSelectAttribute(string[] methodNames, string[] buttonNames, string nameOfListOrCountProperty) : base(methodNames,
            buttonNames)
        {
            NameOfListOrCountProperty = nameOfListOrCountProperty;
        }

        public MyButtonIntPageSelectAttribute(string methodName1, string methodName2, string nameOfListOrCountProperty) : base(methodName1,
            methodName2)
        {
            NameOfListOrCountProperty = nameOfListOrCountProperty;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class MyButtonToggleSelfAttribute : PropertyAttribute
    {
        public string WhenOn;
        public string WhenOff;

        public MyButtonToggleSelfAttribute(string whenOn, string whenOff)
        {
            WhenOn = whenOn;
            WhenOff = whenOff;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class MyButtonToggleEnumSelfAttribute : PropertyAttribute
    {
        public string Prefix { get; }
        public int[] Parameter { get; }
        public string[] Names { get; }
 

        public MyButtonToggleEnumSelfAttribute(Type enumType)
        {
            Prefix = "";
            var enumList = new EnumList(enumType);
            Parameter = enumList.Values;
            Names = enumList.Names.Select(x => x.text).ToArray();
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class MyButtonToggleAttribute : BaseButtonAttribute
    {
        public MyButtonToggleAttribute(string[] methodNames, string[] buttonNames) : base(methodNames,buttonNames) { }
        public MyButtonToggleAttribute(string[] methodNames) : base(methodNames){ }
        public MyButtonToggleAttribute(string methodNames): base(methodNames){ }
        public MyButtonToggleAttribute(string methodNames1, string methodNames2) : base(methodNames1,methodNames2){ }
        public MyButtonToggleAttribute(string methodNames1, string methodNames2, string methodNames3) : base(methodNames1,methodNames2,methodNames3){ }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class MyButtonFloatAttribute : BaseButtonAttribute
    {
        public MyButtonFloatAttribute(string[] methodNames, string[] buttonNames) : base(methodNames,buttonNames) { }
        public MyButtonFloatAttribute(string[] methodNames) : base(methodNames){ }
        public MyButtonFloatAttribute(string methodNames): base(methodNames){ }
        public MyButtonFloatAttribute(string methodNames1, string methodNames2) : base(methodNames1,methodNames2){ }
        public MyButtonFloatAttribute(string methodNames1, string methodNames2, string methodNames3) : base(methodNames1,methodNames2,methodNames3){ }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class MyButtonFloatSliderAttribute : BaseButtonAttribute
    {
        public MyButtonFloatSliderAttribute(string[] methodNames, string[] buttonNames) : base(methodNames,buttonNames) { }
        public MyButtonFloatSliderAttribute(string[] methodNames) : base(methodNames){ }
        public MyButtonFloatSliderAttribute(string methodNames): base(methodNames){ }
        public MyButtonFloatSliderAttribute(string methodNames1, string methodNames2) : base(methodNames1,methodNames2){ }
        public MyButtonFloatSliderAttribute(string methodNames1, string methodNames2, string methodNames3) : base(methodNames1,methodNames2,methodNames3){ }
    }



    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class MyButtonRangeAttribute : BaseButtonAttribute
    {
        public int From { get; private set; }
        public int To { get; private set; }
        public string Prefix { get; private set; }
        public string[] Names { get; private set; }
        public int[] Colors { get; private set; }
        public string Label { get; private set; }

        public MyButtonRangeAttribute(int from, int to, string methodNames,string[] names = null, string prefix = "",string label = "") : base(methodNames)
        {
            From = from;
            To = to;
            Prefix = prefix;
            Names = names;
            Label = label;
        }
    
        public MyButtonRangeAttribute(int[] colors,int from, int to, string methodNames,string[] names = null, string prefix = "",string label = "") : base(methodNames)
        {
            From = from;
            To = to;
            Prefix = prefix;
            Names = names;
            Label = label;
            Colors = colors;
        }
    }


    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class MyButtonVector2IntAttribute : BaseButtonAttribute
    {
        public MyButtonVector2IntAttribute(string[] methodNames, string[] buttonNames) : base(methodNames, buttonNames)
        {
        }

        public MyButtonVector2IntAttribute(string[] methodNames) : base(methodNames)
        {
        }

        public MyButtonVector2IntAttribute(string methodNames) : base(methodNames)
        {
        }

        public MyButtonVector2IntAttribute(string methodNames1, string methodNames2) : base(methodNames1, methodNames2)
        {
        }

        public MyButtonVector2IntAttribute(string methodNames1, string methodNames2, string methodNames3) : base(
            methodNames1, methodNames2, methodNames3)
        {
        }
    }
}