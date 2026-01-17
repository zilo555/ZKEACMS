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
        #region Test Entities

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

        public class EntityWithNested
        {
            [AuditKey]
            public int Id { get; set; }
            public string Name { get; set; }
            public SimpleEntity Owner { get; set; }
        }

        public class EntityWithCollection
        {
            [AuditKey]
            public int Id { get; set; }
            public string Name { get; set; }
            public List<OrderItem> Items { get; set; }
        }

        public class OrderItem
        {
            [AuditKey]
            public int ItemId { get; set; }
            [AuditTitle]
            public string ProductName { get; set; }
            public int Quantity { get; set; }
        }

        public class EntityWithArray
        {
            public int Id { get; set; }
            public string[] Tags { get; set; }
        }

        #endregion

        #region Basic Property Comparison Tests

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

        #region Nested Object Comparison Tests

        [TestMethod]
        public void Compare_NestedObject_ChangesDetected()
        {
            // Arrange
            var entity1 = new EntityWithNested
            {
                Id = 1,
                Name = "Parent",
                Owner = new SimpleEntity { Id = 100, Name = "OldOwner", Age = 30 }
            };
            var entity2 = new EntityWithNested
            {
                Id = 1,
                Name = "Parent",
                Owner = new SimpleEntity { Id = 100, Name = "NewOwner", Age = 35 }
            };

            // Act
            var changes = EntityComparer.Compare(entity1, entity2);

            // Assert
            Assert.IsTrue(changes.Exists(c => c.Field == "Owner.Name"));
            Assert.IsTrue(changes.Exists(c => c.Field == "Owner.Age"));
            Assert.AreEqual("OldOwner", changes.First(c => c.Field == "Owner.Name").OldValue);
            Assert.AreEqual("NewOwner", changes.First(c => c.Field == "Owner.Name").NewValue);
            Assert.AreEqual("30", changes.First(c => c.Field == "Owner.Age").OldValue);
            Assert.AreEqual("35", changes.First(c => c.Field == "Owner.Age").NewValue);
        }

        [TestMethod]
        public void Compare_NestedObject_NullToValue_ChangeDetected()
        {
            // Arrange
            var entity1 = new EntityWithNested
            {
                Id = 1,
                Name = "Parent",
                Owner = null
            };
            var entity2 = new EntityWithNested
            {
                Id = 1,
                Name = "Parent",
                Owner = new SimpleEntity { Id = 100, Name = "NewOwner", Age = 30 }
            };

            // Act
            var changes = EntityComparer.Compare(entity1, entity2);

            // Assert
            Assert.HasCount(3, changes);
            Assert.AreEqual("100", changes.First(c => c.Field == "Owner.Id").NewValue);
            Assert.AreEqual("NewOwner", changes.First(c => c.Field == "Owner.Name").NewValue);
            Assert.AreEqual("30", changes.First(c => c.Field == "Owner.Age").NewValue);
        }

        #endregion

        #region IgnoreAudit Attribute Tests

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

        #region DateTime Comparison Tests

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

        #region Collection Comparison Tests

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
            Assert.AreEqual("{Added} Product2(2)", changes[0].NewValue);
            Assert.IsNull(changes[0].OldValue);
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
            Assert.AreEqual("{Removed} Product2(2)", changes[0].OldValue);
            Assert.IsNull(changes[0].NewValue);
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
            Assert.AreEqual("Items[Product1(1)].Quantity", changes[0].Field);
            Assert.AreEqual("2", changes[0].OldValue);
            Assert.AreEqual("5", changes[0].NewValue);
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
            Assert.AreEqual("{Added} Product1(1)", changes[0].NewValue);
        }

        #endregion
    }
}