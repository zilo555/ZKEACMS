/* http://www.zkea.net/
 * Copyright (c) ZKEASOFT. All rights reserved.
 * http://www.zkea.net/licenses */

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Easy.AuditTrail
{
    public interface IAuditFieldValueProvider : IAuditPropertyProvider
    {
        IEnumerable<AuditField> GetFields(PropertyInfo property, object rawValue);
    }
}