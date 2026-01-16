/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using Easy;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using ZKEACMS.Common.Models;
using ZKEACMS.Common.Models.Attributes;
using ZKEACMS.Common.Service;

namespace ZKEACMS.AuditTrail.Service
{
    /// <summary>
    /// 审计跟踪服务实现
    /// </summary>
    public class AuditTrailService : IAuditTrailService
    {
        private readonly IAuditTrailData _auditTrailData;
        private readonly IApplicationContext _applicationContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditTrailService(
            IAuditTrailData auditTrailData,
            IApplicationContext applicationContext,
            IHttpContextAccessor httpContextAccessor)
        {
            _auditTrailData = auditTrailData;
            _applicationContext = applicationContext;
            _httpContextAccessor = httpContextAccessor;
        }

        #region 记录数据变化

        public void LogCreate<TEntity>(TEntity entity, string remark = null) where TEntity : class
        {
            if (entity == null) return;

            var entityType = typeof(TEntity);
            if (ShouldIgnoreAudit(entityType)) return;

            var record = CreateRecord("Create", entityType, entity, remark);
            
            // 创建操作：将新数据作为 Changes
            var properties = GetAuditProperties(entityType);
            var changes = properties.Select(p => new FieldChange
            {
                Field = p.Name,
                OldValue = null,
                NewValue = EntityComparer.SerializeValue(p.GetValue(entity))
            }).ToList();

            record.Changes = JsonSerializer.Serialize(changes);
            record.ChangedFields = string.Join(",", changes.Select(c => c.Field));

            _auditTrailData.Save(record);
        }

        public void LogUpdate<TEntity>(TEntity oldEntity, TEntity newEntity, string remark = null) where TEntity : class
        {
            if (oldEntity == null || newEntity == null) return;

            var entityType = typeof(TEntity);
            if (ShouldIgnoreAudit(entityType)) return;

            var changes = EntityComparer.Compare(oldEntity, newEntity);
            if (!changes.Any()) return; // 没有变化则不记录

            var record = CreateRecord("Update", entityType, newEntity, remark);
            record.Changes = JsonSerializer.Serialize(changes);
            record.ChangedFields = string.Join(",", changes.Select(c => c.Field));

            _auditTrailData.Save(record);
        }

        public void LogDelete<TEntity>(TEntity entity, string remark = null) where TEntity : class
        {
            if (entity == null) return;

            var entityType = typeof(TEntity);
            if (ShouldIgnoreAudit(entityType)) return;

            var record = CreateRecord("Delete", entityType, entity, remark);
            
            // 删除操作：将原数据作为 Changes
            var properties = GetAuditProperties(entityType);
            var changes = properties.Select(p => new FieldChange
            {
                Field = p.Name,
                OldValue = EntityComparer.SerializeValue(p.GetValue(entity)),
                NewValue = null
            }).ToList();

            record.Changes = JsonSerializer.Serialize(changes);
            record.ChangedFields = string.Join(",", changes.Select(c => c.Field));

            _auditTrailData.Save(record);
        }

        #endregion

        #region 查询审计记录

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

        #region 数据维护

        public int CleanUp(DateTime beforeDate)
        {
            return _auditTrailData.CleanUp(beforeDate);
        }

        public async Task<int> CleanUpAsync(DateTime beforeDate)
        {
            return await Task.Run(() => CleanUp(beforeDate));
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 创建审计记录
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
        /// 获取实体ID
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

            // 如果没有 Key 特性，尝试查找名为 ID 或 Id 的属性
            var idProperty = entity.GetType().GetProperty("ID") ?? entity.GetType().GetProperty("Id");
            if (idProperty != null)
            {
                var idValue = idProperty.GetValue(entity);
                return idValue?.ToString();
            }

            return null;
        }

        /// <summary>
        /// 获取实体标题
        /// </summary>
        private string GetEntityTitle<TEntity>(TEntity entity)
        {
            if (entity == null) return null;

            // 优先查找带有 AuditTitleAttribute 标记的属性
            var auditTitleProperty = entity.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(p => p.GetCustomAttribute<AuditTitleAttribute>() != null);

            if (auditTitleProperty != null && auditTitleProperty.PropertyType == typeof(string))
            {
                var value = auditTitleProperty.GetValue(entity) as string;
                if (!string.IsNullOrEmpty(value))
                {
                    return value.Length > 500 ? value.Substring(0, 500) : value;
                }
            }

            // 如果没有标记，则尝试常见的标题属性
            var titleProperties = new[] { "Title", "Name", "DisplayName", "Description" };
            foreach (var propName in titleProperties)
            {
                var property = entity.GetType().GetProperty(propName);
                if (property != null && property.PropertyType == typeof(string))
                {
                    var value = property.GetValue(entity) as string;
                    if (!string.IsNullOrEmpty(value))
                    {
                        return value.Length > 500 ? value.Substring(0, 500) : value;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 判断是否应该忽略审计
        /// </summary>
        private bool ShouldIgnoreAudit(Type entityType)
        {
            return entityType.GetCustomAttribute<IgnoreAuditAttribute>() != null;
        }

        /// <summary>
        /// 获取需要审计的属性列表
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
