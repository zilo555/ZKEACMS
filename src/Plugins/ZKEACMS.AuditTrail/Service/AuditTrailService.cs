/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using AngleSharp.Dom;
using Easy;
using Easy.AuditTrail;
using Easy.AuditTrail.Attributes;
using Easy.Extend;
using Easy.RepositoryPattern;
using Easy.Serializer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace ZKEACMS.AuditTrail.Service
{
    /// <summary>
    /// Audit trail service implementation
    /// </summary>
    public class AuditTrailService : ServiceBase<AuditTrailRecord>, IAuditTrailService
    {
        private readonly IApplicationContext _applicationContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditTrailService(
            IApplicationContext applicationContext,
            CMSDbContext dbContext,
            IHttpContextAccessor httpContextAccessor)
            : base(applicationContext, dbContext)
        {
            _applicationContext = applicationContext;
            _httpContextAccessor = httpContextAccessor;
        }

        #region Record data changes

        public void AuditCreate<T>(string id, string field, string newValue, string remark = null) where T : class
        {
            if (id == null) return;

            var entityType = typeof(T);
            if (ShouldIgnoreAudit(entityType)) return;

            var record = CreateRecord(entityType, id, remark);

            var changes = new List<FieldChange>
            {
                new FieldChange
                {
                    Field = field,
                    ChangeType= (int)AuditChangeType.Added,
                    NewValue = newValue
                }
            };

            record.Changes = JsonConverter.Serialize(changes);

            Add(record);
        }
        public void AuditCreate<TEntity>(TEntity entity, string remark = null) where TEntity : class
        {
            if (entity == null) return;

            var entityType = typeof(TEntity);
            if (ShouldIgnoreAudit(entityType)) return;

            var record = CreateRecord(entityType, entity, remark);

            var changes = new List<FieldChange>
            {
                new FieldChange
                {
                    ChangeType = (int)AuditChangeType.Added,
                    NewValue = EntityComparer.GetKeyAndTitle<TEntity>(entity)
                }
            };

            record.Changes = JsonConverter.Serialize(changes);

            Add(record);
        }

        public void AuditUpdate<TEntity>(TEntity oldEntity, TEntity newEntity, string remark = null) where TEntity : class
        {
            if (oldEntity == null || newEntity == null) return;

            var entityType = typeof(TEntity);
            if (ShouldIgnoreAudit(entityType)) return;

            var changes = EntityComparer.Compare(oldEntity, newEntity, _applicationContext.ServiceProvider.GetServices<IAuditValueProvider>());
            if (!changes.Any()) return; // Don't record if no changes

            var record = CreateRecord(entityType, newEntity, remark);
            for (int i = 0; i < changes.Count; i++)
            {
                changes[i].Sequence = i;
            }
            record.Changes = JsonConverter.Serialize(changes);
            Add(record);
        }

        public void AuditDelete<TEntity>(TEntity entity, string remark = null) where TEntity : class
        {
            if (entity == null) return;

            var entityType = typeof(TEntity);
            if (ShouldIgnoreAudit(entityType)) return;

            var record = CreateRecord(entityType, entity, remark);

            var changes = new List<FieldChange>
            {
                new FieldChange
                {
                    ChangeType = (int)AuditChangeType.Deleted,
                    OldValue = EntityComparer.GetKeyAndTitle<TEntity>(entity)
                }
            };

            record.Changes = JsonConverter.Serialize(changes);
            Add(record);
        }
        public void AuditDelete<T>(string id, string field, string oldValue, string remark = null) where T : class
        {
            if (id == null) return;

            var entityType = typeof(T);
            if (ShouldIgnoreAudit(entityType)) return;

            var record = CreateRecord(entityType, id, remark);

            var changes = new List<FieldChange>
            {
                new FieldChange
                {
                    Field = field,
                    ChangeType = (int)AuditChangeType.Deleted,
                    OldValue = oldValue
                }
            };

            record.Changes = JsonConverter.Serialize(changes);

            Add(record);
        }
        #endregion

        #region Query audit records

        public IList<AuditTrailRecord> GetByEntity(string entityType, string entityID, Pagination pagination)
        {
            pagination.OrderByDescending = nameof(AuditTrailRecord.ID);
            return Get(m => m.EntityType == entityType && m.EntityID == entityID, pagination);
        }

        private bool ShouldIgnoreAudit(Type entityType)
        {
            return entityType.GetCustomAttribute<AuditIgnoreAttribute>() != null;
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
                EntityType = WebEncoders.Base64UrlEncode(entityType.FullName.ToByte()),
                EntityID = GetEntityID(entity),
                IPAddress = httpContext?.Connection?.RemoteIpAddress?.ToString(),
                Description = remark
            };
        }
        private AuditTrailRecord CreateRecord(Type entityType, string entityId, string remark)
        {
            var currentUser = _applicationContext.CurrentUser;
            var httpContext = _httpContextAccessor.HttpContext;


            return new AuditTrailRecord
            {
                EntityType = WebEncoders.Base64UrlEncode(entityType.FullName.ToByte()),
                EntityID = entityId,
                IPAddress = httpContext?.Connection?.RemoteIpAddress?.ToString(),
                Description = remark
            };
        }

        #endregion
    }
}