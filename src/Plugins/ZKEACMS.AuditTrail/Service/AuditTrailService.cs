/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using Easy;
using Easy.AuditTrail;
using Easy.AuditTrail.Attributes;
using Easy.RepositoryPattern;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using ZKEACMS.Common.Models;
using ZKEACMS.Common.Service;

namespace ZKEACMS.AuditTrail.Service
{
    /// <summary>
    /// Audit trail service implementation
    /// </summary>
    public class AuditTrailService : ServiceBase<AuditTrailRecord>, IAuditTrailService
    {
        private readonly IApplicationContext _applicationContext;
        private readonly IEnumerable<IAuditValueProvider> _auditTrailValueProviders;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditTrailService(
            IApplicationContext applicationContext,
            IEnumerable<IAuditValueProvider> auditTrailValueProviders,
            CMSDbContext dbContext,
            IHttpContextAccessor httpContextAccessor)
            : base(applicationContext, dbContext)
        {
            _applicationContext = applicationContext;
            _auditTrailValueProviders = auditTrailValueProviders;
            _httpContextAccessor = httpContextAccessor;
        }

        #region Record data changes

        public void LogCreate<TEntity>(TEntity entity, string remark = null) where TEntity : class
        {
            if (entity == null) return;

            var entityType = typeof(TEntity);
            if (ShouldIgnoreAudit(entityType)) return;

            var record = CreateRecord(entityType, entity, remark);

            var changes = new List<FieldChange>
            {
                new FieldChange
                {
                    NewValue = EntityComparer.GetKeyAndTitle<TEntity>(entity)
                }
            };

            record.Changes = JsonSerializer.Serialize(changes);

            Add(record);
        }

        public void LogUpdate<TEntity>(TEntity oldEntity, TEntity newEntity, string remark = null) where TEntity : class
        {
            if (oldEntity == null || newEntity == null) return;

            var entityType = typeof(TEntity);
            if (ShouldIgnoreAudit(entityType)) return;

            var changes = EntityComparer.Compare(oldEntity, newEntity, _auditTrailValueProviders);
            if (!changes.Any()) return; // Don't record if no changes

            var record = CreateRecord(entityType, newEntity, remark);
            record.Changes = JsonSerializer.Serialize(changes);

            Add(record);
        }

        public void LogDelete<TEntity>(TEntity entity, string remark = null) where TEntity : class
        {
            if (entity == null) return;

            var entityType = typeof(TEntity);
            if (ShouldIgnoreAudit(entityType)) return;

            var record = CreateRecord(entityType, entity, remark);

            var changes = new List<FieldChange>
            {
                new FieldChange
                {
                    OldValue = EntityComparer.GetKeyAndTitle<TEntity>(entity)
                }
            };

            record.Changes = JsonSerializer.Serialize(changes);
            Add(record);
        }

        #endregion

        #region Query audit records

        public IList<AuditTrailRecord> GetByEntity(string entityType, string entityID, Pagination pagination)
        {
            return Get(m => m.EntityType == entityType && m.EntityID == entityID, pagination);
        }

        private bool ShouldIgnoreAudit(Type entityType)
        {
            return entityType.GetCustomAttribute<IgnoreAuditAttribute>() != null;
        }
        private string GetEntityID<TEntity>(TEntity entity)
        {
            if (entity == null) return null;

            var keyProperties = entity.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttribute<KeyAttribute>() != null);

            if (!keyProperties.Any())
            {
                throw new InvalidOperationException($"Entity type {entity.GetType().FullName} does not have a property marked with [Key] attribute.");
            }

            var keyValue = string.Join(":", keyProperties.Select(p => p.GetValue(entity).ToString()));
            return keyValue.ToString();
        }

        private AuditTrailRecord CreateRecord<TEntity>(Type entityType, TEntity entity, string remark)
        {
            var currentUser = _applicationContext.CurrentUser;
            var httpContext = _httpContextAccessor.HttpContext;

            return new AuditTrailRecord
            {
                EntityType = entityType.FullName,
                EntityID = GetEntityID(entity),
                IPAddress = httpContext?.Connection?.RemoteIpAddress?.ToString(),
                Description = remark
            };
        }
        #endregion
    }
}