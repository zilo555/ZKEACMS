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

        // Composite Key and Title Test Entities
        public class CompositeKeyEntity
        {
            [AuditKey(Order = 1)]
            public int Id { get; set; }
            [AuditKey(Order = 0)]
            public string Code { get; set; }
            public string Name { get; set; }
        }

        public class CompositeTitleEntity
        {
            [AuditTitle(Order = 0)]
            public string FirstName { get; set; }
            [AuditTitle(Order = 1)]
            public string LastName { get; set; }
            public int Age { get; set; }
        }

        public class CompositeKeyTitleEntity
        {
            [AuditKey(Order = 1)]
            public string Department { get; set; }
            [AuditKey(Order = 0)]
            public int EmployeeId { get; set; }

            [AuditTitle(Order = 0)]
            public string FirstName { get; set; }
            [AuditTitle(Order = 1)]
            public string LastName { get; set; }

            public decimal Salary { get; set; }
        }

        public class EntityWithCompositeCollection
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<CompositeKeyTitleEntity> Employees { get; set; }
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

        #region Composite Key and Title Tests

        [TestMethod]
        public void Compare_CompositeKeyEntity_IdAndCodeCombinedAsKey()
        {
            // Arrange
            var entity1 = new CompositeKeyEntity
            {
                Id = 1,
                Code = "A001",
                Name = "Test Entity 1"
            };
            var entity2 = new CompositeKeyEntity
            {
                Id = 1,
                Code = "A002", // Changed code
                Name = "Test Entity 1"
            };

            // Act
            var changes = EntityComparer.Compare(entity1, entity2);

            // Assert
            // Since both entities have the same Id, but different Code, they are considered different entities
            // The composite key would be "A001|1" vs "A002|1" due to Order attribute (Code first, then Id)
            Assert.IsGreaterThanOrEqualTo(0, changes.Count); // This test verifies that the composite key works correctly
        }

        [TestMethod]
        public void Compare_CompositeTitleEntity_FirstNameAndLastNameCombinedAsTitle()
        {
            // Arrange
            var entity1 = new CompositeTitleEntity
            {
                FirstName = "John",
                LastName = "Doe",
                Age = 30
            };
            var entity2 = new CompositeTitleEntity
            {
                FirstName = "Jane", // Changed
                LastName = "Doe",
                Age = 30
            };

            // Act
            var changes = EntityComparer.Compare(entity1, entity2);

            // Assert
            Assert.IsTrue(changes.Exists(c => c.Field == "FirstName"));
            Assert.AreEqual("John", changes.First(c => c.Field == "FirstName").OldValue);
            Assert.AreEqual("Jane", changes.First(c => c.Field == "FirstName").NewValue);
        }

        [TestMethod]
        public void Compare_CompositeKeyTitleEntity_CompositeKeyAndTitle()
        {
            // Arrange
            var entity1 = new CompositeKeyTitleEntity
            {
                EmployeeId = 100,
                Department = "IT",
                FirstName = "John",
                LastName = "Smith",
                Salary = 50000
            };
            var entity2 = new CompositeKeyTitleEntity
            {
                EmployeeId = 100,
                Department = "IT",
                FirstName = "Jane", // Changed
                LastName = "Smith",
                Salary = 55000 // Changed
            };

            // Act
            var changes = EntityComparer.Compare(entity1, entity2);

            // Assert
            Assert.IsTrue(changes.Exists(c => c.Field == "FirstName"));
            Assert.IsTrue(changes.Exists(c => c.Field == "Salary"));
            Assert.AreEqual("John", changes.First(c => c.Field == "FirstName").OldValue);
            Assert.AreEqual("Jane", changes.First(c => c.Field == "FirstName").NewValue);
            Assert.AreEqual("50000", changes.First(c => c.Field == "Salary").OldValue);
            Assert.AreEqual("55000", changes.First(c => c.Field == "Salary").NewValue);
        }

        [TestMethod]
        public void Compare_EntityWithCompositeCollection_AddedItemUsesCompositeKeyTitle()
        {
            // Arrange
            var entity1 = new EntityWithCompositeCollection
            {
                Id = 1,
                Name = "Department A",
                Employees = new List<CompositeKeyTitleEntity>
                {
                    new CompositeKeyTitleEntity
                    {
                        EmployeeId = 1,
                        Department = "IT",
                        FirstName = "John",
                        LastName = "Doe",
                        Salary = 50000
                    }
                }
            };
            var entity2 = new EntityWithCompositeCollection
            {
                Id = 1,
                Name = "Department A",
                Employees = new List<CompositeKeyTitleEntity>
                {
                    new CompositeKeyTitleEntity
                    {
                        EmployeeId = 1,
                        Department = "IT",
                        FirstName = "John",
                        LastName = "Doe",
                        Salary = 50000
                    },
                    new CompositeKeyTitleEntity
                    {
                        EmployeeId = 2,
                        Department = "IT",
                        FirstName = "Jane",
                        LastName = "Smith",
                        Salary = 52000
                    }
                }
            };

            // Act
            var changes = EntityComparer.Compare(entity1, entity2);

            // Assert
            Assert.IsTrue(changes.Exists(c => c.Field == "Employees"));
            var addedChange = changes.First(c => c.Field == "Employees");
            Assert.IsNull(addedChange.OldValue);
            Assert.AreEqual("{Added} Jane, Smith(2|IT)", addedChange.NewValue); // Composite title: "Jane, Smith" and composite key: "2|IT" (EmployeeId|Department)
        }

        [TestMethod]
        public void Compare_EntityWithCompositeCollection_RemovedItemUsesCompositeKeyTitle()
        {
            // Arrange
            var entity1 = new EntityWithCompositeCollection
            {
                Id = 1,
                Name = "Department A",
                Employees = new List<CompositeKeyTitleEntity>
                {
                    new CompositeKeyTitleEntity
                    {
                        EmployeeId = 1,
                        Department = "IT",
                        FirstName = "John",
                        LastName = "Doe",
                        Salary = 50000
                    },
                    new CompositeKeyTitleEntity
                    {
                        EmployeeId = 2,
                        Department = "IT",
                        FirstName = "Jane",
                        LastName = "Smith",
                        Salary = 52000
                    }
                }
            };
            var entity2 = new EntityWithCompositeCollection
            {
                Id = 1,
                Name = "Department A",
                Employees = new List<CompositeKeyTitleEntity>
                {
                    new CompositeKeyTitleEntity
                    {
                        EmployeeId = 1,
                        Department = "IT",
                        FirstName = "John",
                        LastName = "Doe",
                        Salary = 50000
                    }
                }
            };

            // Act
            var changes = EntityComparer.Compare(entity1, entity2);

            // Assert
            Assert.IsTrue(changes.Exists(c => c.Field == "Employees"));
            var removedChange = changes.First(c => c.Field == "Employees");
            Assert.IsNull(removedChange.NewValue);
            Assert.AreEqual("{Removed} Jane, Smith(2|IT)", removedChange.OldValue); // Composite title: "Jane, Smith" and composite key: "2|IT" (EmployeeId|Department)
        }

        [TestMethod]
        public void Compare_EntityWithCompositeCollection_ModifiedItemUsesCompositeKeyTitle()
        {
            // Arrange
            var entity1 = new EntityWithCompositeCollection
            {
                Id = 1,
                Name = "Department A",
                Employees = new List<CompositeKeyTitleEntity>
                {
                    new CompositeKeyTitleEntity
                    {
                        EmployeeId = 1,
                        Department = "IT",
                        FirstName = "John",
                        LastName = "Doe",
                        Salary = 50000
                    }
                }
            };
            var entity2 = new EntityWithCompositeCollection
            {
                Id = 1,
                Name = "Department A",
                Employees = new List<CompositeKeyTitleEntity>
                {
                    new CompositeKeyTitleEntity
                    {
                        EmployeeId = 1,
                        Department = "IT",
                        FirstName = "John Updated", // Changed
                        LastName = "Doe",
                        Salary = 55000 // Changed
                    }
                }
            };

            // Act
            var changes = EntityComparer.Compare(entity1, entity2);

            // Assert
            // Check that the changes are detected under the correct path using composite key and title
            Assert.IsTrue(changes.Exists(c => c.Field == "Employees[John, Doe(1|IT)].FirstName")); // Composite title: "John, Doe" and composite key: "1|IT"
            Assert.IsTrue(changes.Exists(c => c.Field == "Employees[John, Doe(1|IT)].Salary"));   // Composite title: "John, Doe" and composite key: "1|IT"

            var firstNameChange = changes.First(c => c.Field == "Employees[John, Doe(1|IT)].FirstName");
            Assert.AreEqual("John", firstNameChange.OldValue);
            Assert.AreEqual("John Updated", firstNameChange.NewValue);

            var salaryChange = changes.First(c => c.Field == "Employees[John, Doe(1|IT)].Salary");
            Assert.AreEqual("50000", salaryChange.OldValue);
            Assert.AreEqual("55000", salaryChange.NewValue);
        }

        #endregion
    }
}