/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using Easy;
using Easy.AuditTrail;
using Easy.Cache;
using Easy.Extend;
using Easy.Modules.MutiLanguage;
using Easy.Modules.Role;
using Easy.RepositoryPattern;
using Easy.Serializer;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using ZKEACMS.Event;
using ZKEACMS.Layout;
using ZKEACMS.Options;
using ZKEACMS.Page;
using ZKEACMS.Widget;
using ZKEACMS.Zone;

namespace ZKEACMS.Test.Page
{
    [TestClass]
    public class PagePublishTest
    {
        private const string LayoutId = "TestLayout";

        [TestMethod]
        public void Publish_ShouldStoreOnlyPageWidgetsInContent()
        {
            // Arrange
            var (pageService, widgetServiceMock, seedPage) = CreatePageServiceWithMocks();

            var draftPage = new PageEntity
            {
                ID = "TestPage",
                LayoutId = LayoutId,
                Url = "~/test-page",
                Title = "Test Page",
                IsPublishedPage = false,
                PageName = "TestPageName"
            };
            seedPage(draftPage);

            // Act
            var result = pageService.Publish(draftPage);

            // Assert
            Assert.IsFalse(result.HasError, $"Publish failed: {result.Errors.FirstOrDefault()?.Message}");

            var publishedPage = result.Result;
            Assert.IsNotNull(publishedPage.Content, "Published page should have serialized content");

            var content = JsonConverter.DeserializePolymorphic<PageContent>(publishedPage.Content);
            Assert.IsNotNull(content, "PageContent should be deserializable");

            // Content should contain only page-specific widgets (not layout widgets)
            Assert.IsFalse(content.Widgets.Any(w => w.ID == "LayoutWidget"),
                "Layout widget should NOT be in Content snapshot — it must come from DB live");
            Assert.IsTrue(content.Widgets.Any(w => w.ID == "PageWidget"),
                "Page-specific widget should be included in published page content");
            Assert.HasCount(1, content.Widgets,
                "Published page content should contain exactly 1 widget (page-specific only)");

            // Layout widgets must still be queryable from DB after publish
            var layoutWidgets = widgetServiceMock.Object.GetByLayoutId(LayoutId);
            Assert.IsTrue(layoutWidgets.Any(w => w.ID == "LayoutWidget"),
                "Layout widget should still be accessible via GetByLayoutId after publish");
        }

        [TestMethod]
        public void GetAllByPage_ShouldCombineLayoutAndPageWidgetsForPublishedPage()
        {
            // Arrange
            var (widgetService, publishedPage) = CreateRealWidgetServiceWithPublishedPage();

            // Act
            var allWidgets = widgetService.GetAllByPage(publishedPage);

            // Assert
            Assert.IsTrue(allWidgets.Any(w => w.ID == "LayoutWidget"),
                "Layout widget should be returned by GetAllByPage for published page");
            Assert.IsTrue(allWidgets.Any(w => w.ID == "PageWidget"),
                "Page-specific widget should be returned by GetAllByPage for published page");
            Assert.IsGreaterThanOrEqualTo(allWidgets.Count(), 2,
                "GetAllByPage should return at least 2 widgets (layout + page)");
        }

        #region Test Helpers

        /// <summary>
        /// Creates a PageService with mocked dependencies for testing Publish() behavior.
        /// Returns (pageService, widgetServiceMock, seedPageAction).
        /// </summary>
        private (PageService, Mock<IWidgetBasePartService>, Action<PageEntity>) CreatePageServiceWithMocks()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var dbOptions = new DbContextOptionsBuilder<CMSDbContext>()
                .UseSqlite(connection)
                .Options;

            var dbContext = new CMSDbContext(dbOptions, new[] { new TestEasyModelCreating() }, new DatabaseOption());
            dbContext.Database.EnsureCreated();

            dbContext.Layout.Add(new LayoutEntity { ID = LayoutId, LayoutName = "TestLayoutName" });
            dbContext.SaveChanges();

            // ---- Mock IWidgetBasePartService ----
            var widgetServiceMock = new Mock<IWidgetBasePartService>();

            // Layout widget: has LayoutId, no PageId
            var layoutWidgetPart = new WidgetBasePart
            {
                ID = "LayoutWidget",
                LayoutId = LayoutId,
                PageId = null,
                Title = "Layout Widget",
                Status = (int)WidgetStatus.Visible
            };
            var layoutWidgetBase = layoutWidgetPart.ToWidgetBase();

            // Page-specific widget: PageId set dynamically
            var pageWidgetPart = new WidgetBasePart
            {
                ID = "PageWidget",
                Title = "Page Widget",
                Status = (int)WidgetStatus.Visible
            };

            string actualPageId = null;
            widgetServiceMock.Setup(m => m.GetByLayoutId(It.Is<string>(id => id == LayoutId)))
                .Returns(new[] { layoutWidgetBase });
            widgetServiceMock.Setup(m => m.GetByPageId(It.IsAny<string>()))
                .Returns<string>(id =>
                {
                    if (id == actualPageId)
                    {
                        var pageWidgetBase = pageWidgetPart.ToWidgetBase();
                        pageWidgetBase.PageId = id;
                        return new[] { pageWidgetBase };
                    }
                    return Enumerable.Empty<WidgetBase>();
                });

            // ---- Mock IWidgetActivator ----
            var widgetDriverMock = new Mock<IWidgetPartDriver>();
            widgetDriverMock.Setup(m => m.GetWidget(It.IsAny<WidgetBase>()))
                .Returns<WidgetBase>(w =>
                {
                    var copy = new WidgetBase();
                    w.CopyTo(copy);
                    return copy;
                });
            widgetDriverMock.Setup(m => m.Publish(It.IsAny<WidgetBase>()));
            widgetDriverMock.Setup(m => m.Dispose());

            var widgetActivatorMock = new Mock<IWidgetActivator>();
            widgetActivatorMock.Setup(m => m.Create(It.IsAny<WidgetBase>()))
                .Returns(widgetDriverMock.Object);

            // ---- Mock other services ----
            var zoneServiceMock = new Mock<IZoneService>();
            zoneServiceMock.Setup(m => m.GetByPageId(It.IsAny<string>()))
                .Returns(Enumerable.Empty<ZoneEntity>());

            var layoutHtmlServiceMock = new Mock<ILayoutHtmlService>();
            layoutHtmlServiceMock.Setup(m => m.GetByPageId(It.IsAny<string>()))
                .Returns(Enumerable.Empty<LayoutHtml>());

            var eventManagerMock = new Mock<IEventManager>();
            eventManagerMock.Setup(m => m.Trigger(It.IsAny<string>(), It.IsAny<object>()));
            eventManagerMock.Setup(m => m.Trigger(It.IsAny<EventArg>(), It.IsAny<object>()));

            // ---- ServiceLocator (required by ServiceBase) ----
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddHttpContextAccessor();
            ServiceLocator.Setup(serviceCollection.BuildServiceProvider());

            // ---- Create PageService ----
            var appContextMock = new Mock<IApplicationContext>();
            var localizeMock = new Mock<ILocalize>();
            var auditTrailMock = new Mock<IAuditTrailService>();

            var pageService = new PageService(
                widgetServiceMock.Object,
                appContextMock.Object,
                widgetActivatorMock.Object,
                zoneServiceMock.Object,
                layoutHtmlServiceMock.Object,
                localizeMock.Object,
                dbContext,
                eventManagerMock.Object,
                auditTrailMock.Object
            );

            void SeedPage(PageEntity page)
            {
                if (dbContext.Page.Find(page.ID) == null)
                {
                    pageService.Add(page);
                    actualPageId = page.ID; // Add() generates a new GUID
                }
            }

            return (pageService, widgetServiceMock, SeedPage);
        }

        /// <summary>
        /// Creates a real WidgetBasePartService and a published page in the DB,
        /// then returns (widgetService, publishedPage).
        /// </summary>
        private (WidgetBasePartService, PageEntity) CreateRealWidgetServiceWithPublishedPage()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var dbOptions = new DbContextOptionsBuilder<CMSDbContext>()
                .UseSqlite(connection)
                .Options;

            var dbContext = new CMSDbContext(dbOptions, new[] { new TestEasyModelCreating() }, new DatabaseOption());
            dbContext.Database.EnsureCreated();

            // Seed layout
            dbContext.Layout.Add(new LayoutEntity { ID = LayoutId, LayoutName = "TestLayout" });
            // Seed a layout widget in the DB
            dbContext.WidgetBasePart.Add(new WidgetBasePart
            {
                ID = "LayoutWidget",
                LayoutId = LayoutId,
                PageId = null,
                Title = "Layout Widget",
                Status = (int)WidgetStatus.Visible
            });
            dbContext.SaveChanges();

            // ---- Mock IWidgetActivator ----
            var widgetDriverMock = new Mock<IWidgetPartDriver>();
            widgetDriverMock.Setup(m => m.GetWidget(It.IsAny<WidgetBase>()))
                .Returns<WidgetBase>(w =>
                {
                    var copy = new WidgetBase();
                    w.CopyTo(copy);
                    return copy;
                });
            widgetDriverMock.Setup(m => m.Publish(It.IsAny<WidgetBase>()));
            widgetDriverMock.Setup(m => m.Dispose());

            var widgetActivatorMock = new Mock<IWidgetActivator>();
            widgetActivatorMock.Setup(m => m.Create(It.IsAny<WidgetBase>()))
                .Returns(widgetDriverMock.Object);

            // ---- Mock ICacheManager ----
            var cacheManagerMock = new Mock<ICacheManager<WidgetBasePartService>>();
            var cacheEntry = new Mock<ICacheEntry>();
            cacheEntry.Setup(e => e.ExpirationTokens).Returns(new List<IChangeToken>());
            cacheManagerMock
                .Setup(m => m.GetOrCreate(It.IsAny<string>(), It.IsAny<Func<ICacheEntry, object>>()))
                .Returns<string, Func<ICacheEntry, object>>((key, factory) => factory(cacheEntry.Object));

            // ---- Mock remaining services ----
            var appContextMock = new Mock<IApplicationContext>();
            var eventManagerMock = new Mock<IEventManager>();
            var signalsMock = new Mock<ISignals>();
            signalsMock.Setup(m => m.When(It.IsAny<string>())).Returns(new SignalChangeToken());

            // ---- Create real WidgetBasePartService ----
            var widgetService = new WidgetBasePartService(
                appContextMock.Object,
                widgetActivatorMock.Object,
                cacheManagerMock.Object,
                dbContext,
                eventManagerMock.Object,
                signalsMock.Object
            );

            // ---- ServiceLocator (required by ServiceBase) ----
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddHttpContextAccessor();
            ServiceLocator.Setup(serviceCollection.BuildServiceProvider());

            // ---- Create PageService ----
            var zoneServiceMock = new Mock<IZoneService>();
            zoneServiceMock.Setup(m => m.GetByPageId(It.IsAny<string>()))
                .Returns(Enumerable.Empty<ZoneEntity>());

            var layoutHtmlServiceMock = new Mock<ILayoutHtmlService>();
            layoutHtmlServiceMock.Setup(m => m.GetByPageId(It.IsAny<string>()))
                .Returns(Enumerable.Empty<LayoutHtml>());

            var localizeMock = new Mock<ILocalize>();
            var auditTrailMock = new Mock<IAuditTrailService>();

            var pageService = new PageService(
                widgetService,
                appContextMock.Object,
                widgetActivatorMock.Object,
                zoneServiceMock.Object,
                layoutHtmlServiceMock.Object,
                localizeMock.Object,
                dbContext,
                eventManagerMock.Object,
                auditTrailMock.Object
            );

            // Create draft page and add a page-specific widget to DB
            var draftPage = new PageEntity
            {
                ID = "IntegrationTestPage",
                LayoutId = LayoutId,
                Url = "~/integration-test",
                Title = "Integration Test",
                IsPublishedPage = false,
                PageName = "IntegrationTest"
            };
            pageService.Add(draftPage);

            dbContext.WidgetBasePart.Add(new WidgetBasePart
            {
                ID = "PageWidget",
                PageId = draftPage.ID,
                Title = "Page Widget",
                Status = (int)WidgetStatus.Visible
            });
            dbContext.SaveChanges();

            // Publish the page
            var result = pageService.Publish(draftPage);
            Assert.IsFalse(result.HasError, $"Publish failed: {result.Errors.FirstOrDefault()?.Message}");

            var publishedPage = result.Result;

            // Verify Content only has page-specific widgets (not layout widgets)
            Assert.IsNotNull(publishedPage.Content);
            var content = JsonConverter.DeserializePolymorphic<PageContent>(publishedPage.Content);
            Assert.IsTrue(content.Widgets.Any(w => w.ID == "PageWidget"));
            Assert.IsFalse(content.Widgets.Any(w => w.ID == "LayoutWidget"),
                "Layout widget should NOT be in Content snapshot (should come from DB live)");

            return (widgetService, publishedPage);
        }

        #endregion
    }

    /// <summary>
    /// Provides key configuration for internal EasyDbContext entities
    /// that use composite keys defined via IOnModelCreating.
    /// </summary>
    public class TestEasyModelCreating : IOnModelCreating
    {
        public void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LanguageEntity>().HasKey(m => new { m.LanKey, m.CultureName });
            modelBuilder.Entity<Permission>().HasKey(m => new { m.PermissionKey, m.RoleId });
        }
    }
}