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
    internal class ProductGalleryAuditValueProvider : IAuditValueProvider
    {
        private readonly IProductGalleryService _productGalleryService;

        public ProductGalleryAuditValueProvider(IProductGalleryService productGalleryService)
        {
            _productGalleryService = productGalleryService;
        }

        public int Priority => 10;

        public bool CanHandle(PropertyInfo property, Type entityType)
        {
            return (entityType == typeof(ProductGalleryWidget) && property.Name == nameof(ProductGalleryWidget.ProductGalleryId));
        }

        public string GetDisplayValue(PropertyInfo property, object rawValue)
        {
            if (rawValue is int galleryId)
            {
                var gallery = _productGalleryService.Get(galleryId);
                return gallery?.Title ?? rawValue.ToString();
            }
            return rawValue?.ToString() ?? string.Empty;
        }
    }
}
