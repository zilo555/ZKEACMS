/* http://www.zkea.net/
 * Copyright (c) ZKEASOFT. All rights reserved.
 * http://www.zkea.net/licenses */

using System;
using System.Reflection;

namespace Easy.AuditTrail
{
    /// <summary>
    /// Provides custom display values and names for audit logging
    /// </summary>
    public interface IAuditDisplayProvider : IAuditPropertyProvider
    {
        string GetDisplayName(PropertyInfo property, Type entityType);
    }
}