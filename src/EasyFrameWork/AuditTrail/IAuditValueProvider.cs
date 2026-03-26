/* http://www.zkea.net/
 * Copyright (c) ZKEASOFT. All rights reserved.
 * http://www.zkea.net/licenses */

using System;
using System.Reflection;

namespace Easy.AuditTrail
{    
    /// <summary>
    /// Provides custom display values for audit logging
    /// </summary>
    public interface IAuditValueProvider
    {
        int Priority { get; }
        /// <summary>
        /// Gets the display value for a specific property
        /// </summary>
        /// <param name="property">The property info</param>
        /// <param name="rawValue">The raw value from the entity</param>
        /// <returns>The display-friendly value</returns>
        string GetDisplayValue(PropertyInfo property, object rawValue);
        
        /// <summary>
        /// Determines if this provider can handle a specific property
        /// </summary>
        /// <param name="property">The property info</param>
        /// <param name="entityType">The type of the entity</param>
        /// <param name="operationType">The type of operation to handle</param>
        /// <returns>True if this provider can handle the property, otherwise false</returns>
        bool CanHandle(PropertyInfo property, Type entityType);
    }
}