/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using Easy.AuditTrail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ZKEACMS.Widget;
using ZKEACMS.Zone;

namespace ZKEACMS.Common.Service
{
    public class AuditWidgetZoneValueProvider : IAuditValueProvider, IAuditWidgetZoneValueProvider
    {
        private IEnumerable<ZoneEntity> _zones;
        public int Priority => 10;
        public bool CanHandle(PropertyInfo property, Type entityType)
        {
            return property.Name.Equals(nameof(WidgetBase.ZoneId)) && typeof(WidgetBase).IsAssignableFrom(entityType);
        }

        public string GetDisplayValue(PropertyInfo property, object rawValue)
        {
            if (_zones == null) return rawValue?.ToString();

            return _zones.FirstOrDefault(z => z.HeadingCode.Equals(rawValue?.ToString()))?.ZoneName ?? rawValue?.ToString();
        }

        public void SetZones(IEnumerable<ZoneEntity> zones)
        {
            _zones = zones;
        }
    }
}
