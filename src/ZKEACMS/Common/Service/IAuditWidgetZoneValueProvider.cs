/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZKEACMS.Zone;

namespace ZKEACMS.Common.Service
{
    public interface IAuditWidgetZoneValueProvider
    {
        void SetZones(IEnumerable<ZoneEntity> zones);
    }
}
