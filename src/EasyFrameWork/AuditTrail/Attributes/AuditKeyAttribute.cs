/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using System;
using System.Linq;
using System.Reflection;

namespace Easy.AuditTrail.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class AuditKeyAttribute : Attribute
    {
        public int Order { get; set; } = 0;

        public static PropertyInfo[] GetKeyProperties(Type type)
        {
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Get all properties marked with [AuditKey]
            var auditKeyProperties = properties
                .Where(p => p.GetCustomAttribute<AuditKeyAttribute>() != null)
                .OrderBy(p => p.GetCustomAttribute<AuditKeyAttribute>().Order)
                .ToArray();

            return auditKeyProperties;
        }

        public static string GetCombinedKeyValue(PropertyInfo[] keyProperties, object item)
        {
            if (keyProperties == null || !keyProperties.Any())
            {
                return "";
            }

            var keyValues = keyProperties.Select(prop =>
            {
                var value = prop.GetValue(item);
                if (value == null) return null;

                var valueType = value.GetType();
                if (IsValueType(valueType))
                {
                    return value.ToString();
                }
                var childKeys = valueType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(child => child.GetCustomAttribute<AuditKeyAttribute>() != null)
                .OrderBy(child => child.GetCustomAttribute<AuditKeyAttribute>().Order)
                .ToArray();

                if (childKeys.Length == 0) throw new InvalidOperationException($"Key property '{prop.Name}' of type '{valueType.Name}' must have at least one property marked with [AuditKey] attribute for auditing.");

                return GetCombinedKeyValue(childKeys, value);
            }).Where(m => m != null)
            .ToArray();

            return string.Join("|", keyValues); // Using pipe as separator for composite keys
        }

        public static string GetCombinedKeyValue(object item, params string[] ignoreProperties)
        {
            var keyProperties = GetKeyProperties(item.GetType())
                .Where(m=> !ignoreProperties.Contains(m.Name))
                .ToArray();

            return GetCombinedKeyValue(keyProperties, item);
        }

        static bool IsValueType(Type type)
        {
            if (type == null) return true;
            return type.IsValueType || type == typeof(string);
        }
    }
}