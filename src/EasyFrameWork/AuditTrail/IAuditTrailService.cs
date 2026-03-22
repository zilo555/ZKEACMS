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
        void AuditCreate<T>(string id, string field, string newValue, string remark = null) where T : class;
        void AuditCreate<TEntity>(TEntity entity, string remark = null) where TEntity : class;
        void AuditUpdate<TEntity>(TEntity oldEntity, TEntity newEntity, string remark = null) where TEntity : class;
        void AuditDelete<TEntity>(TEntity entity, string remark = null) where TEntity : class;
        void AuditDelete<T>(string id, string field, string oldValue, string remark = null) where T : class;
        IList<AuditTrailRecord> GetByEntity(string entityType, string entityID, Pagination pagination);
    }
}
