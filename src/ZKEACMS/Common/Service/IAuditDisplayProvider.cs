/* http://www.zkea.net/
 * Copyright (c) ZKEASOFT. All rights reserved.
 * http://www.zkea.net/licenses */

using System;
using System.Reflection;

namespace ZKEACMS.Common.Service
{
    /// <summary>
    /// Provides custom display values and names for audit logging
    /// </summary>
    public interface IAuditDisplayProvider : IAuditValueProvider
    {
        /// <summary>
        /// Gets the display name for a specific property
        /// </summary>
        /// <param name="property">The property info</param>
        /// <param name="entityType">The type of the entity</param>
        /// <returns>The display-friendly name of the property</returns>
        string GetDisplayName(PropertyInfo property, Type entityType);
    }
}