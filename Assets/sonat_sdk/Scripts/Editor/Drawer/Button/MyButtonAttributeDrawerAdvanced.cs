using System;
using System.Collections;
using System.Collections.Generic;
using Sonat.Attributes;
using UnityEditor;
using UnityEngine;

namespace Sonat.Editor.Drawer.Button
{
    [CustomPropertyDrawer(typeof(MyButtonToggleEnumSelfAttribute))]
    public class MyButtonToggleEnumSelfAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var att = (MyButtonToggleEnumSelfAttribute) attribute;
            var rects = position.GetSplitRect(att.Names.Length);
            if (property.propertyType == SerializedPropertyType.Integer)
            {
                for (var i = 0; i < att.Names.Length; i++)
                {
                    GUI.enabled = att.Parameter[i] != property.intValue;
                    if (GUI.Button(rects[i], att.Names[i]))
                        property.intValue = att.Parameter[i];
                }
            }
            else
            {
                var propertyValue = property.serializedObject.FindProperty(property.propertyPath + ".value");
                for (var i = 0; i < att.Names.Length; i++)
                {
                    GUI.enabled = att.Parameter[i] != propertyValue.intValue;
                    if (GUI.Button(rects[i], att.Names[i]))
                        propertyValue.intValue = att.Parameter[i];
                }
            }
       
            GUI.enabled = true;
        }
    }


    [CustomPropertyDrawer(typeof(MyButtonToggleSelfAttribute))]
    public class MyButtonToggleSelfAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var att = (MyButtonToggleSelfAttribute) attribute;
            if (!Check(property))
                Check(property.serializedObject.FindProperty(property.propertyPath + ".value"));

            bool Check(SerializedProperty p)
            {
                if (p.propertyType == SerializedPropertyType.Boolean)
                {
                    IfBoolean(p);
                    return true;
                }
                if (p.propertyType == SerializedPropertyType.Integer)
                {
                    IfInteger(p);
                    return true;
                }
                return false;
            }

            void IfBoolean(SerializedProperty p)
            {
                GUI.color = !p.boolValue ? Color.red : Color.green;
                if (GUI.Button(position, p.boolValue ? att.WhenOn : att.WhenOff))
                    p.boolValue = !p.boolValue;
            }
        
            void IfInteger(SerializedProperty p)
            {
                GUI.color = p.intValue == 0 ? Color.red : Color.green;
                if (GUI.Button(position, p.boolValue ? att.WhenOn : att.WhenOff))
                    p.intValue = p.intValue == 0 ? 1 : 0;
            }
        
            GUI.color = Color.white; 
        }
    }

    [CustomPropertyDrawer(typeof(MyButtonIntParameterPresetAttribute))]
    public class MyButtonIntParameterPresetDrawer : ClassicButtonDrawer
    {
        protected override bool UsePropertyAsParameter => false;
        protected override bool Skip => true;

        protected override List<Type[]> FindParams => new List<Type[]>
        {
            new Type[0],
            new []{typeof(int)},
        };

        protected override int NButtons => ((MyButtonIntParameterPresetAttribute) attribute).Parameter.Length;

        public override object GetParams(SerializedProperty property, int i)
        {
            var att = (MyButtonIntParameterPresetAttribute) attribute;
            return att.Parameter[i];
        }

        protected override string GetMethodName(int i) => ButtonAttribute.MethodNames[0];
        protected override string GetButtonName(int i)
        {
            var att = ((MyButtonIntParameterPresetAttribute) attribute);
            if (att.Names == null)
                return att.Prefix + " " + att.Parameter[i];
            return att.Names[i];
        }
    }

    [CustomPropertyDrawer(typeof(MyButtonRangeAttribute))]
    public class ButtonRangeAttributeDrawer : ClassicButtonDrawer
    {
        protected override bool UsePropertyAsParameter => false;
        protected override bool Skip => true;

        protected override int[] Colors => ((MyButtonRangeAttribute) attribute).Colors;

        protected override List<Type[]> FindParams => new List<Type[]>
        {
            new Type[0],
            new []{typeof(int)},
        };

        protected override int NButtons
        {
            get
            {   
                var att = (MyButtonRangeAttribute) attribute;
                return att.To - att.From + 1;// + (att.Label != null ? 1 : 0);
            }
        }

        public override object GetParams(SerializedProperty property, int i)
        {
            var att = (MyButtonRangeAttribute) attribute;
            return att.From + i;
        }

        protected override string GetMethodName(int i) => ButtonAttribute.MethodNames[0];
        protected override string GetButtonName(int i)
        {
            var att = ((MyButtonRangeAttribute) attribute);
            if(att.Names != null)
                return att.Names[i];
            return att.Prefix + " " + (att.From + i);
        }
    }

    [CustomPropertyDrawer(typeof(MyButtonIntPageSelectAttribute))]
    public class MyButtonIntPageSelectAttributeDrawer : ClassicButtonDrawer
    {
        protected override bool UsePropertyAsParameter => true;

        protected override List<Type[]> FindParams => new List<Type[]>
        {
            new Type[0],
            new []{typeof(int)},
        };

        public override object GetParams(SerializedProperty property, int index)
        {
            return property.intValue;
        }
    
        public override void DrawProperty(SerializedProperty property, Rect rect,GUIContent label)
        {
            base.DrawProperty(property, rect,label);
            var rects = rect.GetSplitRect(3);

        
            GUI.enabled = property.intValue > 0;

            if (GUI.Button(rects[0], "<<"))
            {
                property.intValue--;
            }

            var att = (MyButtonIntPageSelectAttribute)attribute;
            var parent = (object) property.serializedObject.targetObject;
            if (parent == null)
                parent = property.GetParentSerializedValueRaw();

            // count 
            var field = parent.GetType().GetField(att.NameOfListOrCountProperty);
            if (field != null)
            {
                var property2 = field.GetValue(parent);
                if (property2 is ICollection)
                {
                    GUI.enabled = property.intValue < ((ICollection)property2).Count;
                }
            }
            else
            {
                var field2 = parent.GetType().GetProperty(att.NameOfListOrCountProperty);
                GUI.enabled = property.intValue < (int)field2.GetValue(parent);
            }
    

            if (GUI.Button(rects[2], ">>"))
            {
                property.intValue++;
            }
            GUI.enabled = true;

            property.intValue = EditorGUI.IntField(rects[1], property.intValue);

        }
    }
}