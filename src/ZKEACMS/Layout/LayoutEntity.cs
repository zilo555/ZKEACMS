/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using Easy.AuditTrail.Attributes;
using Easy.MetaData;
using Easy.Models;
using Easy.RepositoryPattern;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ZKEACMS.Extend;
using ZKEACMS.Filter;
using ZKEACMS.Page;
using ZKEACMS.Theme;
using ZKEACMS.Widget;
using ZKEACMS.Zone;

namespace ZKEACMS.Layout
{
    [DataTable("CMS_Layout")]
    public class LayoutEntity : EditorEntity, IImage
    {
        public const string DefaultThumbnial = "~/images/layout.jpg";
        [Key]
        public string ID { get; set; }

        public string LayoutName { get; set; }
        public string ContainerClass { get; set; }
        public string Script { get; set; }
        public string Style { get; set; }
        [NotMapped, AuditIgnore]
        public ZoneCollection Zones { get; set; }
        [NotMapped, AuditIgnore]
        public ZoneWidgetCollection ZoneWidgets { get; set; }
        [NotMapped, AuditIgnore]
        public LayoutHtmlCollection Html { get; set; }
        [NotMapped, AuditIgnore]
        public PageEntity Page { get; set; }
        [NotMapped, AuditIgnore]
        public PageEntity PreViewPage { get; set; }

        public string ImageUrl { get; set; }
        public string ImageThumbUrl { get; set; }
        [NotMapped, AuditIgnore]
        public ThemeEntity CurrentTheme { get; set; }
        [NotMapped, AuditIgnore]
        public string Layout { get; set; }

        [NotMapped, AuditIgnore]
        public IEnumerable<WidgetBasePart> Templates { get; set; }
    }

    class LayoutEntityMetaData : ViewMetaData<LayoutEntity>
    {

        protected override void ViewConfigure()
        {
            ViewConfig(m => m.ID).AsHidden();
            ViewConfig(m => m.Layout).AsHidden();
            ViewConfig(m => m.ContainerClass).AsHidden();
            ViewConfig(m => m.Title).AsHidden();
            ViewConfig(m => m.LayoutName).AsTextBox().Required();
            ViewConfig(m => m.Script).AsTextBox().FileSelector();
            ViewConfig(m => m.Style).AsTextBox().FileSelector();
            ViewConfig(m => m.ImageThumbUrl).AsHidden();
            ViewConfig(m => m.ImageUrl).AsHidden();
            ViewConfig(m => m.Zones).AsHidden().Ignore();
            ViewConfig(m => m.ZoneWidgets).AsHidden().Ignore();
            ViewConfig(m => m.Html).AsHidden().Ignore();
            ViewConfig(m => m.Page).AsHidden().Ignore();
            ViewConfig(m => m.PreViewPage).AsHidden().Ignore();
            ViewConfig(m => m.CurrentTheme).AsHidden().Ignore();
            ViewConfig(m => m.Templates).AsHidden().Ignore();
        }
    }

}
