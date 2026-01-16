/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ZKEACMS.AuditTrail.Service;
using ZKEACMS.Common.Models;
using ZKEACMS.Common.Models.Attributes;

[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]

namespace ZKEACMS.AuditTrail.Test
{
    [TestClass]
    public class EntityComparerTests
    {
        #region 测试实体

        public class SimpleEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
            public DateTime CreatedDate { get; set; }
        }

        public class EntityWithIgnore
        {
            public string Name { get; set; }
            
            [IgnoreAudit]
            public string Password { get; set; }
            
            [IgnoreAudit]
            public string Secret { get; set; }
        }

        [IgnoreAudit]
        public class IgnoredEntity
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        public class EntityWithCollection
        {
            [Key]
            public int Id { get; set; }
            public string Name { get; set; }
            public List<OrderItem> Items { get; set; }
        }

        public class OrderItem
        {
            [Key]
            public int ItemId { get; set; }
            public string ProductName { get; set; }
            public int Quantity { get; set; }
        }

        public class EntityWithArray
        {
            public int Id { get; set; }
            public string[] Tags { get; set; }
        }

        #endregion

        #region 基础属性对比测试

        [TestMethod]
        public void Compare_NoChanges_ReturnsEmptyList()
        {
            // Arrange
            var entity1 = new SimpleEntity { Id = 1, Name = "Test", Age = 25 };
            var entity2 = new SimpleEntity { Id = 1, Name = "Test", Age = 25 };

            // Act
            var changes = EntityComparer.Compare(entity1, entity2);

            // Assert
            Assert.IsEmpty(changes);
        }

        [TestMethod]
        public void Compare_SinglePropertyChanged_ReturnsOneChange()
        {
            // Arrange
            var entity1 = new SimpleEntity { Id = 1, Name = "OldName", Age = 25 };
            var entity2 = new SimpleEntity { Id = 1, Name = "NewName", Age = 25 };

            // Act
            var changes = EntityComparer.Compare(entity1, entity2);

            // Assert
            Assert.HasCount(1, changes);
            Assert.AreEqual("Name", changes[0].Field);
            Assert.AreEqual("OldName", changes[0].OldValue);
            Assert.AreEqual("NewName", changes[0].NewValue);
        }

        [TestMethod]
        public void Compare_MultiplePropertiesChanged_ReturnsMultipleChanges()
        {
            // Arrange
            var entity1 = new SimpleEntity { Id = 1, Name = "OldName", Age = 25 };
            var entity2 = new SimpleEntity { Id = 2, Name = "NewName", Age = 30 };

            // Act
            var changes = EntityComparer.Compare(entity1, entity2);

            // Assert
            Assert.HasCount(3, changes);
            Assert.IsTrue(changes.Any(c => c.Field == "Id"));
            Assert.IsTrue(changes.Any(c => c.Field == "Name"));
            Assert.IsTrue(changes.Any(c => c.Field == "Age"));
        }

        [TestMethod]
        public void Compare_NullEntities_ReturnsEmptyList()
        {
            // Act
            var changes1 = EntityComparer.Compare<SimpleEntity>(null, null);
            var changes2 = EntityComparer.Compare<SimpleEntity>(null, new SimpleEntity());
            var changes3 = EntityComparer.Compare<SimpleEntity>(new SimpleEntity(), null);

            // Assert
            Assert.IsEmpty(changes1);
            Assert.IsEmpty(changes2);
            Assert.IsEmpty(changes3);
        }

        #endregion

        #region IgnoreAudit 特性测试

        [TestMethod]
        public void Compare_IgnoredProperties_NotIncludedInChanges()
        {
            // Arrange
            var entity1 = new EntityWithIgnore 
            { 
                Name = "OldName", 
                Password = "OldPassword", 
                Secret = "OldSecret" 
            };
            var entity2 = new EntityWithIgnore 
            { 
                Name = "NewName", 
                Password = "NewPassword", 
                Secret = "NewSecret" 
            };

            // Act
            var changes = EntityComparer.Compare(entity1, entity2);

            // Assert
            Assert.HasCount(1, changes);
            Assert.AreEqual("Name", changes[0].Field);
            Assert.IsFalse(changes.Any(c => c.Field == "Password"));
            Assert.IsFalse(changes.Any(c => c.Field == "Secret"));
        }

        [TestMethod]
        public void Compare_IgnoredEntity_ReturnsEmptyList()
        {
            // Arrange
            var entity1 = new IgnoredEntity { Name = "OldName", Value = "OldValue" };
            var entity2 = new IgnoredEntity { Name = "NewName", Value = "NewValue" };

            // Act
            var changes = EntityComparer.Compare(entity1, entity2);

            // Assert
            Assert.IsEmpty(changes);
        }

        #endregion

        #region DateTime 对比测试

        [TestMethod]
        public void Compare_DateTimeWithinOneSecond_NoChange()
        {
            // Arrange
            var baseTime = DateTime.Now;
            var entity1 = new SimpleEntity { Id = 1, Name = "Test", CreatedDate = baseTime };
            var entity2 = new SimpleEntity { Id = 1, Name = "Test", CreatedDate = baseTime.AddMilliseconds(500) };

            // Act
            var changes = EntityComparer.Compare(entity1, entity2);

            // Assert
            Assert.IsEmpty(changes);
        }

        [TestMethod]
        public void Compare_DateTimeMoreThanOneSecond_HasChange()
        {
            // Arrange
            var baseTime = DateTime.Now;
            var entity1 = new SimpleEntity { Id = 1, Name = "Test", CreatedDate = baseTime };
            var entity2 = new SimpleEntity { Id = 1, Name = "Test", CreatedDate = baseTime.AddSeconds(2) };

            // Act
            var changes = EntityComparer.Compare(entity1, entity2);

            // Assert
            Assert.HasCount(1, changes);
            Assert.AreEqual("CreatedDate", changes[0].Field);
        }

        #endregion

        #region 集合对比测试

        [TestMethod]
        public void Compare_CollectionWithKey_DetectsAddedItem()
        {
            // Arrange
            var entity1 = new EntityWithCollection
            {
                Id = 1,
                Name = "Order",
                Items = new List<OrderItem>
                {
                    new OrderItem { ItemId = 1, ProductName = "Product1", Quantity = 2 }
                }
            };
            var entity2 = new EntityWithCollection
            {
                Id = 1,
                Name = "Order",
                Items = new List<OrderItem>
                {
                    new OrderItem { ItemId = 1, ProductName = "Product1", Quantity = 2 },
                    new OrderItem { ItemId = 2, ProductName = "Product2", Quantity = 3 }
                }
            };

            // Act
            var changes = EntityComparer.Compare(entity1, entity2);

            // Assert
            Assert.HasCount(1, changes);
            Assert.AreEqual("Items", changes[0].Field);
            StringAssert.Contains(changes[0].NewValue, "Added: 2");
        }

        [TestMethod]
        public void Compare_CollectionWithKey_DetectsRemovedItem()
        {
            // Arrange
            var entity1 = new EntityWithCollection
            {
                Id = 1,
                Name = "Order",
                Items = new List<OrderItem>
                {
                    new OrderItem { ItemId = 1, ProductName = "Product1", Quantity = 2 },
                    new OrderItem { ItemId = 2, ProductName = "Product2", Quantity = 3 }
                }
            };
            var entity2 = new EntityWithCollection
            {
                Id = 1,
                Name = "Order",
                Items = new List<OrderItem>
                {
                    new OrderItem { ItemId = 1, ProductName = "Product1", Quantity = 2 }
                }
            };

            // Act
            var changes = EntityComparer.Compare(entity1, entity2);

            // Assert
            Assert.HasCount(1, changes);
            Assert.AreEqual("Items", changes[0].Field);
            StringAssert.Contains(changes[0].NewValue, "Removed: 2");
        }

        [TestMethod]
        public void Compare_CollectionWithKey_DetectsModifiedItem()
        {
            // Arrange
            var entity1 = new EntityWithCollection
            {
                Id = 1,
                Name = "Order",
                Items = new List<OrderItem>
                {
                    new OrderItem { ItemId = 1, ProductName = "Product1", Quantity = 2 }
                }
            };
            var entity2 = new EntityWithCollection
            {
                Id = 1,
                Name = "Order",
                Items = new List<OrderItem>
                {
                    new OrderItem { ItemId = 1, ProductName = "Product1", Quantity = 5 }
                }
            };

            // Act
            var changes = EntityComparer.Compare(entity1, entity2);

            // Assert
            Assert.HasCount(1, changes);
            Assert.AreEqual("Items", changes[0].Field);
            StringAssert.Contains(changes[0].NewValue, "Modified: 1");
        }

        [TestMethod]
        public void Compare_ArrayProperty_DetectsChanges()
        {
            // Arrange
            var entity1 = new EntityWithArray 
            { 
                Id = 1, 
                Tags = new[] { "tag1", "tag2" } 
            };
            var entity2 = new EntityWithArray 
            { 
                Id = 1, 
                Tags = new[] { "tag1", "tag2", "tag3" } 
            };

            // Act
            var changes = EntityComparer.Compare(entity1, entity2);

            // Assert
            Assert.HasCount(1, changes);
            Assert.AreEqual("Tags", changes[0].Field);
        }

        [TestMethod]
        public void Compare_NullCollections_HandledCorrectly()
        {
            // Arrange
            var entity1 = new EntityWithCollection { Id = 1, Name = "Order", Items = null };
            var entity2 = new EntityWithCollection 
            { 
                Id = 1, 
                Name = "Order", 
                Items = new List<OrderItem> 
                { 
                    new OrderItem { ItemId = 1, ProductName = "Product1", Quantity = 1 } 
                } 
            };

            // Act
            var changes = EntityComparer.Compare(entity1, entity2);

            // Assert
            Assert.HasCount(1, changes);
            Assert.AreEqual("Items", changes[0].Field);
        }

        #endregion

        #region SerializeValue 测试

        [TestMethod]
        public void SerializeValue_NullValue_ReturnsNull()
        {
            // Act
            var result = EntityComparer.SerializeValue(null);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void SerializeValue_StringValue_ReturnsString()
        {
            // Act
            var result = EntityComparer.SerializeValue("test string");

            // Assert
            Assert.AreEqual("test string", result);
        }

        [TestMethod]
        public void SerializeValue_IntValue_ReturnsString()
        {
            // Act
            var result = EntityComparer.SerializeValue(123);

            // Assert
            Assert.AreEqual("123", result);
        }

        [TestMethod]
        public void SerializeValue_DateTimeValue_ReturnsFormattedString()
        {
            // Arrange
            var dateTime = new DateTime(2026, 1, 14, 15, 30, 45);

            // Act
            var result = EntityComparer.SerializeValue(dateTime);

            // Assert
            Assert.AreEqual("2026-01-14 15:30:45", result);
        }

        [TestMethod]
        public void SerializeValue_ComplexObject_ReturnsJson()
        {
            // Arrange
            var obj = new SimpleEntity { Id = 1, Name = "Test", Age = 25 };

            // Act
            var result = EntityComparer.SerializeValue(obj);

            // Assert
            Assert.IsNotNull(result);
            StringAssert.Contains(result, "\"Id\":1");
            StringAssert.Contains(result, "\"Name\":\"Test\"");
            StringAssert.Contains(result, "\"Age\":25");
        }

        #endregion
    }
}
