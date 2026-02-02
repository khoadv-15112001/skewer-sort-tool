using System;
using System.Collections.Generic;
using System.Reflection;
using Sonat.Attributes;
using UnityEditor;
using UnityEngine;

namespace Sonat.Editor.Drawer.Button
{
    [CustomPropertyDrawer(typeof(MyButtonAttribute))]
    public class MyButtonAttributeDrawer : ClassicButtonDrawer
    {
        protected override bool UsePropertyAsParameter => false;

        protected override List<Type[]> FindParams => new List<Type[]>
        {
            new Type[0], new[] {typeof(int)}
        };

        public override object GetParams(SerializedProperty property,int i)
        {
            return null;
        }
    }

    [CustomPropertyDrawer(typeof(MyButtonStringAttribute))]
    public class ButtonStringAttributeDrawer : ClassicButtonDrawer
    {
        protected override List<Type[]> FindParams => new List<Type[]>
        {
            new Type[0], new[] {typeof(string)}
        }; 
    
        public override object GetParams(SerializedProperty property,int i)
        {
            return property.stringValue;
        }

        public override void DrawProperty(SerializedProperty property, Rect rect,GUIContent label)
        {
            base.DrawProperty(property, rect,label);
            property.stringValue = EditorGUI.TextField(rect, property.stringValue);
        }
    }

    [CustomPropertyDrawer(typeof(MyButtonToggleAttribute))]
    public class ButtonToggleAttributeDrawer : ClassicButtonDrawer
    {
        protected override List<Type[]> FindParams => new List<Type[]>
        {
            new Type[0], new[] {typeof(bool)}
        };
    
        public override object GetParams(SerializedProperty property,int i)
        {
            return property.boolValue;
        }

        public override void DrawProperty(SerializedProperty property, Rect rect,GUIContent label)
        {
            base.DrawProperty(property, rect, label);
            property.boolValue = EditorGUI.Toggle(rect, property.boolValue);
        }
    }

    [CustomPropertyDrawer(typeof(MyButtonFloatAttribute))]
    public class MyButtonFloatAttributeDrawer : ClassicButtonDrawer
    {
        protected override List<Type[]> FindParams => new List<Type[]>
        {
            new Type[0], new[] {typeof(float)}
        };
    
        public override object GetParams(SerializedProperty property,int i)
        {
            return property.floatValue;
        }

        public override void DrawProperty(SerializedProperty property, Rect rect,GUIContent label)
        {
            base.DrawProperty(property, rect, label);
            property.floatValue = EditorGUI.FloatField(rect, property.floatValue);
        }
    }


    [CustomPropertyDrawer(typeof(MyButtonFloatSliderAttribute))]
    public class MyButtonFloatSliderAttributeDrawer : ClassicButtonDrawer
    {
        protected override List<Type[]> FindParams => new List<Type[]>
        {
            new Type[0], new[] {typeof(float)}
        };
    
        public override object GetParams(SerializedProperty property,int i)
        {
            return property.floatValue;
        }

        public override void DrawProperty(SerializedProperty property, Rect rect,GUIContent label)
        {
            base.DrawProperty(property, rect, label);
            property.floatValue = EditorGUI.Slider(rect, property.floatValue,0,1);
        }
    }


    [CustomPropertyDrawer(typeof(MyButtonVector3Attribute))]
    public class ButtonVector3AttributeDrawer : ClassicButtonDrawer
    {
        protected override List<Type[]> FindParams => new List<Type[]>
        {
            new Type[0], new[] {typeof(Vector3)}
        };
    
        public override object GetParams(SerializedProperty property,int i)
        {
            return property.vector3Value;
        }

        public override void DrawProperty(SerializedProperty property, Rect rect,GUIContent label)
        {
            base.DrawProperty(property, rect, label);
            property.vector3Value = EditorGUI.Vector3Field(rect,new GUIContent(),  property.vector3Value);
        }
    }

    [CustomPropertyDrawer(typeof(MyButtonVector2IntAttribute))]
    public class ButtonVector2IntAttributeDrawer : ClassicButtonDrawer
    {
        protected override List<Type[]> FindParams => new List<Type[]>
        {
            new Type[0], new[] {typeof(Vector2Int)}
        };
    
        public override object GetParams(SerializedProperty property,int i)
        {
            return property.vector2IntValue;
        }

        public override void DrawProperty(SerializedProperty property, Rect rect,GUIContent label)
        {
            base.DrawProperty(property, rect, label);
            property.vector2IntValue = EditorGUI.Vector2IntField(rect, "", property.vector2IntValue);
        }
    }

    [CustomPropertyDrawer(typeof(MyButtonObjectAttribute))]
    public class ButtonObjectAttributeDrawer  : ClassicButtonDrawer
    {
        protected override List<Type[]> FindParams => new List<Type[]>
        {
            new Type[0]
        };

        protected override MethodInfo FindMethod(object target, string methodName, List<Type[]> parameterTypes, out int indexOfParam)
        {
            indexOfParam = 0;
            try
            {
                Type type = target.GetType();
                return type.GetMethod(methodName);
            }
            catch (Exception e)
            {
                Debug.LogError(methodName + " " + e);
            }
            return null;
        }

        public override object GetParams(SerializedProperty property,int i)
        {
            return property.objectReferenceValue;
        }

        public override void DrawProperty(SerializedProperty property, Rect rect,GUIContent label)
        {
            base.DrawProperty(property, rect, label);
            EditorGUIUtility.labelWidth = 1;
            EditorGUI.PropertyField(rect, property, new GUIContent("abc"), true);
            EditorGUIUtility.labelWidth = 0;
        }
    }
}