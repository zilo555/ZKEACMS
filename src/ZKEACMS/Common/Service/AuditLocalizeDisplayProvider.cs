using Easy;
using Easy.Options;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ZKEACMS.Common.Service
{
    public class AuditLocalizeDisplayProvider : IAuditDisplayProvider
    {
        private readonly ILocalize _localize;
        private readonly CultureOption _culture;

        public AuditLocalizeDisplayProvider(ILocalize localize, IOptions<CultureOption> culture)
        {
            _localize = localize;
            _culture = culture.Value;
        }

        public bool CanHandle(PropertyInfo property, Type entityType, AuditOperationType operationType)
        {
            return operationType == AuditOperationType.GetName;
        }

        public string GetDisplayName(PropertyInfo property, Type entityType)
        {
            var key = $"{entityType.Name}@{property.Name}";
            var local = _localize.GetOrNull(key, _culture.Code);
            return local ?? _localize.Get(property.Name);
        }

        public string GetDisplayValue(PropertyInfo property, object rawValue)
        {
            throw new NotImplementedException();
        }
    }
}
