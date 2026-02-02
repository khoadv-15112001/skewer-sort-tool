#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Sonat.Attributes;
using UnityEditor;
using UnityEngine;

namespace Sonat.Editor.Drawer
{
    public static class IndexAsEnumDrawerHelper
    {
        public static void DrawPropertyAsEnum(EnumList enumList, SerializedProperty property, Rect position,
            GUIContent label)
        {
            var _index = -1;
            for (var i = 0; i < enumList.Values.Length; i++)
                if (property.intValue == enumList.Values[i])
                {
                    _index = i;
                    break;
                }

            IList<int> values = enumList.Values.ToList();
            IList<GUIContent> names = enumList.Names.ToList();
            if (_index == -1)
            {
                _index = values.Count;
                values = ArrayAdd(values, property.intValue);
                names = ArrayAdd(names, new GUIContent($"NotFound {enumList.TypeName}_{property.intValue}"));
            }

            property.intValue = EditorGUI.IntPopup(position, label, property.intValue, names.ToArray(),
                values.ToArray());
        }
    
        public static void DrawPropertyAsEnum(EnumList enumList, SerializedProperty property, Rect position)
        {
            var _index = -1;
            for (var i = 0; i < enumList.Values.Length; i++)
                if (property.intValue == enumList.Values[i])
                {
                    _index = i;
                    break;
                }

            IList<int> values = enumList.Values.ToList();
            IList<GUIContent> names = enumList.Names.ToList();
            if (_index == -1)
            {
                _index = values.Count;
                values = ArrayAdd(values, property.intValue);
                names = ArrayAdd(names, new GUIContent($"NotFound {enumList.TypeName}_{property.intValue}"));
            }

            property.intValue = EditorGUI.IntPopup(position,  property.intValue, names.ToArray(),
                values.ToArray());
        }

        public static void DrawPropertyAsFlagEnum(EnumList enumList, SerializedProperty property, Rect position,
            GUIContent label)
        {
            IList<int> values = enumList.Values.ToList();
            IList<string> names = enumList.Names.Select(x => x.text).ToList();

            property.intValue = DrawBitMaskField(position, property.intValue, names.ToArray(),
                values.ToArray(), label);
        }

        public static int DrawBitMaskField(Rect aPosition, int aMask, string[] itemNames, int[] itemValues,
            GUIContent aLabel)
        {
//            var itemNames = System.Enum.GetNames(aType);
//            var itemValues = System.Enum.GetValues(aType) as int[];

            int val = aMask;
            int maskVal = 0;
            for (int i = 0; i < itemValues.Length; i++)
            {
                if (itemValues[i] != 0)
                {
                    if ((val & itemValues[i]) == itemValues[i])
                        maskVal |= 1 << i;
                }
                else if (val == 0)
                    maskVal |= 1 << i;
            }

            int newMaskVal = EditorGUI.MaskField(aPosition, aLabel, maskVal, itemNames);
            int changes = maskVal ^ newMaskVal;

            for (int i = 0; i < itemValues.Length; i++)
            {
                if ((changes & (1 << i)) != 0) // has this list item changed?
                {
                    if ((newMaskVal & (1 << i)) != 0) // has it been set?
                    {
                        if (itemValues[i] == 0) // special case: if "0" is set, just set the val to 0
                        {
                            val = 0;
                            break;
                        }
                        else
                            val |= itemValues[i];
                    }
                    else // it has been reset
                    {
                        val &= ~itemValues[i];
                    }
                }
            }

            return val;
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
    }
}
#endif