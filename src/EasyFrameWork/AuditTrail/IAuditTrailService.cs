/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Easy.AuditTrail
{
    /// <summary>
    /// 审计跟踪服务接口 - 记录数据的增删改操作
    /// </summary>
    public interface IAuditTrailService
    {
        #region 记录数据变化

        /// <summary>
        /// 记录创建操作
        /// </summary>
        void LogCreate<TEntity>(TEntity entity, string remark = null) where TEntity : class;

        /// <summary>
        /// 记录更新操作
        /// </summary>
        void LogUpdate<TEntity>(TEntity oldEntity, TEntity newEntity, string remark = null) where TEntity : class;

        /// <summary>
        /// 记录删除操作
        /// </summary>
        void LogDelete<TEntity>(TEntity entity, string remark = null) where TEntity : class;

        #endregion

        #region 查询审计记录

        /// <summary>
        /// 根据实体类型和ID获取审计记录
        /// </summary>
        IList<AuditTrailRecord> GetByEntity(string entityType, string entityID);

        /// <summary>
        /// 根据实体对象获取审计记录
        /// </summary>
        IList<AuditTrailRecord> GetByEntity<TEntity>(TEntity entity) where TEntity : class;

        /// <summary>
        /// 根据用户ID获取操作记录
        /// </summary>
        IList<AuditTrailRecord> GetByUser(string userID, DateTime? startTime = null, DateTime? endTime = null, int limit = 1000);

        #endregion

        #region 数据维护

        /// <summary>
        /// 清理指定日期之前的审计记录
        /// </summary>
        int CleanUp(DateTime beforeDate);

        /// <summary>
        /// 清理指定日期之前的审计记录（异步）
        /// </summary>
        Task<int> CleanUpAsync(DateTime beforeDate);

        #endregion
    }
}
