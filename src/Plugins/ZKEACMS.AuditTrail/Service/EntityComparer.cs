/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using ZKEACMS.Common.Models;
using ZKEACMS.Common.Models.Attributes;

namespace ZKEACMS.AuditTrail.Service
{
    /// <summary>
    /// 实体对比工具
    /// </summary>
    public class EntityComparer
    {
        /// <summary>
        /// 对比两个实体，返回变更的字段
        /// </summary>
        public static List<FieldChange> Compare<TEntity>(TEntity oldEntity, TEntity newEntity) where TEntity : class
        {
            var changes = new List<FieldChange>();
            
            if (oldEntity == null || newEntity == null)
            {
                return changes;
            }

            var entityType = typeof(TEntity);
            
            // 如果类标记了 IgnoreAudit，则不对比
            if (entityType.GetCustomAttribute<IgnoreAuditAttribute>() != null)
            {
                return changes;
            }

            var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && !p.GetCustomAttributes<IgnoreAuditAttribute>().Any());

            foreach (var property in properties)
            {
                try
                {
                    var oldValue = property.GetValue(oldEntity);
                    var newValue = property.GetValue(newEntity);

                    // 如果是集合类型（数组、List等），使用特殊的对比逻辑
                    if (IsCollectionType(property.PropertyType))
                    {
                        var collectionChanges = CompareCollection(property, oldValue, newValue);
                        changes.AddRange(collectionChanges);
                    }
                    else
                    {
                        // 普通属性对比
                        if (!AreEqual(oldValue, newValue))
                        {
                            changes.Add(new FieldChange
                            {
                                Field = property.Name,
                                OldValue = SerializeValue(oldValue),
                                NewValue = SerializeValue(newValue)
                            });
                        }
                    }
                }
                catch (Exception)
                {
                    // 忽略无法访问的属性
                }
            }

            return changes;
        }

        /// <summary>
        /// 对比集合类型
        /// </summary>
        private static List<FieldChange> CompareCollection(PropertyInfo property, object oldValue, object newValue)
        {
            var changes = new List<FieldChange>();

            var oldList = oldValue as IEnumerable;
            var newList = newValue as IEnumerable;

            // 如果一个为空一个不为空，记录整体变化
            if ((oldList == null && newList != null) || (oldList != null && newList == null))
            {
                changes.Add(new FieldChange
                {
                    Field = property.Name,
                    OldValue = SerializeValue(oldValue),
                    NewValue = SerializeValue(newValue)
                });
                return changes;
            }

            if (oldList == null && newList == null)
            {
                return changes;
            }

            // 转换为列表
            var oldItems = oldList.Cast<object>().ToList();
            var newItems = newList.Cast<object>().ToList();

            // 获取元素类型
            var elementType = GetCollectionElementType(property.PropertyType);
            if (elementType == null || elementType == typeof(object))
            {
                // 无法确定元素类型，使用简单对比
                if (oldItems.Count != newItems.Count || !oldItems.SequenceEqual(newItems))
                {
                    changes.Add(new FieldChange
                    {
                        Field = property.Name,
                        OldValue = SerializeValue(oldValue),
                        NewValue = SerializeValue(newValue)
                    });
                }
                return changes;
            }

            // 获取主键属性
            var keyProperty = GetKeyProperty(elementType);

            if (keyProperty == null)
            {
                // 没有主键，使用简单对比
                if (oldItems.Count != newItems.Count || !oldItems.SequenceEqual(newItems))
                {
                    changes.Add(new FieldChange
                    {
                        Field = property.Name,
                        OldValue = $"Count: {oldItems.Count}",
                        NewValue = $"Count: {newItems.Count}"
                    });
                }
            }
            else
            {
                // 按主键对比
                var oldDict = oldItems.ToDictionary(item => keyProperty.GetValue(item));
                var newDict = newItems.ToDictionary(item => keyProperty.GetValue(item));

                var allKeys = oldDict.Keys.Union(newDict.Keys).ToList();
                var itemChanges = new List<string>();

                foreach (var key in allKeys)
                {
                    if (!oldDict.ContainsKey(key))
                    {
                        itemChanges.Add($"Added: {key}");
                    }
                    else if (!newDict.ContainsKey(key))
                    {
                        itemChanges.Add($"Removed: {key}");
                    }
                    else if (!AreEqual(oldDict[key], newDict[key]))
                    {
                        itemChanges.Add($"Modified: {key}");
                    }
                }

                if (itemChanges.Any())
                {
                    changes.Add(new FieldChange
                    {
                        Field = property.Name,
                        OldValue = $"Count: {oldItems.Count}",
                        NewValue = $"Count: {newItems.Count}, Changes: {string.Join(", ", itemChanges)}"
                    });
                }
            }

            return changes;
        }

        /// <summary>
        /// 判断是否为集合类型
        /// </summary>
        private static bool IsCollectionType(Type type)
        {
            if (type == typeof(string))
            {
                return false;
            }

            return typeof(IEnumerable).IsAssignableFrom(type);
        }

        /// <summary>
        /// 获取集合的元素类型
        /// </summary>
        private static Type GetCollectionElementType(Type collectionType)
        {
            if (collectionType.IsArray)
            {
                return collectionType.GetElementType();
            }

            var enumerableType = collectionType.GetInterfaces()
                .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            return enumerableType?.GetGenericArguments()[0];
        }

        /// <summary>
        /// 获取标记了 [Key] 特性的属性
        /// </summary>
        private static PropertyInfo GetKeyProperty(Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null);
        }

        /// <summary>
        /// 判断两个值是否相等
        /// </summary>
        private static bool AreEqual(object value1, object value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // 特殊处理DateTime类型，只比较到秒
            if (value1 is DateTime dt1 && value2 is DateTime dt2)
            {
                return Math.Abs((dt1 - dt2).TotalSeconds) < 1;
            }

            return value1.Equals(value2);
        }

        /// <summary>
        /// 序列化值为字符串
        /// </summary>
        public static string SerializeValue(object value)
        {
            if (value == null) return null;

            if (value is string str) return str;
            if (value is DateTime dt) return dt.ToString("yyyy-MM-dd HH:mm:ss");
            
            // 对于复杂类型，使用 JSON 序列化
            if (value.GetType().IsClass && value.GetType() != typeof(string))
            {
                try
                {
                    return JsonSerializer.Serialize(value, new JsonSerializerOptions 
                    { 
                        WriteIndented = false,
                        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                    });
                }
                catch
                {
                    return value.ToString();
                }
            }

            return value.ToString();
        }
    }
}
