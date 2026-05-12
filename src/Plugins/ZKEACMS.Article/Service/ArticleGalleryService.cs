/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using Easy;
using Easy.AuditTrail;
using Easy.RepositoryPattern;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZKEACMS.Article.Models;

namespace ZKEACMS.Article.Service
{
    public class ArticleGalleryService : ServiceBase<ArticleGallery>, IArticleGalleryService
    {
        private readonly IArticleService _articleService;
        private readonly IAuditTrailService _auditTrailService;
        public ArticleGalleryService(IApplicationContext applicationContext,
            CMSDbContext dbContext,
            IArticleService articleService,
            IAuditTrailService auditTrailService)
            : base(applicationContext, dbContext)
        {
            _articleService = articleService;
            _auditTrailService = auditTrailService;
        }
        public override ArticleGallery Get(params object[] primaryKey)
        {
            var gallery = base.Get(primaryKey);
            if (gallery != null)
            {
                var articleIds = gallery.Articles.Where(m => m.Article != null).Select(m => m.Article.ArticleID).ToArray();
                if (articleIds.Length > 0)
                {
                    var articles = _articleService.Get(m => articleIds.Contains(m.ID));
                    foreach (var item in gallery.Articles.Where(m => m.Article != null))
                    {
                        item.Article.Title = articles.FirstOrDefault(m => m.ID == item.Article.ArticleID)?.Title;
                    }
                }
            }
            return gallery;
        }
        public override ErrorOr<ArticleGallery> Update(ArticleGallery item)
        {
            var oldItem = Get(item.ID);
            var result = base.Update(item);
            if (result.IsSuccess)
            {
                var newItem = Get(item.ID);
                _auditTrailService.AuditUpdate(oldItem, newItem);
            }
            return result;
        }
    }
}
