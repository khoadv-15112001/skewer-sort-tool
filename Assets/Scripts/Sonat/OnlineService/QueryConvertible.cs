using System;
using System.Collections.Generic;
using System.Reflection;

namespace GrillSort.OnlineService
{
    public abstract class QueryConvertible
    {
        public virtual string ToQueryString()
        {
            var properties = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            var queryParams = new List<string>();

            foreach (var prop in properties)
            {
                var value = prop.GetValue(this);
                if (value == null) continue;

                string stringValue;
                if (prop.FieldType == typeof(DateTime) || prop.FieldType == typeof(DateTime?))
                {
                    stringValue = ((DateTime)value).ToString("O");
                }
                else if (prop.FieldType.IsEnum)
                {
                    stringValue = value.ToString().ToLower();
                }
                else
                {
                    stringValue = value.ToString();
                }
                
                queryParams.Add(
                    $"{Uri.EscapeDataString(prop.Name)}={Uri.EscapeDataString(stringValue)}"
                );
            }

            return queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;
        }
    }
}