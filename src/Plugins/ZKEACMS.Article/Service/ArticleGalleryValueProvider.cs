/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using Easy.AuditTrail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ZKEACMS.Article.Models;

namespace ZKEACMS.Article.Service
{
    internal class ArticleGalleryValueProvider : IAuditValueProvider
    {
        private readonly IArticleGalleryService _articleGalleryService;

        public ArticleGalleryValueProvider(IArticleGalleryService articleGalleryService)
        {
            _articleGalleryService = articleGalleryService;
        }

        public int Priority => 10;

        public bool CanHandle(PropertyInfo property, Type entityType)
        {
            return (property.Name == nameof(ArticleGalleryWidget.ArticleGalleryId) && entityType == typeof(ArticleGalleryWidget));
        }

        public string GetDisplayValue(PropertyInfo property, object rawValue)
        {
            if (rawValue == null) return string.Empty;

            return _articleGalleryService.Get((int)rawValue)?.Title ?? rawValue.ToString();
        }
    }
}
