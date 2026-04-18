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
            var changes = new List<FieldChange>
            {
                new FieldChange
                {
                    Field = field,
                    ChangeType = (int)AuditChangeType.Added,
                    NewValue = newValue
                }
            };

            SaveAuditRecord(entityType, id, changes, remark);
        }

        public void AuditCreate<TEntity>(TEntity entity, string remark = null) where TEntity : class
        {
            if (entity == null) return;

            var entityType = typeof(TEntity);
            var changes = new List<FieldChange>
            {
                new FieldChange
                {
                    ChangeType = (int)AuditChangeType.Added,
                    NewValue = EntityComparer.GetKeyAndTitle<TEntity>(entity)
                }
            };

            SaveAuditRecord(entityType, entity, changes, remark);
        }

        public void AuditUpdate<TEntity>(TEntity oldEntity, TEntity newEntity, string remark = null) where TEntity : class
        {
            if (oldEntity == null || newEntity == null) return;

            var entityType = typeof(TEntity);
            var changes = EntityComparer.Compare(oldEntity, newEntity, GetAuditValueProviders());
            if (!changes.Any()) return;

            SaveAuditRecord(entityType, newEntity, changes, remark);
        }

        public void AuditUpdate(Type entityType, object oldEntity, object newEntity, string remark = null)
        {
            if (oldEntity == null || newEntity == null) return;

            var changes = EntityComparer.Compare(oldEntity, newEntity, GetAuditValueProviders());
            if (!changes.Any()) return;

            SaveAuditRecord(entityType, newEntity, changes, remark);
        }


        public void AuditUpdate<T>(string id, string field, object oldValue, object newValue, string remark = null) where T : class
        {
            if (oldValue == null || newValue == null || oldValue == newValue) return;

            var entityType = typeof(T);
            var changes = EntityComparer.Compare(oldValue, newValue, GetAuditValueProviders());
            if (!changes.Any()) return;

            for (int i = 0; i < changes.Count; i++)
            {
                changes[i].Field = field;
            }
            SaveAuditRecord(entityType, id, changes, remark);
        }

        public void AuditDelete<TEntity>(TEntity entity, string remark = null) where TEntity : class
        {
            if (entity == null) return;

            var entityType = typeof(TEntity);
            var changes = new List<FieldChange>
            {
                new FieldChange
                {
                    ChangeType = (int)AuditChangeType.Deleted,
                    OldValue = EntityComparer.GetKeyAndTitle<TEntity>(entity)
                }
            };

            SaveAuditRecord(entityType, entity, changes, remark);
        }

        public void AuditDelete<T>(string id, string field, string oldValue, string remark = null) where T : class
        {
            if (id == null) return;

            var entityType = typeof(T);
            var changes = new List<FieldChange>
            {
                new FieldChange
                {
                    Field = field,
                    ChangeType = (int)AuditChangeType.Deleted,
                    OldValue = oldValue
                }
            };

            SaveAuditRecord(entityType, id, changes, remark);
        }

        private void SaveAuditRecord(Type entityType, string entityId, List<FieldChange> changes, string remark)
        {
            if (ShouldIgnoreAudit(entityType)) return;

            var record = CreateRecord(entityType, entityId, remark);
            for (int i = 0; i < changes.Count; i++)
            {
                changes[i].Sequence = i;
            }
            record.Changes = JsonConverter.Serialize(changes);
            Add(record);
        }

        private void SaveAuditRecord<TEntity>(Type entityType, TEntity entity, List<FieldChange> changes, string remark) where TEntity : class
        {
            if (ShouldIgnoreAudit(entityType)) return;

            var record = CreateRecord(entityType, entity, remark);
            for (int i = 0; i < changes.Count; i++)
            {
                changes[i].Sequence = i;
            }
            record.Changes = JsonConverter.Serialize(changes);
            Add(record);
        }

        private List<IAuditPropertyProvider> GetAuditValueProviders()
        {
            return _applicationContext.ServiceProvider.GetServices<IAuditPropertyProvider>()
                .OrderByDescending(m => m.Priority)
                .ToList();
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

        private AuditTrailRecord CreateRecord(Type entityType, string entityId, string remark)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            return new AuditTrailRecord
            {
                EntityType = WebEncoders.Base64UrlEncode(entityType.FullName.ToByte()),
                EntityID = entityId,
                IPAddress = httpContext?.Connection?.RemoteIpAddress?.ToString(),
                Description = remark
            };
        }

        private AuditTrailRecord CreateRecord<TEntity>(Type entityType, TEntity entity, string remark)
        {
            return CreateRecord(entityType, GetEntityID(entity), remark);
        }

        #endregion
    }
}