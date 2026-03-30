using Easy;
using Easy.AuditTrail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ZKEACMS.EventAction.Service
{
    public class EventAuditValueProvider : IAuditValueProvider
    {
        private readonly ILocalize _localize;

        public EventAuditValueProvider(ILocalize localize)
        {
            _localize = localize;
        }

        public int Priority => 10;

        public bool CanHandle(PropertyInfo property, Type entityType)
        {
            return entityType == typeof(Models.EventAction) &&
                property.Name == nameof(Models.EventAction.Event);
        }

        public string GetDisplayValue(PropertyInfo property, object rawValue)
        {
            if (rawValue is string eventName && 
                Models.EventAction.EventNameValueMapping.TryGetValue(eventName, out var value))
            {
                return _localize.Get(value);
            }
            return rawValue?.ToString() ?? string.Empty;
        }
    }
}
