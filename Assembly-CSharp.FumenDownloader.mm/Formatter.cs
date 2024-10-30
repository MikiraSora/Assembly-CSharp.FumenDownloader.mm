using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

namespace DpPatches.FumenDownloader
{
    public static class Formatter
    {
        public static string FormatValue(object value, int tabIdx = 0)
        {
            var htab = string.Concat(Enumerable.Repeat(" ", tabIdx).ToArray());
            var ctab = string.Concat(Enumerable.Repeat(" ", tabIdx + 2).ToArray());

            if (value is null)
                return "<null>";
            var type = value.GetType();
            if (type.IsPrimitive)
                return $"{value}";
            if (type == typeof(string))
                return $"\"{value}\"";
            if (value is IEnumerable enumerable)
            {
                var sb2 = new StringBuilder();
                sb2.AppendLine($"{htab}[");
                foreach (var item in enumerable)
                {
                    var r = FormatValue(item, tabIdx + 2);
                    sb2.AppendLine($"{ctab}{r},");
                }
                sb2.AppendLine($"{htab}]");
                return sb2.ToString();
            }
            if (type.FullName.StartsWith("System."))
                return "<sys-hide>";

            //complex type


            var sb = new StringBuilder();
            sb.AppendLine($"{htab}{{");

            void write(string subName, string subValueStr, object subValue)
            {
                if ((subName == "_address" || subName == "_ipAddress") && subValue is uint ipAddr)
                {
                    var b = BitConverter.GetBytes(ipAddr);
                    Array.Reverse(b);
                    var ipaddr = new IPAddress(BitConverter.ToUInt32(b, 0));
                    subValueStr = $"IPAddress({ipaddr})";
                }

                sb.AppendLine($"{ctab}{subName} : {subValueStr}");
            }

            foreach (var fieldInfo in type.GetFields())
            {
                var subValue = fieldInfo.GetValue(value);
                write(fieldInfo.Name, FormatValue(subValue, tabIdx + 2), subValue);
            }

            foreach (var propertyInfo in type.GetProperties())
            {
                if (propertyInfo.GetGetMethod().IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false) &&
                    propertyInfo.GetSetMethod().IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false))
                {
                    var fieldInfo = type.GetField($"<{propertyInfo.Name}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (fieldInfo != null)
                    {
                        var subValue = fieldInfo.GetValue(value);
                        write(fieldInfo.Name, FormatValue(subValue, tabIdx + 2), subValue);
                    }
                }
            }

            sb.AppendLine($"{htab}}}");
            return sb.ToString();
        }
    }
}
