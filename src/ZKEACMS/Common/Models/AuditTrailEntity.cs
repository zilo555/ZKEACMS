/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using Easy.MetaData;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Easy.RepositoryPattern;

namespace ZKEACMS.Common.Models
{
    public class AuditTrailEntity
    {
        public long ID { get; set; }
        public string UserID { get; set; }
        public string UserName { get; set; }
        public DateTime OperationTime { get; set; }

        ///Create/Update/Delete
        public string OperationType { get; set; }
        public string EntityType { get; set; }

        public string EntityID { get; set; }

        public string EntityTitle { get; set; }

        /// <summary>
        /// 变更详情（JSON格式数组）
        /// 格式：[{"Field":"字段名","OldValue":"旧值","NewValue":"新值"}]
        /// </summary>
        public string Changes { get; set; }

        /// <summary>
        /// 变更的字段名列表（逗号分隔，用于快速查询）
        /// </summary>
        public string ChangedFields { get; set; }

        public string IPAddress { get; set; }
        
        public string Remark { get; set; }
    }
}
