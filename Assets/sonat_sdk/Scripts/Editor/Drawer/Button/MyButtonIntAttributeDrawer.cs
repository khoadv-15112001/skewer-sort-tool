using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sonat.Attributes;
using UnityEditor;
using UnityEngine;


namespace Sonat.Editor.Drawer.Button
{
    [CustomPropertyDrawer(typeof(MyButtonIntAttribute))]
    public class MyButtonIntAttributeDrawer : ClassicButtonDrawer
    {
        protected override List<Type[]> FindParams => new List<Type[]>
        {
            new Type[0],
            new[] { typeof(float) },
            new[] { typeof(bool) },
            new[] { typeof(int) },
            new[] { typeof(int), typeof(int) },
        };

        protected override void InvokeMethod(MethodInfo methodInfo, SerializedProperty property, int indexOfParam, object target,
            object[] parameters)
        {
            if (indexOfParam == 0 || indexOfParam == 3)
                base.InvokeMethod(methodInfo, property, indexOfParam, target, parameters);
            if (indexOfParam == 2)
                methodInfo.Invoke(target, new object[] { property.intValue != 0 });
            if (indexOfParam == 1)
                methodInfo.Invoke(target, new object[] { property.intValue / 100f });
            if (indexOfParam == 4)
            {
                var temp = property.serializedObject.FindProperty(property.propertyPath + ".value");
                var temp2 = property.serializedObject.FindProperty(property.propertyPath + ".value2");
                methodInfo.Invoke(target, new object[] { temp.intValue, temp2.intValue });
            }
        }

        public override object GetParams(SerializedProperty property, int i)
        {
            if (property.propertyType == SerializedPropertyType.Integer)
                return property.intValue;
            return null;
        }

        public override void DrawProperty(SerializedProperty property, Rect rect, GUIContent label)
        {
            base.DrawProperty(property, rect, label);
            var att = ((MyButtonIntAttribute)attribute);
            if (att.enumList == null)
            {
                property.intValue = EditorGUI.IntField(rect, property.intValue);
            }
            else
            {
                var keyProperty = property;

                if (!att.enumList.Found)
                    att.enumList.Reload(GetType(att.enumList.TypeName));

                if (keyProperty.propertyType != SerializedPropertyType.Integer)
                {
                    var temp = property.serializedObject.FindProperty(property.propertyPath + ".value");
                    if (temp.propertyType == SerializedPropertyType.Integer)
                    {
                        keyProperty = temp;

//                    rect.x -= 15;
                        float valueWidth = 45;
                        rect.width -= valueWidth;
                        var rect2 = rect;
                        rect2.x += rect.width + 2;
                        rect2.width = valueWidth - 2;
                        var valueProperty = property.serializedObject.FindProperty(property.propertyPath + ".value2");
                        valueProperty.intValue = EditorGUI.IntField(rect2, valueProperty.intValue);
                    }
                    else
                    {
                        EditorGUI.LabelField(rect, "not integer");
                        return;
                    }
                }

                if (att.enumList.Found && att.enumList.Values.Length > 0)
                {
                    if (keyProperty.propertyType == SerializedPropertyType.Integer)
                        IndexAsEnumDrawerHelper.DrawPropertyAsEnum(att.enumList, keyProperty, rect, null);
                }
                else
                {
                    label.text = label.text + $"({att.enumList.TypeName})";
                    EditorGUI.PropertyField(rect, property, label, true);
                }
            }
        }

        private static IList<T> ArrayAdd<T>(IList<T> list, T add)
        {
            return IeArrayAdd(list, add).ToArray();
        }

        private static IEnumerable<T> IeArrayAdd<T>(IEnumerable<T> list, T add)
        {
            foreach (var x1 in list)
                yield return x1;
            yield return add;
        }

        private Type GetType(string enumTypeName)
        {
            Type type = Type.GetType(enumTypeName);
            if (type != null && type.IsEnum)
            {
                return type;
            }
#if UNITY_EDITOR
            else
            {
                type = TryGetType(enumTypeName);
                if (type != null && type.IsEnum)
                    return type;
            }

            return null;
#endif
        }

        private static Type TryGetType(string className)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsEnum)
                    {
                        if (type.Name.Equals(className))
                        {
                            return type;
                        }
                    }
                }
            }

            return null;
        }
    }
}