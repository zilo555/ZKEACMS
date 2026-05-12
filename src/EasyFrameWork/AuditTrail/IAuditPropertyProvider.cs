/* http://www.zkea.net/
 * Copyright (c) ZKEASOFT. All rights reserved.
 * http://www.zkea.net/licenses */

using System;
using System.Reflection;

namespace Easy.AuditTrail
{
    public interface IAuditPropertyProvider
    {
        int Priority { get; }
        bool CanHandle(PropertyInfo property, Type entityType);
    }
}