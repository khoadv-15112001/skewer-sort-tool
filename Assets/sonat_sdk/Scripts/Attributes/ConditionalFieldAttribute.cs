using System;
using UnityEngine;

namespace Sonat.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ConditionalFieldAttribute : PropertyAttribute
    {
        public readonly string PropertyToCheck;
        public readonly object[] CompareValues;
        public readonly bool Enable;
        public readonly bool Reverse;

        public ConditionalFieldAttribute(string propertyToCheck, object[] compareValue = null, bool reverse = false, bool enable = true)
        {
            PropertyToCheck = propertyToCheck;
            CompareValues = compareValue;
            Enable = enable;
            Reverse = reverse;
        }

        public ConditionalFieldAttribute(string propertyToCheck, object compareValue, bool reverse = false, bool enable = true)
        {
            PropertyToCheck = propertyToCheck;
            CompareValues = new object[] { compareValue };
            Enable = enable;
            Reverse = reverse;
        }
    }


    [AttributeUsage(AttributeTargets.Field)]
    public class ConditionLabelAttribute : PropertyAttribute
    {
        public readonly string PropertyToCheck;
        public readonly object[] Conditions;
        public readonly string[] Labels;

        public ConditionLabelAttribute(string propertyToCheck, object[] conditions, string[] labels)
        {
            this.PropertyToCheck = propertyToCheck;
            this.Conditions = conditions;
            this.Labels = labels;
        }
    }
}