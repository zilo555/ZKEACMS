/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using System;

namespace ZKEACMS.Common.Models.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class AuditKeyAttribute : Attribute
    {
        public int Order { get; set; } = 0;
    }
}