/* http://www.zkea.net/
 * Copyright (c) ZKEASOFT. All rights reserved.
 * http://www.zkea.net/licenses */

using System;
using System.Reflection;

namespace Easy.AuditTrail
{
    public interface IAuditValueProvider : IAuditPropertyProvider
    {
        string GetDisplayValue(PropertyInfo property, object rawValue);
    }
}