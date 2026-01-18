/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using Easy;
using Easy.AuditTrail;
using Easy.AuditTrail.Attributes;
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
    public class AuditTrailService : IAuditTrailService
    {
        private readonly IAuditTrailData _auditTrailData;
        private readonly IApplicationContext _applicationContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEnumerable<IAuditValueProvider> _auditTrailValueProviders;

        public AuditTrailService(
            IAuditTrailData auditTrailData,
            IApplicationContext applicationContext,
            IHttpContextAccessor httpContextAccessor,
            IEnumerable<IAuditValueProvider> auditTrailValueProviders)
        {
            _auditTrailData = auditTrailData;
            _applicationContext = applicationContext;
            _httpContextAccessor = httpContextAccessor;
            _auditTrailValueProviders = auditTrailValueProviders;
        }

        #region Record data changes

        public void LogCreate<TEntity>(TEntity entity, string remark = null) where TEntity : class
        {
            if (entity == null) return;

            var entityType = typeof(TEntity);
            if (ShouldIgnoreAudit(entityType)) return;

            var record = CreateRecord("Create", entityType, entity, remark);

            // Create operation: only record the title as Changes
            var (titleProperty, titleValue) = GetEntityTitlePropertyAndValue(entity);
            var changes = new List<FieldChange>
            {
                new FieldChange
                {
                    Field = titleProperty?.Name ?? "Title",
                    OldValue = null,
                    NewValue = titleValue
                }
            };

            record.Changes = JsonSerializer.Serialize(changes);

            _auditTrailData.Save(record);
        }

        public void LogUpdate<TEntity>(TEntity oldEntity, TEntity newEntity, string remark = null) where TEntity : class
        {
            if (oldEntity == null || newEntity == null) return;

            var entityType = typeof(TEntity);
            if (ShouldIgnoreAudit(entityType)) return;

            var changes = EntityComparer.Compare(oldEntity, newEntity, _auditTrailValueProviders);
            if (!changes.Any()) return; // Don't record if no changes

            var record = CreateRecord("Update", entityType, newEntity, remark);
            record.Changes = JsonSerializer.Serialize(changes);

            _auditTrailData.Save(record);
        }

        public void LogDelete<TEntity>(TEntity entity, string remark = null) where TEntity : class
        {
            if (entity == null) return;

            var entityType = typeof(TEntity);
            if (ShouldIgnoreAudit(entityType)) return;

            var record = CreateRecord("Delete", entityType, entity, remark);

            // Delete operation: only record the title as Changes
            var (titleProperty, titleValue) = GetEntityTitlePropertyAndValue(entity);
            var changes = new List<FieldChange>
            {
                new FieldChange
                {
                    Field = titleProperty?.Name ?? "Title",
                    OldValue = titleValue,
                    NewValue = null
                }
            };

            record.Changes = JsonSerializer.Serialize(changes);

            _auditTrailData.Save(record);
        }

        #endregion

        #region Query audit records

        public IList<AuditTrailRecord> GetByEntity(string entityType, string entityID)
        {
            return _auditTrailData.GetByEntity(entityType, entityID).ToList();
        }

        public IList<AuditTrailRecord> GetByEntity<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity == null) return new List<AuditTrailRecord>();

            var entityType = typeof(TEntity);
            var entityID = GetEntityID(entity);

            return GetByEntity(entityType.FullName, entityID);
        }

        public IList<AuditTrailRecord> GetByUser(string userID, DateTime? startTime = null, DateTime? endTime = null, int limit = 1000)
        {
            return _auditTrailData.GetByUser(userID, startTime, endTime, limit).ToList();
        }

        #endregion

        #region Data maintenance

        public int CleanUp(DateTime beforeDate)
        {
            return _auditTrailData.CleanUp(beforeDate);
        }

        public async Task<int> CleanUpAsync(DateTime beforeDate)
        {
            return await Task.Run(() => CleanUp(beforeDate));
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Create audit record
        /// </summary>
        private AuditTrailRecord CreateRecord<TEntity>(string operationType, Type entityType, TEntity entity, string remark)
        {
            var currentUser = _applicationContext.CurrentUser;
            var httpContext = _httpContextAccessor.HttpContext;

            return new AuditTrailRecord
            {
                ID = DateTime.Now.Ticks,
                UserID = currentUser?.UserID,
                UserName = currentUser?.UserName,
                OperationTime = DateTime.Now,
                OperationType = operationType,
                EntityType = entityType.FullName,
                EntityID = GetEntityID(entity),
                EntityTitle = GetEntityTitle(entity),
                IPAddress = httpContext?.Connection?.RemoteIpAddress?.ToString(),
                Remark = remark
            };
        }

        /// <summary>
        /// Get entity ID
        /// </summary>
        private string GetEntityID<TEntity>(TEntity entity)
        {
            if (entity == null) return null;

            var keyProperty = entity.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null);

            if (keyProperty != null)
            {
                var keyValue = keyProperty.GetValue(entity);
                return keyValue?.ToString();
            }

            // If no Key attribute, try to find properties named ID or Id
            var idProperty = entity.GetType().GetProperty("ID") ?? entity.GetType().GetProperty("Id");
            if (idProperty != null)
            {
                var idValue = idProperty.GetValue(entity);
                return idValue?.ToString();
            }

            return null;
        }

        /// <summary>
        /// Get entity title property and value
        /// </summary>
        private (PropertyInfo Property, string Value) GetEntityTitlePropertyAndValue<TEntity>(TEntity entity)
        {
            if (entity == null) return (null, null);

            var entityType = entity.GetType();

            // First, look for properties marked with AuditTitleAttribute
            var auditTitleProperty = entityType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(p => p.GetCustomAttribute<AuditTitleAttribute>() != null);

            if (auditTitleProperty != null && auditTitleProperty.PropertyType == typeof(string))
            {
                var value = auditTitleProperty.GetValue(entity) as string;
                if (!string.IsNullOrEmpty(value))
                {
                    return (auditTitleProperty, value.Length > 500 ? value.Substring(0, 500) : value);
                }
            }

            // If no attribute found, try common title properties
            var titleProperties = new[] { "Title", "Name", "DisplayName", "Description" };
            foreach (var propName in titleProperties)
            {
                var property = entityType.GetProperty(propName);
                if (property != null && property.PropertyType == typeof(string))
                {
                    var value = property.GetValue(entity) as string;
                    if (!string.IsNullOrEmpty(value))
                    {
                        return (property, value.Length > 500 ? value.Substring(0, 500) : value);
                    }
                }
            }

            // If still no value found, but one of the common title properties exists, return the property but with an empty value
            foreach (var propName in titleProperties)
            {
                var property = entityType.GetProperty(propName);
                if (property != null && property.PropertyType == typeof(string))
                {
                    var value = property.GetValue(entity) as string;
                    return (property, value?.Length > 500 ? value.Substring(0, 500) : value);
                }
            }

            return (null, null);
        }

        /// <summary>
        /// Get entity title
        /// </summary>
        private string GetEntityTitle<TEntity>(TEntity entity)
        {
            return GetEntityTitlePropertyAndValue(entity).Value;
        }

        /// <summary>
        /// Determine whether audit should be ignored
        /// </summary>
        private bool ShouldIgnoreAudit(Type entityType)
        {
            return entityType.GetCustomAttribute<IgnoreAuditAttribute>() != null;
        }

        /// <summary>
        /// Get list of properties to audit
        /// </summary>
        private IEnumerable<PropertyInfo> GetAuditProperties(Type entityType)
        {
            return entityType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && !p.GetCustomAttributes<IgnoreAuditAttribute>().Any());
        }
        #endregion
    }
}