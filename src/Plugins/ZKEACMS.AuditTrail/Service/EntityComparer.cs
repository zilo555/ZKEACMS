/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using Easy.AuditTrail;
using Easy.AuditTrail.Attributes;
using Easy.Serializer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using ZKEACMS.Common.Models;
using ZKEACMS.Common.Service;

namespace ZKEACMS.AuditTrail.Service
{
    /// <summary>
    /// Entity comparison utility
    /// </summary>
    public class EntityComparer
    {
        /// <summary>
        /// Compare two entities and return changed fields
        /// </summary>
        public static List<FieldChange> Compare<TEntity>(TEntity oldEntity,
            TEntity newEntity,
            IEnumerable<IAuditValueProvider> valueProviders = null)
            where TEntity : class
        {
            var changes = new List<FieldChange>();

            if (oldEntity == null || newEntity == null)
            {
                return changes;
            }

            var entityType = typeof(TEntity);

            // If class is marked with IgnoreAudit, skip comparison
            if (entityType.GetCustomAttribute<AuditIgnoreAttribute>() != null)
            {
                return changes;
            }

            CompareRecursive(oldEntity, newEntity, "", changes, valueProviders, null);
            return changes;
        }

        /// <summary>
        /// Recursively compare entities
        /// </summary>
        private static void CompareRecursive(object oldObj, object newObj,
            string prefix,
            List<FieldChange> changes,
            IEnumerable<IAuditValueProvider> valueProviders,
            PropertyInfo currentPropertyInfo)
        {
            if (oldObj == null && newObj == null)
            {
                return;
            }

            var type = oldObj == null ? newObj.GetType() : oldObj.GetType();
            if (type.IsValueType || type == typeof(string))
            {
                if (!AreEqual(oldObj, newObj))
                {
                    changes.Add(new FieldChange
                    {
                        Field = prefix.TrimEnd('.'),
                        OldValue = SerializeValue(oldObj, currentPropertyInfo, valueProviders),
                        NewValue = SerializeValue(newObj, currentPropertyInfo, valueProviders)
                    });
                }
                return;
            }
            if (oldObj == null)
            {
                oldObj = Activator.CreateInstance(type);
            }
            if (newObj == null)
            {
                newObj = Activator.CreateInstance(type);
            }
            if (IsCollectionType(type))
            {
                CompareCollection(oldObj, newObj, prefix.TrimEnd('.'), changes, valueProviders, currentPropertyInfo);
                return;
            }

            // Handle ordinary complex objects
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && !p.GetCustomAttributes<AuditIgnoreAttribute>().Any());

            foreach (var property in properties)
            {
                // Get the display name for the current property if available
                var displayName = GetDisplayPropertyName(property, valueProviders);
                var propPrefix = string.IsNullOrEmpty(prefix) ? displayName : $"{prefix}.{displayName}";

                var oldValue = property.GetValue(oldObj);
                var newValue = property.GetValue(newObj);

                // Pass the property info for the current property being compared
                CompareRecursive(oldValue, newValue, propPrefix, changes, valueProviders, property);
            }
        }

        /// <summary>
        /// Determine if type is a simple type (value type or string)
        /// </summary>
        private static bool IsValueType(Type type)
        {
            if (type == null) return true;
            return type.IsValueType || type == typeof(string);
        }

        /// <summary>
        /// Compare collection types
        /// </summary>
        private static void CompareCollection(object oldObj,
            object newObj,
            string fieldName,
            List<FieldChange> changes,
            IEnumerable<IAuditValueProvider> valueProviders,
            PropertyInfo currentPropertyInfo)
        {
            var oldList = oldObj as IEnumerable;
            var newList = newObj as IEnumerable;

            if (oldList == null && newList == null)
            {
                return;
            }

            if (oldList == null)
            {
                oldList = Activator.CreateInstance(newList.GetType()) as IEnumerable;
            }

            if (newList == null)
            {
                newList = Activator.CreateInstance(oldList.GetType()) as IEnumerable;
            }

            var type = oldObj == null ? newObj.GetType() : oldObj.GetType();

            // Handle Dictionary separately
            if (IsDictionaryType(type))
            {
                CompareDictionary(oldObj, newObj, fieldName, changes, valueProviders, currentPropertyInfo);
                return;
            }

            // Convert to list
            var oldItems = oldList.Cast<object>().ToList();
            var newItems = newList.Cast<object>().ToList();

            // Get element type
            var elementType = GetCollectionElementType(oldObj.GetType());

            // Check if elementType is a simple type (value type or string)
            if (IsValueType(elementType))
            {
                CompareSimpleElements(fieldName, changes, oldItems, newItems, valueProviders, currentPropertyInfo);
                return;
            }

            // Compare by key combination, using each item's runtime type to get key/title properties
            var oldDict = oldItems.ToDictionary(
                item =>
                {
                    var key = GetItemKey(item, valueProviders);
                    if (key.EndsWith(":"))
                    {
                        throw new InvalidOperationException($"Collection item type '{item.GetType().Name}' must have at least one property marked with [AuditKey] attribute for auditing.");
                    }
                    return key;
                });
            var newDict = newItems.ToDictionary(
                item =>
                {
                    var key = GetItemKey(item, valueProviders);
                    if (key.EndsWith(":"))
                    {
                        throw new InvalidOperationException($"Collection item type '{item.GetType().Name}' must have at least one property marked with [AuditKey] attribute for auditing.");
                    }
                    return key;
                });

            var allKeys = oldDict.Keys.Union(newDict.Keys).ToList();

            foreach (var key in allKeys)
            {
                if (!oldDict.ContainsKey(key))
                {
                    // Added item
                    var addedItem = newDict[key];
                    var keyAndTitle = GetItemKeyAndTitle(addedItem, valueProviders);
                    changes.Add(new FieldChange
                    {
                        Field = fieldName,
                        ChangeType = (int)AuditChangeType.Added,
                        NewValue = keyAndTitle
                    });
                }
                else if (!newDict.ContainsKey(key))
                {
                    // Removed item
                    var removedItem = oldDict[key];
                    var keyAndTitle = GetItemKeyAndTitle(removedItem, valueProviders);
                    changes.Add(new FieldChange
                    {
                        Field = fieldName,
                        ChangeType = (int)AuditChangeType.Deleted,
                        OldValue = keyAndTitle
                    });
                }
                else
                {
                    // Modified item, compare recursively
                    var oldItem = oldDict[key];
                    var newItem = newDict[key];

                    if (!AreEqual(oldItem, newItem))
                    {
                        var keyAndTitle = GetItemKeyAndTitle(oldItem, valueProviders);
                        CompareRecursive(oldItem, newItem, $"{fieldName}[{keyAndTitle}]", changes, valueProviders, null);
                    }
                }
            }
        }

        private static void CompareSimpleElements(string fieldName,
            List<FieldChange> changes,
            List<object> oldItems,
            List<object> newItems,
            IEnumerable<IAuditValueProvider> valueProviders,
            PropertyInfo currentPropertyInfo)
        {
            // Handle simple types: compare which values were added or removed
            var oldSet = new HashSet<object>(oldItems.Where(i => i != null));
            var newSet = new HashSet<object>(newItems.Where(i => i != null));

            // Find items that were added
            var addedItems = newSet.Except(oldSet).ToList();
            if (addedItems.Any())
            {
                changes.Add(new FieldChange
                {
                    Field = fieldName,
                    ChangeType = (int)AuditChangeType.Added,
                    NewValue = SerializeValue(addedItems, currentPropertyInfo, valueProviders)
                });
            }

            // Find items that were removed
            var deletedItems = oldSet.Except(newSet).ToList();
            if (deletedItems.Any())
            {
                changes.Add(new FieldChange
                {
                    Field = fieldName,
                    ChangeType = (int)AuditChangeType.Deleted,
                    OldValue = SerializeValue(deletedItems, currentPropertyInfo, valueProviders)
                });
            }
        }

        /// <summary>
        /// Compare dictionary types
        /// </summary>
        private static void CompareDictionary(object oldObj, object newObj, string fieldName, List<FieldChange> changes, IEnumerable<IAuditValueProvider> valueProviders, PropertyInfo currentPropertyInfo)
        {
            var oldDict = oldObj as IDictionary;
            var newDict = newObj as IDictionary;

            if (oldDict == null && newDict == null)
            {
                return;
            }

            if (oldDict == null)
            {
                var dictType = newObj.GetType();
                oldDict = (IDictionary)Activator.CreateInstance(dictType);
            }

            if (newDict == null)
            {
                var dictType = oldObj.GetType();
                newDict = (IDictionary)Activator.CreateInstance(dictType);
            }

            // Get all keys from both dictionaries
            var allKeys = new HashSet<object>();
            foreach (var key in oldDict.Keys)
            {
                allKeys.Add(key);
            }
            foreach (var key in newDict.Keys)
            {
                allKeys.Add(key);
            }

            // Compare values for each key
            foreach (var key in allKeys)
            {
                var oldValue = oldDict.Contains(key) ? oldDict[key] : null;
                var newValue = newDict.Contains(key) ? newDict[key] : null;
                if (oldValue == null && newValue == null)
                {
                    continue;
                }
                var valueType = oldValue?.GetType() ?? newValue?.GetType();

                // Use the key in the field name, applying display name logic if available
                var keyStr = key?.ToString() ?? "null";
                var fieldPath = $"{fieldName}[{keyStr}]";

                if (IsValueType(valueType))
                {// If value is a simple type (value type or string), handle directly
                    if (AreEqual(oldValue, newValue)) continue;

                    changes.Add(new FieldChange
                    {
                        Field = fieldPath,
                        OldValue = SerializeValue(oldValue, currentPropertyInfo, valueProviders),
                        NewValue = SerializeValue(newValue, currentPropertyInfo, valueProviders)
                    });
                }
                else
                {// For complex objects, compare recursively using the new path
                    CompareRecursive(oldValue, newValue, fieldPath, changes, valueProviders, currentPropertyInfo);
                }
            }
        }

        /// <summary>
        /// Get the display name for a property
        /// </summary>
        private static string GetDisplayPropertyName(PropertyInfo property, IEnumerable<IAuditValueProvider> valueProviders)
        {
            if (valueProviders == null)
            {
                return property.Name;
            }

            // Check if any provider implements IAuditDisplayProvider and can provide a display name
            var displayProviders = valueProviders.OfType<IAuditDisplayProvider>();

            foreach (var displayProvider in displayProviders)
            {
                if (!displayProvider.CanHandle(property, property.DeclaringType)) continue;

                var displayName = displayProvider.GetDisplayName(property, property.DeclaringType);
                if (string.IsNullOrEmpty(displayName)) continue;

                return displayName;
            }
            return property.Name;
        }

        public static string GetKeyAndTitle<TEntity>(TEntity item, IEnumerable<IAuditValueProvider> valueProviders = null) where TEntity : class
        {
            var type = typeof(TEntity);
            var keyProperties = GetKeyProperties(type);
            var titleProperties = GetTitleProperties(type);
            return GetKeyAndTitle(keyProperties, titleProperties, item, valueProviders);
        }

        /// <summary>
        /// Get combined key and title information for composite keys/titles
        /// </summary>
        private static string GetKeyAndTitle(PropertyInfo[] keyProperties, PropertyInfo[] titleProperties, object item, IEnumerable<IAuditValueProvider> valueProviders = null)
        {
            var keyValue = GetCombinedKeyValue(keyProperties, item, valueProviders);
            var titleValue = GetCombinedTitleValue(titleProperties, item, valueProviders);

            if (string.IsNullOrEmpty(titleValue))
            {
                return keyValue;
            }

            return $"({keyValue}):{titleValue}";
        }

        /// <summary>
        /// Get key and title for an item using its runtime type
        /// </summary>
        private static string GetItemKeyAndTitle(object item, IEnumerable<IAuditValueProvider> valueProviders = null)
        {
            var type = item.GetType();
            var keyProperties = GetKeyProperties(type);
            var titleProperties = GetTitleProperties(type);
            return GetKeyAndTitle(keyProperties, titleProperties, item, valueProviders);
        }

        /// <summary>
        /// Gets the combined value of all key properties sorted by Order
        /// </summary>
        private static string GetCombinedKeyValue(PropertyInfo[] keyProperties, object item, IEnumerable<IAuditValueProvider> valueProviders = null)
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
                    return SerializeValue(value, prop, valueProviders);
                }
                var childKeys = valueType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(child => child.GetCustomAttribute<AuditKeyAttribute>() != null)
                .OrderBy(child => child.GetCustomAttribute<AuditKeyAttribute>().Order)
                .ToArray();

                if (childKeys.Length == 0) throw new InvalidOperationException($"Key property '{prop.Name}' of type '{valueType.Name}' must have at least one property marked with [AuditKey] attribute for auditing.");

                return GetCombinedKeyValue(childKeys, value, valueProviders);
            }).Where(m => m != null)
            .ToArray();

            return string.Join("|", keyValues);
        }

        /// <summary>
        /// Get key for an item using its runtime type, includes type information to distinguish different derived types
        /// </summary>
        private static string GetItemKey(object item, IEnumerable<IAuditValueProvider> valueProviders = null)
        {
            var type = item.GetType();
            var keyProperties = GetKeyProperties(type);
            var typePrefix = type.FullName;
            return $"{typePrefix}:{GetCombinedKeyValue(keyProperties, item, valueProviders)}";
        }

        /// <summary>
        /// Gets the combined value of all title properties sorted by Order
        /// </summary>
        private static string GetCombinedTitleValue(PropertyInfo[] titleProperties, object item, IEnumerable<IAuditValueProvider> valueProviders = null)
        {
            if (titleProperties == null || !titleProperties.Any())
            {
                return "";
            }

            var titleValues = titleProperties.Select(prop =>
            {
                var value = prop.GetValue(item);
                if (value == null) return null;

                var valueType = value.GetType();
                if (IsValueType(valueType))
                {
                    return SerializeValue(value, prop, valueProviders);
                }
                var childKeys = valueType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(child => child.GetCustomAttribute<AuditTitleAttribute>() != null)
                .OrderBy(child => child.GetCustomAttribute<AuditTitleAttribute>().Order)
                .ToArray();

                if (childKeys.Length == 0) throw new InvalidOperationException($"Title property '{prop.Name}' of type '{valueType.Name}' must have at least one property marked with [AuditTitle] attribute for auditing.");

                return GetCombinedTitleValue(childKeys, value, valueProviders);
            }).Where(v => v != null).ToArray();
            return string.Join(", ", titleValues); // Using comma space to join title parts
        }

        /// <summary>
        /// Get all title properties ordered by their Order property
        /// </summary>
        private static PropertyInfo[] GetTitleProperties(Type type)
        {
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Get all properties marked with [AuditTitle]
            var auditTitleProperties = properties
                .Where(p => p.GetCustomAttribute<AuditTitleAttribute>() != null)
                .OrderBy(p => p.GetCustomAttribute<AuditTitleAttribute>().Order)
                .ToArray();

            return auditTitleProperties;
        }

        /// <summary>
        /// Determine if type is a collection type
        /// </summary>
        private static bool IsCollectionType(Type type)
        {
            if (type == typeof(string))
            {
                return false;
            }

            return typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string);
        }

        /// <summary>
        /// Get element type of collection
        /// </summary>
        private static Type GetCollectionElementType(Type collectionType)
        {
            if (collectionType.IsArray)
            {
                return collectionType.GetElementType();
            }

            var enumerableType = collectionType.GetInterfaces()
                .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (enumerableType != null)
            {
                return enumerableType.GetGenericArguments()[0];
            }

            // If above didn't work, it could be direct inheritance like List<T>
            var genericArgs = collectionType.GetGenericArguments();
            if (genericArgs.Length > 0)
            {
                return genericArgs[0];
            }

            return null;
        }

        /// <summary>
        /// Get all key properties ordered by their Order property
        /// </summary>
        private static PropertyInfo[] GetKeyProperties(Type type)
        {
            return AuditKeyAttribute.GetKeyProperties(type);
        }

        /// <summary>
        /// Determine if two values are equal
        /// </summary>
        private static bool AreEqual(object value1, object value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Special handling for DateTime type, only compare to seconds
            if (value1 is DateTime dt1 && value2 is DateTime dt2)
            {
                return Math.Abs((dt1 - dt2).TotalSeconds) < 1;
            }

            return value1.Equals(value2);
        }

        /// <summary>
        /// Serialize value to string with property and value providers
        /// </summary>
        private static string SerializeValue(object value, PropertyInfo propertyInfo = null, IEnumerable<IAuditValueProvider> valueProviders = null)
        {
            if (value == null) return null;

            if (propertyInfo != null && valueProviders != null)
            {
                foreach (var provider in valueProviders.Where(m => m is not IAuditDisplayProvider))
                {
                    if (!provider.CanHandle(propertyInfo, propertyInfo.DeclaringType)) continue;

                    var result = provider.GetDisplayValue(propertyInfo, value);
                    if (result == null) continue;

                    return result;
                }

            }

            if (value is string str) return str;
            if (value is DateTime dt) return dt.ToString("yyyy-MM-dd HH:mm:ss");

            if (IsValueType(value.GetType()))
            {
                return value.ToString();
            }
            return JsonConverter.Serialize(value);
        }

        /// <summary>
        /// Determine if type is a dictionary type
        /// </summary>
        private static bool IsDictionaryType(Type type)
        {
            return type.IsGenericType &&
                   (type.GetGenericTypeDefinition() == typeof(Dictionary<,>) ||
                    type.GetInterface(nameof(IDictionary)) != null);
        }
    }
}