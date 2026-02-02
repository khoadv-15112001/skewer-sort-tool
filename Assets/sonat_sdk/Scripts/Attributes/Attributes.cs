using System;
using UnityEngine;

namespace Sonat.Attributes
{
    public class ArrayElementTitleSonatSdkAttribute : PropertyAttribute
    {
        public readonly string varName;
        public ArrayElementTitleSonatSdkAttribute(string elementTitleVar)
        {
            varName = elementTitleVar;
        }
    }


    public class ArrayElementTitleAttribute : PropertyAttribute
    {
        public readonly string varName;
        public ArrayElementTitleAttribute(string elementTitleVar)
        {
            varName = elementTitleVar;
        }
    }


    public class ArrayElementIndexTitleAttribute2 : PropertyAttribute
    {
        public readonly EnumList enumList;
        public ArrayElementIndexTitleAttribute2(string builtInEnumType)
        {
            enumList = new EnumList(builtInEnumType);
        }

        public ArrayElementIndexTitleAttribute2(BuiltInEnumType builtInEnumType)
        {
            enumList = new EnumList(builtInEnumType.ToString());
        }
    }

    public class ArrayElementIndexTitleAttribute : PropertyAttribute
    {
        public readonly string KeyVarName;
        public readonly string IndexVarName;
        public readonly EnumList EnumList;
        public ArrayElementIndexTitleAttribute(string elementTitleKeyVar,BuiltInEnumType builtInEnumType,string indexName = null)
        {
            IndexVarName = indexName;
            KeyVarName = elementTitleKeyVar;
            EnumList = new EnumList(builtInEnumType.ToString());
        }

        public ArrayElementIndexTitleAttribute(string[] elementTitleVars, BuiltInEnumType builtInEnumType,
            string[] indexName = null)
        {
            if (indexName != null)
                IndexVarName = String.Join(".", indexName);
            else
                IndexVarName = null;
            KeyVarName = String.Join(".", elementTitleVars);
            EnumList = new EnumList(builtInEnumType.ToString());
        }
    
        public ArrayElementIndexTitleAttribute(string elementTitleKeyVar,string builtInEnumType,string indexName = null)
        {
            IndexVarName = indexName;
            KeyVarName = elementTitleKeyVar;
            EnumList = new EnumList(builtInEnumType);
        }
    
//    public ArrayElementIndexTitleAttribute(string elementTitleVar,string builtInEnumType,string[] indexName = null)
//    {
//        if (indexName != null)
//            varName2 = String.Join(".", indexName);
//        else
//            varName2 = null;
//        varName = elementTitleVar;
//        enumList = new EnumList(builtInEnumType);
//    }
//    
        public ArrayElementIndexTitleAttribute(string[] elementTitleVar,string builtInEnumType,string[] indexName = null)
        {
            if (indexName != null)
                IndexVarName = String.Join(".", indexName);
            else
                IndexVarName = null;
            KeyVarName = String.Join(".", elementTitleVar);
            EnumList = new EnumList(builtInEnumType);
        }

    
        public ArrayElementIndexTitleAttribute(string[] elementTitleVars,string builtInEnumType,string indexName = null)
        {
            IndexVarName = indexName;
            KeyVarName = String.Join(".",elementTitleVars);
            EnumList = new EnumList(builtInEnumType);
        }
    }


    public class ArrayElementTitleByNameAttribute : PropertyAttribute
    {
        public readonly string KeyVarName;
        public ArrayElementTitleByNameAttribute(string elementTitleKeyVar)
        {
            KeyVarName = elementTitleKeyVar;
        }
    }
}