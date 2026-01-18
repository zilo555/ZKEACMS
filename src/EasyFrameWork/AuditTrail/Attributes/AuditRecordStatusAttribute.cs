using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Easy.AuditTrail.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class AuditRecordStatusAttribute : Attribute
    {
    }
}
