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
using ZKEACMS.Product.Models;

namespace ZKEACMS.Product.Service
{
    public class ProductGalleryService : ServiceBase<ProductGallery>, IProductGalleryService
    {
        private readonly IProductService _productService;
        private readonly IAuditTrailService _auditTrailService;

        public ProductGalleryService(IApplicationContext applicationContext, CMSDbContext dbContext, IProductService productService, IAuditTrailService auditTrailService) : base(applicationContext, dbContext)
        {
            _productService = productService;
            _auditTrailService = auditTrailService;
        }

        public override ProductGallery Get(params object[] primaryKey)
        {
            var gallery = base.Get(primaryKey);
            if (gallery != null)
            {
                var productIds = gallery.Products.Where(m => m.Product != null).Select(m => m.Product.ProductID).ToArray();
                if (productIds.Length > 0)
                {
                    var products = _productService.Get(m => productIds.Contains(m.ID));
                    foreach (var item in gallery.Products.Where(m => m.Product != null))
                    {
                        item.Product.Title = products.FirstOrDefault(m => m.ID == item.Product.ProductID)?.Title;
                    }
                }
            }
            return gallery;
        }

        public override ErrorOr<ProductGallery> Update(ProductGallery item)
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
