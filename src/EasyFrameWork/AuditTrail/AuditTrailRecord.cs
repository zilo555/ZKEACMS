/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using System;

namespace Easy.AuditTrail
{
    /// <summary>
    /// 审计跟踪记录
    /// </summary>
    public class AuditTrailRecord
    {
        public long ID { get; set; }
        public string UserID { get; set; }
        public string UserName { get; set; }
        public DateTime OperationTime { get; set; }
        public string OperationType { get; set; }
        public string EntityType { get; set; }
        public string EntityID { get; set; }
        public string EntityTitle { get; set; }
        public string Changes { get; set; }
        public string IPAddress { get; set; }
        public string Remark { get; set; }
    }

    /// <summary>
    /// 字段变更详情
    /// </summary>
    public class FieldChange
    {
        public string Field { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
}
