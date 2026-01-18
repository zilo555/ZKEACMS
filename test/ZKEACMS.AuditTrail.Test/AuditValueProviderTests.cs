using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ZKEACMS.AuditTrail.Service;
using ZKEACMS.Common.Service;

namespace ZKEACMS.AuditTrail.Test
{
    public class TestEntityWithStatus
    {
        public int Id { get; set; }
        public int Status { get; set; } // 1 = Active, 2 = Inactive
        public string Name { get; set; }
    }

    public class StatusValueProvider : IAuditValueProvider
    {
        public string GetDisplayValue(PropertyInfo property, object rawValue)
        {
            if (property.Name == nameof(TestEntityWithStatus.Status))
            {
                if (rawValue is int intValue)
                {
                    return intValue switch
                    {
                        1 => "Active",
                        2 => "Inactive",
                        _ => $"Unknown ({intValue})"
                    };
                }
            }
            return rawValue?.ToString();
        }

        public bool CanHandle(PropertyInfo property, System.Type entityType, AuditOperationType operationType)
        {
            return operationType == AuditOperationType.GetValue && 
                   entityType == typeof(TestEntityWithStatus) && 
                   property.Name == nameof(TestEntityWithStatus.Status);
        }
    }

    [TestClass]
    public class AuditValueProviderTests
    {
        [TestMethod]
        public void Compare_WithAuditValueProvider_ShouldUseDisplayValues()
        {
            // Arrange
            var oldValue = new TestEntityWithStatus { Id = 1, Status = 1, Name = "Test" };
            var newValue = new TestEntityWithStatus { Id = 1, Status = 2, Name = "Test" };
            
            var valueProviders = new List<IAuditValueProvider> { new StatusValueProvider() };

            // Act
            var changes = EntityComparer.Compare(oldValue, newValue, valueProviders);

            // Assert
            Assert.IsTrue(changes.Exists(c => c.Field.Contains(nameof(TestEntityWithStatus.Status))));
            var statusChange = changes.First(c => c.Field.Contains(nameof(TestEntityWithStatus.Status)));
            Assert.AreEqual("Active", statusChange.OldValue);
            Assert.AreEqual("Inactive", statusChange.NewValue);
        }

        [TestMethod]
        public void Compare_WithoutAuditValueProvider_ShouldUseRawValues()
        {
            // Arrange
            var oldValue = new TestEntityWithStatus { Id = 1, Status = 1, Name = "Test" };
            var newValue = new TestEntityWithStatus { Id = 1, Status = 2, Name = "Test" };
            
            // No value providers
            IEnumerable<IAuditValueProvider> valueProviders = null;

            // Act
            var changes = EntityComparer.Compare(oldValue, newValue, valueProviders);

            // Assert
            Assert.IsTrue(changes.Exists(c => c.Field.Contains(nameof(TestEntityWithStatus.Status))));
            var statusChange = changes.First(c => c.Field.Contains(nameof(TestEntityWithStatus.Status)));
            Assert.AreEqual("1", statusChange.OldValue);
            Assert.AreEqual("2", statusChange.NewValue);
        }

        [TestMethod]
        public void Compare_PropertyNotHandledByProvider_ShouldUseRawValue()
        {
            // Arrange
            var oldValue = new TestEntityWithStatus { Id = 1, Status = 1, Name = "OldName" };
            var newValue = new TestEntityWithStatus { Id = 1, Status = 1, Name = "NewName" };
            
            var valueProviders = new List<IAuditValueProvider> { new StatusValueProvider() }; // Provider only handles Status

            // Act
            var changes = EntityComparer.Compare(oldValue, newValue, valueProviders);

            // Assert
            Assert.IsTrue(changes.Exists(c => c.Field.Contains(nameof(TestEntityWithStatus.Name))));
            var nameChange = changes.First(c => c.Field.Contains(nameof(TestEntityWithStatus.Name)));
            Assert.AreEqual("OldName", nameChange.OldValue);
            Assert.AreEqual("NewName", nameChange.NewValue);
            
            // Verify that Status did not change (since we changed Name only)
            Assert.IsFalse(changes.Exists(c => c.Field.Contains(nameof(TestEntityWithStatus.Status))));
        }
    }
}