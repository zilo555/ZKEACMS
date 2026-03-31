/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using Easy.AuditTrail;
using Easy.Modules.DataDictionary;
using Easy.Modules.User.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Easy.Modules.User.Service
{
    public class UserDictionaryAuditValueProvider : IAuditValueProvider
    {
        private HashSet<string> _properties = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            nameof(UserEntity.Sex),
            nameof(UserEntity.MaritalStatus),
            nameof(UserEntity.UserTypeCD)
        };
        private readonly IDataDictionaryService _dataDictionaryService;
        private readonly ILocalize _localize;

        public UserDictionaryAuditValueProvider(IDataDictionaryService dataDictionaryService, ILocalize localize)
        {
            _dataDictionaryService = dataDictionaryService;
            _localize = localize;
        }

        public int Priority => 10;

        public bool CanHandle(PropertyInfo property, Type entityType)
        {
            return typeof(UserEntity).IsAssignableTo(entityType) && _properties.Contains(property.Name);
        }

        public string GetDisplayValue(PropertyInfo property, object rawValue)
        {
            if (rawValue == null) return string.Empty;

            var title = _dataDictionaryService.
                Get(m => m.DicName == $"UserEntity@{property.Name}" && m.DicValue == rawValue.ToString())
                .FirstOrDefault().Title;

            return _localize.Get(title);
        }
    }
}
