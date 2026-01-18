using Easy.Constant;
using Easy.Modules.DataDictionary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ZKEACMS.Common.Attributes;

namespace ZKEACMS.Common.Service
{
    public class AuditRecordStatusValueProvider : IAuditValueProvider
    {
        private readonly IDataDictionaryService _dataDictionaryService;
        private Dictionary<string, string> _recordStatusDic;

        public AuditRecordStatusValueProvider(IDataDictionaryService dataDictionaryService)
        {
            _dataDictionaryService = dataDictionaryService;
        }

        public bool CanHandle(PropertyInfo property, Type entityType, AuditOperationType operationType)
        {
            if (operationType == AuditOperationType.GetName) return false;

            return property.GetCustomAttribute<AuditRecordStatusAttribute>() != null;
        }

        public string GetDisplayValue(PropertyInfo property, object rawValue)
        {
            if (_recordStatusDic == null)
            {
                _recordStatusDic = _dataDictionaryService.Get(m => m.DicName == DicKeys.RecordStatus).ToDictionary(m => m.DicValue, m => m.Title);
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
