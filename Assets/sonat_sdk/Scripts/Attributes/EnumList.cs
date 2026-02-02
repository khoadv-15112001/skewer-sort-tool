using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sonat.Attributes
{
    public class EnumList
    {
        public readonly string TypeName;
        public IList<GUIContent> Names;
        public int[] Values;
        public bool Found;
        public bool IsNotValid() => Values.HasDuplicate();

        public GUIContent GetName(int value)
        {
            if (Values == null)
                return new GUIContent("ErrNotFound_" + TypeName);
            for (var i = 0; i < Values.Length; i++)
            {
                if (Values[i] == value)
                    return Names[i];
            }

            return new GUIContent("ErrNotFound");
        }
        
        public int GetIntValue(string name)
        {
            if (Values == null)
                return -1;
            for (var i = 0; i < Values.Length; i++)
            {
                if (Names[i].text == name)
                    return Values[i];
            }

            return -1;
        }

        public string GetNameString(int value) => GetName(value).text;

        public string GetRawName(int value)
        {
            if (Values == null)
                return ("ErrNotFound_" + TypeName);
            for (var i = 0; i < Values.Length; i++)
            {
                if (Values[i] == value)
                    return Names[i].text;
            }

            return ("ErrNotFound");
        }

        public EnumList(Type enumType)
        {
            TypeName = enumType.Name;
            Type type = enumType;

            if (type.IsEnum)
            {
                Found = true;
                Names = Enum.GetNames(type).Select(x => new GUIContent(x)).ToList();
                Values = (int[])Enum.GetValues(type);
            }
#if UNITY_EDITOR

#endif
        }


        public EnumList(string enumTypeName)
        {
            TypeName = enumTypeName;
            Type type = Type.GetType(enumTypeName);

            if (type != null && type.IsEnum)
            {
                Found = true;
                Names = Enum.GetNames(type).Select(x => new GUIContent(x)).ToList();
                Values = (int[])Enum.GetValues(type);
            }
#if UNITY_EDITOR
            else
            {
                //var projectSettings = ProjectSettings.LoadProjectSettings();
                // = Type.GetType(projectSettings.nameSpace + "." + enumTypeName);
                //if (type == null)
                type = GetType(enumTypeName);

                if (type != null && type.IsEnum)
                {
                    Found = true;
                    Names = Enum.GetNames(type).Select(x => new GUIContent(x)).ToList();
                    Values = ((int[])Enum.GetValues(type));
                }
            }

            Type GetType(string className)
            {
                className = className.ToLower();
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.IsEnum)
                        {
                            if (type.Name.ToLower().Equals(className))
                            {
                                return type;
                            }
                        }
                    }
                }

                return null;
            }
#endif
        }

        public EnumList(string prefix, int take)
        {
            Found = true;
            Names = Enumerable.Range(0, take).Select(x => new GUIContent(prefix + x)).ToList();
            Values = Enumerable.Range(0, take).ToArray();
        }


        public void Reload(Type type)
        {
            if (type != null)
            {
                Found = true;
                Names = Enum.GetNames(type).Select(x => new GUIContent(x)).ToList();
                Values = ((int[])Enum.GetValues(type));
            }
        }

        public static GUIContent GetName(string enumTypeName, int value)
        {
            var list = new EnumList(enumTypeName);
            if (list.Values.Contains(value))
                return list.Names[list.Values.ToList().IndexOf(value)];
            return new GUIContent("not found");
        }
    }
}