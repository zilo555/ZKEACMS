/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using Easy.RepositoryPattern;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Easy.AuditTrail
{
    public interface IAuditTrailService
    {
        void LogCreate<TEntity>(TEntity entity, string remark = null) where TEntity : class;
        void LogUpdate<TEntity>(TEntity oldEntity, TEntity newEntity, string remark = null) where TEntity : class;
        void LogDelete<TEntity>(TEntity entity, string remark = null) where TEntity : class;
        IList<AuditTrailRecord> GetByEntity(string entityType, string entityID, Pagination pagination);
    }
}
