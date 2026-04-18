/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using Easy.AuditTrail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ZKEACMS.AuditTrail.Service;

namespace ZKEACMS.AuditTrail.Test
{
    public class TestEntityWithTitle
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }

    public class TitleDisplayProvider : IAuditDisplayProvider
    {
        public int Priority => 10;
        public string GetDisplayValue(PropertyInfo property, object rawValue)
        {
            throw new NotImplementedException();
        }

        public string GetDisplayName(PropertyInfo property, Type entityType)
        {
            if (entityType == typeof(TestEntityWithTitle) && property.Name == nameof(TestEntityWithTitle.Title))
            {
                return "标题"; // Chinese for "Title"
            }
            return property.Name; // Return original name for others
        }

        public bool CanHandle(PropertyInfo property, Type entityType)
        {
            return true;
        }
    }

    [TestClass]
    public class AuditDisplayProviderTests
    {
        [TestMethod]
        public void Compare_WithAuditDisplayProvider_ShouldUseDisplayFieldNames()
        {
            // Arrange
            var oldValue = new TestEntityWithTitle { Id = 1, Title = "Old Title", Content = "Old Content" };
            var newValue = new TestEntityWithTitle { Id = 1, Title = "New Title", Content = "New Content" };
            
            var valueProviders = new List<IAuditPropertyProvider> { new TitleDisplayProvider() };

            // Act
            var changes = EntityComparer.Compare(oldValue, newValue, valueProviders);

            // Assert
            Assert.IsTrue(changes.Exists(c => c.Field.Contains("标题") || c.Field.Contains("Content")));
            
            // Check that the Title field was renamed to "标题"
            var titleChange = changes.FirstOrDefault(c => c.Field == "标题");
            Assert.IsNotNull(titleChange);
            Assert.AreEqual("Old Title", titleChange.OldValue);
            Assert.AreEqual("New Title", titleChange.NewValue);
            
            // Check that Content field still uses original name (not handled by GetDisplayName)
            var contentChange = changes.FirstOrDefault(c => c.Field == "Content");
            Assert.IsNotNull(contentChange);
            Assert.AreEqual("Old Content", contentChange.OldValue);
            Assert.AreEqual("New Content", contentChange.NewValue);
        }

        [TestMethod]
        public void Compare_WithoutAuditDisplayProvider_ShouldUseOriginalFieldNames()
        {
            // Arrange
            var oldValue = new TestEntityWithTitle { Id = 1, Title = "Old Title", Content = "Old Content" };
            var newValue = new TestEntityWithTitle { Id = 1, Title = "New Title", Content = "New Content" };
            
            // No value providers
            IEnumerable<IAuditValueProvider> valueProviders = null;

            // Act
            var changes = EntityComparer.Compare(oldValue, newValue, valueProviders);

            // Assert
            Assert.IsTrue(changes.Exists(c => c.Field == "Title"));
            Assert.IsTrue(changes.Exists(c => c.Field == "Content"));
            
            var titleChange = changes.First(c => c.Field == "Title");
            Assert.AreEqual("Old Title", titleChange.OldValue);
            Assert.AreEqual("New Title", titleChange.NewValue);
            
            var contentChange = changes.First(c => c.Field == "Content");
            Assert.AreEqual("Old Content", contentChange.OldValue);
            Assert.AreEqual("New Content", contentChange.NewValue);
        }
    }
}