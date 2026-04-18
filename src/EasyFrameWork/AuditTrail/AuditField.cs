using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Easy.AuditTrail
{
    public sealed class AuditField
    {
        public string FieldName { get; set; }

        public string DisplayName { get; set; }

        public object Value { get; set; }

        public int Order { get; set; }
    }
}
