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
using ZKEACMS.Common.Models;

namespace ZKEACMS.Common.Service
{
    public class AuditNavigationParentValueProvider : IAuditValueProvider
    {
        private readonly INavigationService _navigationService;

        public AuditNavigationParentValueProvider(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public bool CanHandle(PropertyInfo property, Type entityType)
        {
            return entityType == typeof(NavigationEntity) && property.Name == nameof(NavigationEntity.ParentId);
        }

        public string GetDisplayValue(PropertyInfo property, object rawValue)
        {
            if (rawValue == null)
            {
                return string.Empty;
            }

            return _navigationService.Get(rawValue.ToString())?.Title;
        }
    }
}
