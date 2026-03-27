/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using Easy.AuditTrail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ZKEACMS.FormGenerator.Models;

namespace ZKEACMS.FormGenerator.Service
{
    internal class FormAduitValueProvider : IAuditValueProvider
    {
        private readonly IFormService _formService;

        public FormAduitValueProvider(IFormService formService)
        {
            _formService = formService;
        }

        public int Priority => 10;

        public bool CanHandle(PropertyInfo property, Type entityType)
        {
            return property.Name == nameof(FormWidget.FormID) && entityType == typeof(FormWidget);
        }

        public string GetDisplayValue(PropertyInfo property, object rawValue)
        {
            if (rawValue is string formId)
            {
                var form = _formService.Get(formId);
                return form?.Title ?? rawValue.ToString();
            }
            return rawValue?.ToString() ?? string.Empty;
        }
    }
}
