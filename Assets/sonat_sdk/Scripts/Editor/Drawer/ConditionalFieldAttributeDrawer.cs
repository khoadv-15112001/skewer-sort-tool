using System;
using System.Linq;
using Sonat.Attributes;
using UnityEditor;
using UnityEngine;

namespace Sonat.Editor.Drawer
{
    public static class ConditionalFieldAttributeDrawerHelper
    {
        /// <summary>
        /// Get string representation of serialized property, even for non-string fields
        /// </summary>
        public static string AsStringValue(this SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                    return property.stringValue;
                case SerializedPropertyType.Character:
                case SerializedPropertyType.Integer:
                    if (property.type == "char") return System.Convert.ToChar(property.intValue).ToString();
                    return property.intValue.ToString();
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue != null ? property.objectReferenceValue.ToString() : "null";
                case SerializedPropertyType.Boolean:
                    return property.boolValue.ToString();
                case SerializedPropertyType.Enum:
                    if (property.enumValueIndex < 0 || property.enumValueIndex >= property.enumNames.Length)
                        return string.Empty;
                    return property.enumNames[property.enumValueIndex];
                default:
                    return string.Empty;
            }
        }

        public static bool CompareMatch(SerializedProperty property, string propertyPath, object[] compareValues,
            out int indexMatch,out SerializedProperty relativeProperty)
        {
            indexMatch = -1;
            relativeProperty = null;
            if (string.IsNullOrEmpty(propertyPath))
            {
                Debug.LogError(nameof(propertyPath) + " is null");
                return false;
            }

            relativeProperty = FindPropertyRelative(property, propertyPath);

            if (relativeProperty != null)
            {
                bool isBoolMatch = relativeProperty.propertyType == SerializedPropertyType.Boolean &&
                                   relativeProperty.boolValue;

                if (isBoolMatch)
                    return true;

                if (compareValues == null)
                    return false;

                string[] compareStringValues = null;
                if (compareValues.Length > 0)
                {
                    compareStringValues = new string[compareValues.Length];
                    for (int i = 0; i < compareStringValues.Length; i++)
                        compareStringValues[i] =
                            compareValues[i] != null ? compareValues[i].ToString().ToUpper() : "NULL";
                }

                if (compareStringValues != null)
                {
                    string conditionPropertyStringValue = relativeProperty.AsStringValue().ToUpper();
                    for (var i = 0; i < compareStringValues.Length; i++)
                        if (compareStringValues[i] == conditionPropertyStringValue)
                        {
                            indexMatch = i;
                            return true;
                        }
                }
            }

            return false;
        }

        public static SerializedProperty FindPropertyRelative(SerializedProperty property, string propertyPath)
        {
            if (property.depth == 0) return property.serializedObject.FindProperty(propertyPath);

            var path = property.propertyPath.Replace(".Array.data[", "[");
            var elements = path.Split('.');
            SerializedProperty parent = null;
            for (int i = 0; i < elements.Length - 1; i++)
            {
                var element = elements[i];
                int index = -1;
                if (element.Contains("["))
                {
                    index = Convert.ToInt32(element.Substring(element.IndexOf("[", StringComparison.Ordinal))
                        .Replace("[", "").Replace("]", ""));
                    element = element.Substring(0, element.IndexOf("[", StringComparison.Ordinal));
                }

                parent = i == 0 ? property.serializedObject.FindProperty(element) : parent.FindPropertyRelative(element);

                if (index >= 0) parent = parent.GetArrayElementAtIndex(index);
            }


            return parent.FindPropertyRelative(propertyPath);
        }
    }


    public abstract class BaseConditionAttributeDrawer : PropertyDrawer
    {
        protected abstract string PropertyToCheck { get; }
        protected abstract object[] CompareValues { get; }

        protected virtual bool ForceValidToShow => false;

        protected virtual bool CheckIsInteger => false;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (CheckIsInteger && property.propertyType != SerializedPropertyType.Integer)
            {
                EditorGUI.LabelField(position, label.text, "Use PopUp for Integer.");
                return;
            }

            var validToShow = ConditionalFieldAttributeDrawerHelper.CompareMatch(property, PropertyToCheck, CompareValues, out int indexMatch,out SerializedProperty checkProperty) || ForceValidToShow ;

            OnGUIDraw(position, property, label, validToShow, indexMatch, checkProperty);
        }

        protected abstract void OnGUIDraw(Rect position, SerializedProperty property, GUIContent label, bool validToShow,
            int indexMatch, SerializedProperty checkProperty);
    }

    public abstract class BaseIndexAsEnumConditionAttributeDrawer : BaseConditionAttributeDrawer
    {
        protected override bool CheckIsInteger => true;

        protected override void OnGUIDraw(Rect position, SerializedProperty property, GUIContent label, bool validToShow,
            int indexMatch,
            SerializedProperty checkProperty)
        {
            if (!validToShow)
            {
                GUI.enabled = false;
                EditorGUI.PropertyField(position, property, label, true);
                GUI.enabled = true;
                return;
            }

            var enumList = GetEnumList(checkProperty, indexMatch);
            if (enumList.Found && enumList.Values.Length > 0)
                DrawEnum(property, position, label, enumList);
            else
            {
                label.text = label.text + $"({enumList.TypeName})";
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        protected abstract EnumList GetEnumList(SerializedProperty checkProperty, int indexMatch);

        protected abstract void DrawEnum(SerializedProperty property, Rect position, GUIContent label, EnumList enumList);
    }

    [CustomPropertyDrawer(typeof(IndexAsEnumConditionAttribute))]
    public class IndexAsEnumConditionAttributeDrawer : BaseIndexAsEnumConditionAttributeDrawer
    {
        private IndexAsEnumConditionAttribute _attribute;
        private IndexAsEnumConditionAttribute Attribute => _attribute ??= attribute as IndexAsEnumConditionAttribute;

        protected override string PropertyToCheck => Attribute != null ? _attribute.PropertyToCheck : null;
        protected override object[] CompareValues => Attribute != null ? _attribute.CompareValues : null;
    
        protected override EnumList GetEnumList(SerializedProperty checkProperty,int indexMatch)
        {
            return Attribute.EnumLists[indexMatch];
        }
        protected override void DrawEnum(SerializedProperty property, Rect position, GUIContent label, EnumList enumList)
        {
            property.intValue = EditorGUI.IntPopup(position, label, property.intValue,
                enumList.Names.Select(x => new GUIContent(x)).ToArray(), enumList.Values.ToArray());
        }
    }


    [CustomPropertyDrawer(typeof(IndexAsEnumFromPropertyAttribute))]
    public class IndexAsEnumFromPropertyAttributeDrawer : BaseIndexAsEnumConditionAttributeDrawer
    {
        private IndexAsEnumFromPropertyAttribute _attribute;
        private IndexAsEnumFromPropertyAttribute Attribute => _attribute ??= attribute as IndexAsEnumFromPropertyAttribute;

        protected override string PropertyToCheck => Attribute != null ? _attribute.PropertyToCheck : null;
        protected override object[] CompareValues => null;

        protected override bool ForceValidToShow => true;

        protected override EnumList GetEnumList(SerializedProperty checkProperty,int indexMatch)
        {
            return new EnumList(((BuiltInEnumType) checkProperty.intValue).ToString());
        }
    
        protected override void DrawEnum(SerializedProperty property, Rect position, GUIContent label, EnumList enumList)
        {
            property.intValue = EditorGUI.IntPopup(position, label, property.intValue,
                enumList.Names.Select(x => new GUIContent(x)).ToArray(), enumList.Values.ToArray());
        }
    }


    [CustomPropertyDrawer(typeof(ConditionalFieldAttribute))]
    public class ConditionalFieldAttributeDrawer : BaseConditionAttributeDrawer
    {
        private ConditionalFieldAttribute _attribute;
        private ConditionalFieldAttribute Attribute => _attribute ??= attribute as ConditionalFieldAttribute;

        protected override string PropertyToCheck => Attribute != null ? _attribute.PropertyToCheck : null;

        protected override object[] CompareValues => Attribute != null ? _attribute.CompareValues : null;

        private bool Enable => Attribute != null && _attribute.Enable;
        private bool Reverse => Attribute != null && _attribute.Reverse;

        private bool _toShow = true;


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (Enable)
                return EditorGUI.GetPropertyHeight(property);
            return _toShow ? EditorGUI.GetPropertyHeight(property) : 0;
        }


        protected override void OnGUIDraw(Rect position, SerializedProperty property, GUIContent label, bool validToShow, int indexMatch,
            SerializedProperty checkProperty)
        {
            _toShow = validToShow;
            if (!_toShow && !Enable)
                return;
            if (Enable)
                GUI.enabled = Reverse ? !validToShow : validToShow;
            EditorGUI.PropertyField(position, property, label, true);
            if (Enable)
                GUI.enabled = true; 
        }
    }


    [CustomPropertyDrawer(typeof(ConditionLabelAttribute))]
    public class CustomNameAttributeDrawer : BaseConditionAttributeDrawer
    {
        private ConditionLabelAttribute _attribute;
        private ConditionLabelAttribute Attribute => _attribute ??= attribute as ConditionLabelAttribute;

        protected override string PropertyToCheck => Attribute != null ? _attribute.PropertyToCheck : null;
   
        protected override  object[] CompareValues => Attribute != null ? _attribute.Conditions : null;
        private string[] Displays => Attribute != null ? _attribute.Labels : null;


        protected override void OnGUIDraw(Rect position, SerializedProperty property, GUIContent label, bool validToShow, int indexMatch,
            SerializedProperty checkProperty)
        {
            if (validToShow)
                label.text = Displays[indexMatch];
            EditorGUI.PropertyField(position, property, label, true);
        }
    }
}