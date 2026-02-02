using System;

namespace Helper
{
    public static class EnumHelper
    {
        public static bool TryGetEnumByName<T>(string input, out T value, bool ignoreCase = true) where T : struct, Enum
        {
            value = default;
            if (string.IsNullOrEmpty(input)) return false;

            var cmp = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            foreach (var name in Enum.GetNames(typeof(T)))
            {
                if (string.Equals(name, input, cmp))
                {
                    return Enum.TryParse(name, out value);
                }
            }
            return false;
        }
    }
}