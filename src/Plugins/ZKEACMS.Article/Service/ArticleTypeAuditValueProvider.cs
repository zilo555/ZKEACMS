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
            return property.Name == nameof(ArticleEntity.ArticleTypeID) && entityType == typeof(ArticleEntity);
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
