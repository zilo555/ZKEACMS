/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using ZKEACMS.Common.Models;
using ZKEACMS.Storage;

namespace ZKEACMS.AuditTrail.Service
{
    /// <summary>
    /// 审计跟踪数据存储实现
    /// </summary>
    public class AuditTrailData : PluginData<AuditTrailPlug>, IAuditTrailData
    {
        private const string CollectionName = "AuditTrail";

        public AuditTrailData(ILogger<AuditTrailPlug> logger) : base(logger)
        {
        }

        public void Save(AuditTrailRecord record)
        {
            var collection = GetCollection<AuditTrailRecord>(CollectionName);
            
            // 确保索引
            collection.EnsureIndex(m => m.EntityType);
            collection.EnsureIndex(m => m.EntityID);
            collection.EnsureIndex(m => m.UserID);
            collection.EnsureIndex(m => m.OperationTime);
            
            collection.Insert(record);
        }

        public IEnumerable<AuditTrailRecord> GetByEntity(string entityType, string entityID)
        {
            return GetCollection<AuditTrailRecord>(CollectionName)
                .Query()
                .Where(m => m.EntityType == entityType && m.EntityID == entityID)
                .OrderByDescending(m => m.OperationTime)
                .ToEnumerable()
                .ToArray();
        }

        public IEnumerable<AuditTrailRecord> GetByUser(string userID, DateTime? startTime = null, DateTime? endTime = null, int limit = 1000)
        {
            var query = GetCollection<AuditTrailRecord>(CollectionName)
                .Query()
                .Where(m => m.UserID == userID);

            if (startTime.HasValue)
            {
                query = query.Where(m => m.OperationTime >= startTime.Value);
            }

            if (endTime.HasValue)
            {
                query = query.Where(m => m.OperationTime <= endTime.Value);
            }

            return query
                .OrderByDescending(m => m.OperationTime)
                .Limit(limit)
                .ToEnumerable()
                .ToArray();
        }

        public int CleanUp(DateTime beforeDate)
        {
            return GetCollection<AuditTrailRecord>(CollectionName)
                .DeleteMany(m => m.OperationTime < beforeDate);
        }
    }
}
