/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using Easy;
using Easy.AuditTrail;
using Easy.Options;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ZKEACMS.SectionWidget.Models;

namespace ZKEACMS.SectionWidget.Service
{
    internal class SectionGroupTemplateAuditValueProvider : IAuditValueProvider
    {
        private readonly ISectionTemplateService _sectionTemplateService;

        public SectionGroupTemplateAuditValueProvider(ISectionTemplateService sectionTemplateService)
        {
            _sectionTemplateService = sectionTemplateService;
        }

        public int Priority => 10;

        public bool CanHandle(PropertyInfo property, Type entityType)
        {
            return typeof(SectionGroup).IsAssignableFrom(entityType) &&
                nameof(SectionGroup.PartialView) == property.Name;
        }

        public string GetDisplayValue(PropertyInfo property, object rawValue)
        {
            if(rawValue is string templateName)
            {
                return _sectionTemplateService.Get(templateName)?.Title;
            }
            return rawValue?.ToString();
        }
    }
}
