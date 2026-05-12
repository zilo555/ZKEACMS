/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using Easy.AuditTrail.Attributes;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Easy.Models
{
    public class EditorEntity
    {
        [AuditTitle]
        public virtual string Title { get; set; }

        public virtual string Description { get; set; }

        [AuditRecordStatus]
        public virtual int? Status { get; set; }
        [AuditIgnore]
        public virtual string CreateBy { get; set; }
        [AuditIgnore]
        public virtual string CreatebyName { get; set; }
        [AuditIgnore]
        public virtual DateTime? CreateDate { get; set; }
        [AuditIgnore]
        public virtual string LastUpdateBy { get; set; }
        [AuditIgnore]
        public virtual string LastUpdateByName { get; set; }
        [AuditIgnore]
        public virtual DateTime? LastUpdateDate { get; set; }

        [NotMapped, AuditIgnore]
        public virtual Constant.ActionType? ActionType { get; set; }
    }

}
