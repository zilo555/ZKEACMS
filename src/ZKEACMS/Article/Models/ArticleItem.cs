/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */


using Easy.AuditTrail.Attributes;
using Easy.MetaData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZKEACMS.Article.Models
{
    public class ArticleItem
    {
        [AuditTitle]
        public string Title { get; set; }

        [AuditKey]
        public int ArticleID { get; set; }
    }
    class ArticleGalleryProductMetaData : ViewMetaData<ArticleItem>
    {
        protected override void ViewConfigure()
        {
            ViewConfig(m => m.ArticleID).AsHidden().Required();
            ViewConfig(m => m.Title).AsTextBox().Required();
        }
    }
}
