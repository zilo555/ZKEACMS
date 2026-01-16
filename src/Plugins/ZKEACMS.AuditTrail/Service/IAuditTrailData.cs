/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using System;
using System.Collections.Generic;
using ZKEACMS.Common.Models;

namespace ZKEACMS.AuditTrail.Service
{
    /// <summary>
    /// 审计跟踪数据接口
    /// </summary>
    public interface IAuditTrailData
    {
        /// <summary>
        /// 保存审计记录
        /// </summary>
        void Save(AuditTrailRecord record);

        /// <summary>
        /// 根据实体查询审计记录
        /// </summary>
        IEnumerable<AuditTrailRecord> GetByEntity(string entityType, string entityID);

        /// <summary>
        /// 根据用户查询审计记录
        /// </summary>
        IEnumerable<AuditTrailRecord> GetByUser(string userID, DateTime? startTime = null, DateTime? endTime = null, int limit = 1000);

        /// <summary>
        /// 清理指定日期之前的记录
        /// </summary>
        int CleanUp(DateTime beforeDate);
    }
}
