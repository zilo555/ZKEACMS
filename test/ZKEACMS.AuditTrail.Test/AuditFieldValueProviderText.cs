/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using Easy.AuditTrail;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ZKEACMS.AuditTrail.Service;

namespace ZKEACMS.AuditTrail.Test
{
    [TestClass]
    public class AuditFieldValueProviderText
    {
        class CustomFieldItem
        {
            public string Key { get; set; }
            public string Label { get; set; }
            public string Value { get; set; }
        }

        class CustomFieldArrayItem
        {
            public string Key { get; set; }
            public string Label { get; set; }
            public string[] Value { get; set; }
        }

        class EntityWithCustomFields
        {
            public int Id { get; set; }
            public List<CustomFieldItem> CustomFields { get; set; }
        }

        class EntityWithCustomFieldArrays
        {
            public int Id { get; set; }
            public List<CustomFieldArrayItem> CustomFields { get; set; }
        }

        class CustomFieldProvider : IAuditFieldValueProvider
        {
            public int Priority => 10;

            public bool CanHandle(PropertyInfo property, System.Type entityType)
            {
                return entityType == typeof(EntityWithCustomFields) && property.Name == nameof(EntityWithCustomFields.CustomFields);
            }

            public string GetDisplayValue(PropertyInfo property, object rawValue)
            {
                return rawValue?.ToString();
            }

            public IEnumerable<AuditField> GetFields(PropertyInfo property, object rawValue)
            {
                var list = rawValue as IEnumerable<CustomFieldItem>;
                if (list == null)
                {
                    return Enumerable.Empty<AuditField>();
                }

                return list.Select((item, index) => new AuditField
                {
                    FieldName = item.Key ?? index.ToString(),
                    DisplayName = item.Label ?? item.Key ?? index.ToString(),
                    Value = item.Value,
                    Order = index
                });
            }
        }

        class CustomFieldArrayProvider : IAuditFieldValueProvider
        {
            public int Priority => 10;

            public bool CanHandle(PropertyInfo property, System.Type entityType)
            {
                return entityType == typeof(EntityWithCustomFieldArrays) && property.Name == nameof(EntityWithCustomFieldArrays.CustomFields);
            }

            public string GetDisplayValue(PropertyInfo property, object rawValue)
            {
                return rawValue?.ToString();
            }

            public IEnumerable<AuditField> GetFields(PropertyInfo property, object rawValue)
            {
                var list = rawValue as IEnumerable<CustomFieldArrayItem>;
                if (list == null)
                {
                    return Enumerable.Empty<AuditField>();
                }

                return list.Select((item, index) => new AuditField
                {
                    FieldName = item.Key ?? index.ToString(),
                    DisplayName = item.Label ?? item.Key ?? index.ToString(),
                    Value = item.Value,
                    Order = index
                });
            }
        }

        [TestMethod]
        public void Compare_WithCustomFieldValueProvider_ShouldCompareCustomFieldFields()
        {
            var oldValue = new EntityWithCustomFields
            {
                Id = 1,
                CustomFields = new List<CustomFieldItem>
                {
                    new CustomFieldItem { Key = "color", Label = "Color", Value = "Red" },
                    new CustomFieldItem { Key = "size", Label = "Size", Value = "M" }
                }
            };

            var newValue = new EntityWithCustomFields
            {
                Id = 1,
                CustomFields = new List<CustomFieldItem>
                {
                    new CustomFieldItem { Key = "color", Label = "Color", Value = "Blue" },
                    new CustomFieldItem { Key = "material", Label = "Material", Value = "Cotton" }
                }
            };

            var valueProviders = new List<IAuditPropertyProvider> { new CustomFieldProvider() };

            var changes = EntityComparer.Compare(oldValue, newValue, valueProviders);

            Assert.IsTrue(changes.Any(c => c.Field == "CustomFields.Color"));
            Assert.IsTrue(changes.Any(c => c.Field == "CustomFields.Size"));
            Assert.IsTrue(changes.Any(c => c.Field == "CustomFields.Material"));

            var colorChange = changes.First(c => c.Field == "CustomFields.Color");
            Assert.AreEqual("Blue", colorChange.NewValue);
            Assert.AreEqual("Blue", colorChange.NewValue);

            var sizeChange = changes.First(c => c.Field == "CustomFields.Size");
            Assert.AreEqual("M", sizeChange.OldValue);
            Assert.IsNull(sizeChange.NewValue);

            var materialChange = changes.First(c => c.Field == "CustomFields.Material");
            Assert.IsNull(materialChange.OldValue);
            Assert.AreEqual("Cotton", materialChange.NewValue);
        }

        [TestMethod]
        public void Compare_WithCustomFieldArrayValues_ShouldCompareArrayDifferences()
        {
            var oldValue = new EntityWithCustomFieldArrays
            {
                Id = 1,
                CustomFields = new List<CustomFieldArrayItem>
                {
                    new CustomFieldArrayItem { Key = "tags", Label = "Tags", Value = new[] { "A", "B" } }
                }
            };

            var newValue = new EntityWithCustomFieldArrays
            {
                Id = 1,
                CustomFields = new List<CustomFieldArrayItem>
                {
                    new CustomFieldArrayItem { Key = "tags", Label = "Tags", Value = new[] { "B", "C" } }
                }
            };

            var valueProviders = new List<IAuditPropertyProvider> { new CustomFieldArrayProvider() };

            var changes = EntityComparer.Compare(oldValue, newValue, valueProviders);

            Assert.IsTrue(changes.Any(c => c.Field == "CustomFields.Tags"));
            Assert.IsTrue(changes.Any(c => c.Field == "CustomFields.Tags" && c.ChangeType == (int)AuditChangeType.Added));
            Assert.IsTrue(changes.Any(c => c.Field == "CustomFields.Tags" && c.ChangeType == (int)AuditChangeType.Deleted));

            var addedChange = changes.First(c => c.Field == "CustomFields.Tags" && c.ChangeType == (int)AuditChangeType.Added);
            var deletedChange = changes.First(c => c.Field == "CustomFields.Tags" && c.ChangeType == (int)AuditChangeType.Deleted);

            Assert.Contains("C", addedChange.NewValue);
            Assert.Contains("A", deletedChange.OldValue);
        }
    }
}