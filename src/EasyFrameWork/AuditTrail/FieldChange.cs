using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Easy.AuditTrail
{
    public class FieldChange
    {
        public int Sequence { get; set; }
        public string Field { get; set; }
        public int? ValueType { get; set; }
        public int? ChangeType { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
    public enum AuditChangeType
    {
        Added = 1,
        Updated = 2,
        Deleted = 3
    }
    public enum AuditValueType
    {
        String = 1,
        Integer = 2,
        Decimal = 3,
        DateTime = 4,
        Boolean = 5,
        Guid = 6,
        Html = 7
    }
}
