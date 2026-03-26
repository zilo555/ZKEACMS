/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using Easy.AuditTrail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ZKEACMS.Common.Models;
using ZKEACMS.Widget;
using ZKEACMS.Zone;

namespace ZKEACMS.Common.Service
{
    public class AuditCarouselValueProvider : IAuditValueProvider
    {
        private readonly ICarouselService _carouselService;

        public AuditCarouselValueProvider(ICarouselService carouselService)
        {
            _carouselService = carouselService;
        }

        public int Priority => 10;
        public bool CanHandle(PropertyInfo property, Type entityType)
        {
            return property.Name.Equals(nameof(CarouselWidget.CarouselID)) && typeof(CarouselWidget).IsAssignableFrom(entityType);
        }

        public string GetDisplayValue(PropertyInfo property, object rawValue)
        {
            if (rawValue == null) return string.Empty;

            return _carouselService.Get((int)rawValue)?.Title ?? rawValue.ToString();
        }
    }
}
