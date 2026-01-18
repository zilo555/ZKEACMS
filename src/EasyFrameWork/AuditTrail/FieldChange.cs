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
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
}
