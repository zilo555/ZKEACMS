/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using Easy;
using Easy.AuditTrail;
using Easy.AuditTrail.Attributes;
using Easy.Constant;
using Easy.Modules.DataDictionary;
using Easy.Options;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ZKEACMS.Rule;
using ZKEACMS.Widget;

namespace ZKEACMS.Common.Service
{
    public class AuditWidgetRuleValueProvider : IAuditValueProvider
    {
        private readonly IRuleService _ruleService;
        public int Priority => 10;
        public bool CanHandle(PropertyInfo property, Type entityType)
        {
            return property.Name.Equals(nameof(WidgetBase.RuleID)) && typeof(WidgetBase).IsAssignableFrom(entityType);
        }

        public string GetDisplayValue(PropertyInfo property, object rawValue)
        {
            if (rawValue == null) return string.Empty;

            return _ruleService.Get(rawValue.ToString())?.Title ?? rawValue.ToString();
        }
    }
}
