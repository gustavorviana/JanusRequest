using System;
using System.IO;
using System.Linq;

namespace JanusRequest
{
    internal static class ReflectionUtils
    {
        /// <summary>
        /// An array of native types used for fast type checking and conversion.
        /// </summary>
        private static readonly Type[] _nativeTypes = new Type[]
        {
            typeof(bool),
            typeof(DBNull),
            typeof(string),
            typeof(DateTime),
            typeof(Guid),
            typeof(TimeSpan),
            typeof(DateTimeOffset)
#if NET6_0_OR_GREATER
            ,typeof(DateOnly)
            ,typeof(TimeOnly)
#endif
        };

        public static bool IsNative(Type type, bool ignoreBuffer)
        {
            if (type is null)
                return true;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return IsNative(Nullable.GetUnderlyingType(type), ignoreBuffer);

            return type.IsPrimitive || type.IsEnum || _nativeTypes.Contains(type) || IsNumeric(type) || !ignoreBuffer && IsBuffer(type);
        }

        public static bool IsNumeric(Type type)
        {
            return IsNumberWithoutDecimal(type) || IsNumberWithDecimal(type);
        }

        public static bool IsNumericString(string value)
        {
            if (string.IsNullOrEmpty(value) || CheckDot(value[0]) || CheckDot(value[value.Length - 1]))
                return false;

            bool hasDot = false;

            for (int i = 0; i < value.Length; i++)
            {
                var c = value[i];
                bool digit = char.IsDigit(c);
                bool isDot = c == '.' || c == ',';
                if (!digit && !isDot)
                    return false;

                if (digit)
                    continue;

                if (hasDot)
                    return false;

                if (isDot)
                    hasDot = true;
            }

            return true;
        }

        private static bool CheckDot(char c)
        {
            return c == '.' || c == ',';
        }

        public static bool IsNumberWithDecimal(Type type)
        {
            return type == typeof(decimal) || type == typeof(float) || type == typeof(double);
        }

        public static bool IsNumberWithoutDecimal(Type type)
        {
            return type == typeof(int) || type == typeof(long) || type == typeof(byte) || type == typeof(sbyte)
            || type == typeof(short) || type == typeof(ushort) || type == typeof(uint) || type == typeof(long)
            || type == typeof(ulong);
        }

        public static bool IsBuffer(Type type)
        {
            return type == typeof(byte[]) || typeof(Stream).IsAssignableFrom(type);
        }
    }
}
