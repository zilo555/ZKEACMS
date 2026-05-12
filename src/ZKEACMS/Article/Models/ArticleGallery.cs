/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using Easy.AuditTrail.Attributes;
using Easy.MetaData;
using Easy.Models;
using Easy.RepositoryPattern;
using Easy.Serializer;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ZKEACMS.Extend;

namespace ZKEACMS.Article.Models
{
    [DataTable("ArticleGallery")]
    public class ArticleGallery : EditorEntity
    {
        public ArticleGallery()
        {
            Articles = new List<ArticleGalleryItem>();
        }
        [Key]
        public int ID { get; set; }

        [NotMapped]
        public List<ArticleGalleryItem> Articles { get; set; }

        [AuditIgnore]
        public string RawData
        {
            get { return JsonConverter.Serialize(Articles.RemoveDeletedItems()); }
            set { Articles = JsonConverter.Deserialize<List<ArticleGalleryItem>>(value); }
        }
    }
    class ArticleGalleryMetaData : ViewMetaData<ArticleGallery>
    {
        protected override void ViewConfigure()
        {
            ViewConfig(m => m.ID).AsHidden();
            ViewConfig(m => m.RawData).AsHidden().Ignore();

            ViewConfig(m => m.Title).AsTextBox().Required().Order(1).MaxLength(200).ShowInGrid();
            ViewConfig(m => m.Articles).AsListEditor().Order(2).Sortable();
        }
    }
}
