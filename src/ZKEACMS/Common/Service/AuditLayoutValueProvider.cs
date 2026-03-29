/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using Easy.AuditTrail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ZKEACMS.Layout;
using ZKEACMS.Page;
using ZKEACMS.Widget;
using ZKEACMS.Zone;

namespace ZKEACMS.Common.Service
{
    public class AuditLayoutValueProvider : IAuditValueProvider
    {
        private readonly ILayoutService _layoutService;

        public AuditLayoutValueProvider(ILayoutService layoutService)
        {
            _layoutService = layoutService;
        }

        public int Priority => 10;
        public bool CanHandle(PropertyInfo property, Type entityType)
        {
            return property.Name.Equals(nameof(PageEntity.LayoutId)) && typeof(PageEntity) == entityType;
        }

        public string GetDisplayValue(PropertyInfo property, object rawValue)
        {
            if (rawValue == null) return string.Empty;

            if (rawValue is string layoutId)
            {
                var layout = _layoutService.Get(layoutId);
                return layout?.LayoutName ?? layoutId;
            }
            return rawValue.ToString();
        }
    }
}
