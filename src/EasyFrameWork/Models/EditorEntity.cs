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
        public virtual string Title { get; set; }

        public virtual string Description { get; set; }

        [AuditRecordStatus]
        public virtual int? Status { get; set; }

        public virtual string CreateBy { get; set; }

        public virtual string CreatebyName { get; set; }

        public virtual DateTime? CreateDate { get; set; }

        public virtual string LastUpdateBy { get; set; }

        public virtual string LastUpdateByName { get; set; }

        public virtual DateTime? LastUpdateDate { get; set; }

        [NotMapped, AuditIgnore]
        public virtual Constant.ActionType? ActionType { get; set; }
    }

}
