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

namespace ZKEACMS.Common.Service
{
    public class AuditRecordStatusValueProvider : IAuditValueProvider
    {
        private readonly IDataDictionaryService _dataDictionaryService;
        private readonly ILocalize _localize;
        private readonly CultureOption _cultureOption;
        private Dictionary<string, string> _recordStatusDic;

        public AuditRecordStatusValueProvider(IDataDictionaryService dataDictionaryService, ILocalize localize, IOptions<CultureOption> cultureOption)
        {
            _dataDictionaryService = dataDictionaryService;
            _localize = localize;
            _cultureOption = cultureOption.Value;
        }
        public int Priority => 10;
        public bool CanHandle(PropertyInfo property, Type entityType)
        {
            return property.GetCustomAttribute<AuditRecordStatusAttribute>(false) != null;
        }

        public string GetDisplayValue(PropertyInfo property, object rawValue)
        {
            if (_recordStatusDic == null)
            {
                _recordStatusDic = _dataDictionaryService.Get(m => m.DicName == DicKeys.RecordStatus)
                    .ToDictionary(m => m.DicValue, m => _localize.Get(m.Title, _cultureOption.Code));
            }

            var key = rawValue?.ToString();
            if (key != null && _recordStatusDic.TryGetValue(key, out string value))
            {
                return value;
            }
            return key ?? string.Empty;
        }
    }
}
