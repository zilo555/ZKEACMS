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
    internal class SectionContentAuditDisplayProvider : IAuditDisplayProvider
    {
        private readonly ILocalize _localize;        
        private readonly CultureOption _culture;

        public SectionContentAuditDisplayProvider(ILocalize localize, IOptions<CultureOption> culture)
        {
            _localize = localize;
            _culture = culture.Value;
        }

        public int Priority => 20;

        public bool CanHandle(PropertyInfo property, Type entityType)
        {
            return typeof(SectionContent).IsAssignableFrom(entityType);
        }

        public string GetDisplayName(PropertyInfo property, Type entityType)
        {
            var key = $"SectionContent@{property.Name}";
            return _localize.GetOrNull(key, _culture.Code);
        }

        public string GetDisplayValue(PropertyInfo property, object rawValue)
        {
            throw new NotImplementedException();
        }
    }
}
