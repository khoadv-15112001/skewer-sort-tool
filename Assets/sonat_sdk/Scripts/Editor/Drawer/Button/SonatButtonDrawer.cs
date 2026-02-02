using System;
using System.Collections.Generic;
using System.Reflection;
using Sonat.Attributes;
using UnityEditor;
using UnityEngine;

namespace Sonat.Editor.Drawer.Button
{
    public abstract class SonatButtonDrawer : PropertyDrawer
    {
        protected virtual BaseButtonAttribute ButtonAttribute => (BaseButtonAttribute) attribute;

        protected virtual MethodInfo FindMethod(object target, string methodName, List<Type[]> parameterTypes,out int indexOfParam)
        {
            indexOfParam = -1;
            try
            {
                Type type = target.GetType();
                for (var index = 0; index < parameterTypes.Count; index++)
                {
                    Type[] parameterType = parameterTypes[index];
                    var method = type.GetMethod(methodName, parameterType);
                
                    if (method == null)
                        method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public ,
                            Type.DefaultBinder, parameterType, null);
                    if (method != null)
                    {
                        indexOfParam = index;
                        return method;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("check if it is pulbic");
                Debug.LogError( methodName + " " + e);
            }
            return null;
        }
    }

    public abstract class ClassicButtonDrawer : SonatButtonDrawer
    {
        protected virtual bool UsePropertyAsParameter => true;
        protected virtual bool Skip => false;
        protected abstract List<Type[]> FindParams { get; }
//    protected override List<Type[]> FindParams => new List<Type[]>
//    {
//        new Type[0], new[] {typeof(int)}
//    };

        protected int indexOfParams = -1;
        public virtual void DrawProperty(SerializedProperty property,Rect rect,GUIContent label)
        {
        
        }

        public virtual MethodInfo FindMethodAgain(string methodName, SerializedProperty property)
        {
            var target = property.serializedObject.targetObject;
            Type type = target.GetType();
            MethodInfo method = type.GetMethod(methodName);
            return method;
        }

        protected virtual string GetMethodName(int i) => ButtonAttribute.MethodNames[i];
        protected virtual string GetButtonName(int i) => ButtonAttribute.ButtonNames[i];
        protected virtual int NButtons => ButtonAttribute.MethodNames.Length;

        private Color[] _colorsValue;
        protected virtual Color[] ColorsValue
        {
            get
            {
                if(_colorsValue == null)
                    _colorsValue = new Color[]
                    {
                        new Color32(255, 166, 226, 255),
                        new Color32(78, 197, 255, 255),
                        new Color32(74, 131, 222, 255),
                        new Color32(236, 122, 255, 255),
                        new Color32(255, 209, 41, 255),
                        new Color32(27, 255, 41, 255),
                        new Color32(255, 134, 41, 255),
                        new Color32(232, 97, 144, 255),
                        new Color32(255, 75, 78, 255),
                        new Color32(46, 218, 169, 255),
                        new Color32(167, 87, 252, 255),
                        new Color32(167, 228, 60, 255),
                        new Color32(255, 216, 51, 255),
                    };

                return _colorsValue;
            }
        }
    
        protected virtual int[] Colors  => null;
    
        public abstract object GetParams(SerializedProperty property,int index);
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var color = GUI.color;
            //   var att = ((MyButtonAttribute) attribute);
            var rects =  position.GetSplitRect(NButtons + (UsePropertyAsParameter ? 1 : 0));
            if (UsePropertyAsParameter)
                DrawProperty(property,rects[rects.Length-1],label);
            for (var i = 0; i < NButtons; i++)
            {
                if(Colors != null && Colors.Length > 0)
                    GUI.color = color;
            
                var methodName = GetMethodName(i);
                var target = (object) property.serializedObject.targetObject;
                var method = FindMethod(target,methodName,FindParams,out indexOfParams);
                if (method == null)
                {
                    target = property.GetParentSerializedValueRaw();
                    if (target == null)
                    {
                        GUI.Label(rects[i], methodName+" not found target (check private in base class)");
                        return;
                    }
                    method = FindMethod(target,methodName,FindParams,out indexOfParams);

                    if (method == null)
                        method = FindMethodAgain(methodName,property);
                    if (method == null)
                    {
                        GUI.Label(rects[i], "Method could not be found. Is it public?");
                        return;
                    }
                }

                if (Colors != null && Colors.Length > 0)
                    GUI.color = ColorsValue[Mathf.Clamp(Colors[Mathf.Clamp(i,0, Colors.Length-1)],0,ColorsValue.Length-1)];
                // if no parameters, find
                if (method.GetParameters().Length == 0)
                {
                    if (GUI.Button(rects[i], ButtonAttribute.ButtonNames[i]))
                    {
                        method.Invoke(target, new object[] { });
                    }
                }
                else
                {
                    if (!UsePropertyAsParameter && !Skip)
                    {
                        // if not use parameter but method have parameter say error
                        if (method.GetParameters().Length > 0)
                        {
                            GUI.Label(rects[i], "Method cannot have parameters.");
                            return;
                        }
                    }
                    else
                    {
                        // if more than 1 paramter
//                    if (method.GetParameters().Length != 1)
//                    {
//                        GUI.Label(rects[i], "take only 1 parameter");
//                        return;
//                    }
                        if (GUI.Button(rects[i], GetButtonName(i)))
                        {
                            InvokeMethod(method, property, indexOfParams,target, new[] {GetParams(property,i)});
                        }
                    } 
                }
            }
        
            GUI.color = color;
        }

        protected virtual void InvokeMethod(MethodInfo methodInfo,SerializedProperty property,int indexOfParam, object target, object[] parameters)
        {
            methodInfo.Invoke(target, parameters);
        }
    }
}