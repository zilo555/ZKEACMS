using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using ZKEACMS.AuditTrail.Service;
using ZKEACMS.Common.Models.Attributes;
using ZKEACMS.Common.Service;

namespace ZKEACMS.AuditTrail.Test
{
    public class NestedEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }
    }

    public class MainEntity
    {
        public int Id { get; set; }
        public NestedEntity Data { get; set; }
        public string Name { get; set; }
    }

    // New entities for collection testing
    public class CollectionItem
    {
        [Key]
        public int ItemId { get; set; }
        [AuditTitle]
        public string ItemName { get; set; }
        public NestedEntity ItemDetails { get; set; }
    }

    public class EntityWithCollections
    {
        [Key]
        public int Id { get; set; }
        [AuditTitle]
        public string Name { get; set; }
        public List<CollectionItem> Items { get; set; }
        public Dictionary<string, string> Properties { get; set; }
    }

    public class NestedEntityDisplayProvider : IAuditDisplayProvider
    {
        public string GetDisplayValue(PropertyInfo property, object rawValue)
        {
            // Just return raw value for this test
            return rawValue?.ToString();
        }

        public string GetDisplayName(PropertyInfo property, System.Type entityType)
        {
            if (entityType == typeof(NestedEntity) && property.Name == nameof(NestedEntity.Title))
            {
                return "标题"; // Chinese for "Title"
            }
            if (entityType == typeof(MainEntity) && property.Name == nameof(MainEntity.Data))
            {
                return "数据"; // Chinese for "Data"
            }
            if (entityType == typeof(MainEntity) && property.Name == nameof(MainEntity.Name))
            {
                return "姓名"; // Chinese for "Name"
            }
            if (entityType == typeof(CollectionItem) && property.Name == nameof(CollectionItem.ItemName))
            {
                return "项目名称"; // Chinese for "Item Name"
            }
            if (entityType == typeof(CollectionItem) && property.Name == nameof(CollectionItem.ItemDetails))
            {
                return "项目详情"; // Chinese for "Item Details"
            }
            if (entityType == typeof(EntityWithCollections) && property.Name == nameof(EntityWithCollections.Items))
            {
                return "项目列表"; // Chinese for "Items List"
            }
            if (entityType == typeof(EntityWithCollections) && property.Name == nameof(EntityWithCollections.Properties))
            {
                return "属性字典"; // Chinese for "Properties Dictionary"
            }
            return property.Name; // Return original name for others
        }

        public bool CanHandle(PropertyInfo property, System.Type entityType, AuditOperationType operationType = AuditOperationType.GetValue)
        {
            return (entityType == typeof(NestedEntity) && property.Name == nameof(NestedEntity.Title)) ||
                   (entityType == typeof(MainEntity) && property.Name == nameof(MainEntity.Data)) ||
                   (entityType == typeof(MainEntity) && property.Name == nameof(MainEntity.Name)) ||
                   (entityType == typeof(CollectionItem) && property.Name == nameof(CollectionItem.ItemName)) ||
                   (entityType == typeof(CollectionItem) && property.Name == nameof(CollectionItem.ItemDetails)) ||
                   (entityType == typeof(EntityWithCollections) && property.Name == nameof(EntityWithCollections.Items)) ||
                   (entityType == typeof(EntityWithCollections) && property.Name == nameof(EntityWithCollections.Properties));
        }
    }

    [TestClass]
    public class NestedPropertyDisplayTest
    {
        [TestMethod]
        public void Compare_NestedProperties_WithDisplayProvider_ShouldUseDisplayNamesInPath()
        {
            // Arrange
            var oldValue = new MainEntity
            {
                Id = 1,
                Name = "Old Name",
                Data = new NestedEntity { Title = "Old Title", Description = "Description" }
            };
            var newValue = new MainEntity
            {
                Id = 1,
                Name = "New Name",
                Data = new NestedEntity { Title = "New Title", Description = "Description" }
            };
            
            var valueProviders = new List<IAuditValueProvider> { new NestedEntityDisplayProvider() };

            // Act
            var changes = EntityComparer.Compare(oldValue, newValue, valueProviders);

            // Assert
            // Check that we have changes for both the Name and the nested Data.Title properties
            Assert.IsTrue(changes.Exists(c => c.Field == "姓名")); // Name in Chinese
            Assert.IsTrue(changes.Exists(c => c.Field == "数据.标题")); // Data.Title in Chinese
            
            var nameChange = changes.First(c => c.Field == "姓名");
            Assert.AreEqual("Old Name", nameChange.OldValue);
            Assert.AreEqual("New Name", nameChange.NewValue);
            
            var titleChange = changes.First(c => c.Field == "数据.标题");
            Assert.AreEqual("Old Title", titleChange.OldValue);
            Assert.AreEqual("New Title", titleChange.NewValue);
        }

        [TestMethod]
        public void Compare_NestedProperties_WithoutDisplayProvider_ShouldUseOriginalNames()
        {
            // Arrange
            var oldValue = new MainEntity
            {
                Id = 1,
                Name = "Old Name",
                Data = new NestedEntity { Title = "Old Title", Description = "Description" }
            };
            var newValue = new MainEntity
            {
                Id = 1,
                Name = "New Name",
                Data = new NestedEntity { Title = "New Title", Description = "Description" }
            };
            
            // No value providers
            IEnumerable<IAuditValueProvider> valueProviders = null;

            // Act
            var changes = EntityComparer.Compare(oldValue, newValue, valueProviders);

            // Assert
            Assert.IsTrue(changes.Exists(c => c.Field == "Name"));
            Assert.IsTrue(changes.Exists(c => c.Field == "Data.Title"));
            
            var nameChange = changes.First(c => c.Field == "Name");
            Assert.AreEqual("Old Name", nameChange.OldValue);
            Assert.AreEqual("New Name", nameChange.NewValue);
            
            var titleChange = changes.First(c => c.Field == "Data.Title");
            Assert.AreEqual("Old Title", titleChange.OldValue);
            Assert.AreEqual("New Title", titleChange.NewValue);
        }

        [TestMethod]
        public void Compare_CollectionProperties_WithDisplayProvider_ShouldUseDisplayNames()
        {
            // Arrange
            var oldValue = new EntityWithCollections
            {
                Id = 1,
                Name = "Test Entity",
                Items = new List<CollectionItem>
                {
                    new CollectionItem { ItemId = 1, ItemName = "Old Item 1", ItemDetails = new NestedEntity { Title = "Old Detail 1", Description = "Desc 1" } },
                    new CollectionItem { ItemId = 2, ItemName = "Item 2", ItemDetails = new NestedEntity { Title = "Detail 2", Description = "Desc 2" } }
                },
                Properties = new Dictionary<string, string> { {"prop1", "oldValue1"} }
            };
            
            var newValue = new EntityWithCollections
            {
                Id = 1,
                Name = "Test Entity",
                Items = new List<CollectionItem>
                {
                    new CollectionItem { ItemId = 1, ItemName = "New Item 1", ItemDetails = new NestedEntity { Title = "New Detail 1", Description = "Desc 1" } }, // Changed name and detail
                    new CollectionItem { ItemId = 2, ItemName = "Item 2", ItemDetails = new NestedEntity { Title = "Detail 2", Description = "Desc 2" } }, // Unchanged
                    new CollectionItem { ItemId = 3, ItemName = "New Item 3", ItemDetails = new NestedEntity { Title = "Detail 3", Description = "Desc 3" } }  // New item
                },
                Properties = new Dictionary<string, string> { {"prop1", "newValue1"}, {"prop2", "newValue2"} } // Changed value and added new entry
            };

            var valueProviders = new List<IAuditValueProvider> { new NestedEntityDisplayProvider() };

            // Act
            var changes = EntityComparer.Compare(oldValue, newValue, valueProviders);

            // Assert
            // Check that we have changes for the items in the collection using the correct format [{Title}({id})]
            Assert.IsTrue(changes.Exists(c => c.Field == "项目列表[Old Item 1(1)].项目名称")); // Items[New Item 1(1)].ItemName in Chinese
            Assert.IsTrue(changes.Exists(c => c.Field == "项目列表[Old Item 1(1)].项目详情.标题")); // Items[New Item 1(1)].ItemDetails.Title in Chinese
            //Assert.IsTrue(changes.Exists(c => c.Field == "属性字典[prop1]")); // Properties["prop1"] in Chinese
            //Assert.IsTrue(changes.Exists(c => c.Field == "属性字典[prop2]")); // Properties["prop2"] in Chinese
            
            var itemNameChange = changes.First(c => c.Field == "项目列表[Old Item 1(1)].项目名称");
            Assert.AreEqual("Old Item 1", itemNameChange.OldValue);
            Assert.AreEqual("New Item 1", itemNameChange.NewValue);
            
            var itemDetailChange = changes.First(c => c.Field == "项目列表[Old Item 1(1)].项目详情.标题");
            Assert.AreEqual("Old Detail 1", itemDetailChange.OldValue);
            Assert.AreEqual("New Detail 1", itemDetailChange.NewValue);
            
            //var propChange = changes.First(c => c.Field == "属性字典[prop1]");
            //Assert.AreEqual("oldValue1", propChange.OldValue);
            //Assert.AreEqual("newValue1", propChange.NewValue);
            
            //var newPropChange = changes.First(c => c.Field == "属性字典[prop2]");
            //Assert.AreEqual(null, newPropChange.OldValue);
            //Assert.AreEqual("newValue2", newPropChange.NewValue);
        }

        [TestMethod]
        public void Compare_CollectionProperties_WithoutDisplayProvider_ShouldUseOriginalNames()
        {
            // Arrange
            var oldValue = new EntityWithCollections
            {
                Id = 1,
                Name = "Test Entity",
                Items = new List<CollectionItem>
                {
                    new CollectionItem { ItemId = 1, ItemName = "Old Item 1", ItemDetails = new NestedEntity { Title = "Old Detail 1", Description = "Desc 1" } },
                    new CollectionItem { ItemId = 2, ItemName = "Item 2", ItemDetails = new NestedEntity { Title = "Detail 2", Description = "Desc 2" } }
                },
                Properties = new Dictionary<string, string> { {"prop1", "oldValue1"} }
            };
            
            var newValue = new EntityWithCollections
            {
                Id = 1,
                Name = "Test Entity",
                Items = new List<CollectionItem>
                {
                    new CollectionItem { ItemId = 1, ItemName = "New Item 1", ItemDetails = new NestedEntity { Title = "New Detail 1", Description = "Desc 1" } }, // Changed name and detail
                    new CollectionItem { ItemId = 2, ItemName = "Item 2", ItemDetails = new NestedEntity { Title = "Detail 2", Description = "Desc 2" } }, // Unchanged
                    new CollectionItem { ItemId = 3, ItemName = "New Item 3", ItemDetails = new NestedEntity { Title = "Detail 3", Description = "Desc 3" } }  // New item
                },
                Properties = new Dictionary<string, string> { {"prop1", "newValue1"}, {"prop2", "newValue2"} } // Changed value and added new entry
            };

            // No value providers
            IEnumerable<IAuditValueProvider> valueProviders = null;

            // Act
            var changes = EntityComparer.Compare(oldValue, newValue, valueProviders);

            // Assert
            Assert.IsTrue(changes.Exists(c => c.Field == "Items[Old Item 1(1)].ItemName"));
            Assert.IsTrue(changes.Exists(c => c.Field == "Items[Old Item 1(1)].ItemDetails.Title"));
            //Assert.IsTrue(changes.Exists(c => c.Field == "Properties[prop1]"));
            //Assert.IsTrue(changes.Exists(c => c.Field == "Properties[prop2]"));
            
            var itemNameChange = changes.First(c => c.Field == "Items[Old Item 1(1)].ItemName");
            Assert.AreEqual("Old Item 1", itemNameChange.OldValue);
            Assert.AreEqual("New Item 1", itemNameChange.NewValue);
            
            var itemDetailChange = changes.First(c => c.Field == "Items[Old Item 1(1)].ItemDetails.Title");
            Assert.AreEqual("Old Detail 1", itemDetailChange.OldValue);
            Assert.AreEqual("New Detail 1", itemDetailChange.NewValue);
            
            //var propChange = changes.First(c => c.Field == "Properties[prop1]");
            //Assert.AreEqual("oldValue1", propChange.OldValue);
            //Assert.AreEqual("newValue1", propChange.NewValue);
            
            //var newPropChange = changes.First(c => c.Field == "Properties[prop2]");
            //Assert.AreEqual(null, newPropChange.OldValue);
            //Assert.AreEqual("newValue2", newPropChange.NewValue);
        }
    }
}