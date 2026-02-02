using System.Linq;
using Sonat.Attributes;
using Sonat.Editor.Drawer.Button;
using UnityEditor;
using UnityEngine;

namespace Sonat.Editor.Drawer
{
    [CustomPropertyDrawer(typeof(IndexAsEnumSonatSdkAttribute))]
    public class IndexAsEnumAttributeDrawerSonatSdk : SonatButtonDrawer
    {
        private int _index = -1;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.Integer)
            {
                var att = ((IndexAsEnumSonatSdkAttribute) attribute);
                if (att.EnumList[0].Found && att.EnumList[0].Values.Length > 0)
                {
                    _index = -1;
                    for (var i = 0; i < att.EnumList[0].Values.Length; i++)
                    {
                        if (property.intValue == att.EnumList[0].Values[i])
                        {
                            _index = i;
                            break;
                        }
                    }

                    var prevIndex = _index;
                    _index = EditorGUI.Popup(position, property.displayName, Mathf.Max(0, _index),att.EnumList[0].Names.Select(x => x.text).ToArray());
                    if (prevIndex != _index)
                        property.intValue = att.EnumList[0].Values[_index];
                }
                else
                {
                    label.text = label.text + $"({att.EnumList[0].TypeName})";
                    EditorGUI.PropertyField(position, property, label, true);
                }
            }
            else
                EditorGUI.LabelField(position, label.text, "Use PopUp for Integer.");
        }
    }

    [CustomPropertyDrawer(typeof(ArrayElementTitleSonatSdkAttribute))]
    public class ArrayElementTitleSonatSdkAttributeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property,
            GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        protected virtual ArrayElementTitleSonatSdkAttribute Atribute
        {
            get { return (ArrayElementTitleSonatSdkAttribute) attribute; }
        }

        SerializedProperty _titleNameProp;

        public override void OnGUI(Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            string fullPathName = property.propertyPath + "." + Atribute.varName;
            _titleNameProp = property.serializedObject.FindProperty(fullPathName);
            string newlabel = GetTitle();
            if (string.IsNullOrEmpty(newlabel))
                newlabel = label.text;
            EditorGUI.PropertyField(position, property, new GUIContent(newlabel, label.tooltip), true);
        }

        private string GetTitle()
        {
            switch (_titleNameProp.propertyType)
            {
                case SerializedPropertyType.Generic:
                    break;
                case SerializedPropertyType.Integer:
                    return _titleNameProp.intValue.ToString();
                case SerializedPropertyType.Boolean:
                    return _titleNameProp.boolValue.ToString();
                case SerializedPropertyType.Float:
                    return _titleNameProp.floatValue.ToString();
                case SerializedPropertyType.String:
                    return _titleNameProp.stringValue;
                case SerializedPropertyType.Color:
                    return _titleNameProp.colorValue.ToString();
                case SerializedPropertyType.ObjectReference:
                    return _titleNameProp.objectReferenceValue.ToString();
                case SerializedPropertyType.LayerMask:
                    break;
                case SerializedPropertyType.Enum:
                    if (_titleNameProp.enumNames.Length <= _titleNameProp.enumValueIndex || _titleNameProp.enumValueIndex < 0)
                        return "out of range";
                    return _titleNameProp.enumNames[_titleNameProp.enumValueIndex];
                case SerializedPropertyType.Vector2:
                    return _titleNameProp.vector2Value.ToString();
                case SerializedPropertyType.Vector3:
                    return _titleNameProp.vector3Value.ToString();
                case SerializedPropertyType.Vector4:
                    return _titleNameProp.vector4Value.ToString();
                case SerializedPropertyType.Rect:
                    break;
                case SerializedPropertyType.ArraySize:
                    break;
                case SerializedPropertyType.Character:
                    break;
                case SerializedPropertyType.AnimationCurve:
                    break;
                case SerializedPropertyType.Bounds:
                    break;
                case SerializedPropertyType.Gradient:
                    break;
                case SerializedPropertyType.Quaternion:
                    break;
                default:
                    break;
            }

            return "";
        }
    }
}