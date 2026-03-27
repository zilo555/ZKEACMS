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
    internal class ArticleTypeAuditValueProvider : IAuditValueProvider
    {
        private readonly IArticleTypeService _articleTypeService;

        public ArticleTypeAuditValueProvider(IArticleTypeService articleTypeService)
        {
            _articleTypeService = articleTypeService;
        }

        public int Priority => 10;

        public bool CanHandle(PropertyInfo property, Type entityType)
        {
            return (property.Name == nameof(ArticleEntity.ArticleTypeID) && entityType == typeof(ArticleEntity)) ||
                (property.Name == nameof(ArticleListWidget.ArticleTypeID) && entityType == typeof(ArticleListWidget)) ||
                (property.Name == nameof(ArticleTypeWidget.ArticleTypeID) && entityType == typeof(ArticleTypeWidget)) ||
                (property.Name == nameof(ArticleTopWidget.ArticleTypeID) && entityType == typeof(ArticleTopWidget));
        }

        public string GetDisplayValue(PropertyInfo property, object rawValue)
        {
            if (rawValue != null && int.TryParse(rawValue.ToString(), out int articleTypeID))
            {
                var articleType = _articleTypeService.Get(articleTypeID);
                if (articleType != null)
                {
                    return articleType.Title;
                }
            }
            return rawValue?.ToString() ?? string.Empty;
        }
    }
}
