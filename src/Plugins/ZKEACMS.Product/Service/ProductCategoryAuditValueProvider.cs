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
using ZKEACMS.Product.Models;

namespace ZKEACMS.Product.Service
{
    internal class ProductCategoryAuditValueProvider : IAuditValueProvider
    {
        private readonly IProductCategoryService _productCategoryService;

        public ProductCategoryAuditValueProvider(IProductCategoryService productCategoryService)
        {
            _productCategoryService = productCategoryService;
        }
        
        public int Priority => 10;

        public bool CanHandle(PropertyInfo property, Type entityType)
        {
            return (entityType == typeof(ProductEntity) && property.Name == nameof(ProductEntity.ProductCategoryID))||
                (entityType == typeof(ProductListWidget) && property.Name == nameof(ProductListWidget.ProductCategoryID)) ||
                (entityType == typeof(ProductCategoryWidget) && property.Name == nameof(ProductCategoryWidget.ProductCategoryID));
        }

        public string GetDisplayValue(PropertyInfo property, object rawValue)
        {
            if (rawValue is int categoryId)
            {
                var category = _productCategoryService.Get(categoryId);
                return category?.Title ?? rawValue.ToString();
            }
            return rawValue?.ToString() ?? string.Empty;
        }
    }
}
