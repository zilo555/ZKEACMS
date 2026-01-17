/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using System;
using System.Collections.Generic;
using ZKEACMS.Common.Models;

namespace ZKEACMS.AuditTrail.Service
{
    public interface IAuditTrailData
    {
        void Save(AuditTrailRecord record);

        IEnumerable<AuditTrailRecord> GetByEntity(string entityType, string entityID);

        IEnumerable<AuditTrailRecord> GetByUser(string userID, DateTime? startTime = null, DateTime? endTime = null, int limit = 1000);

        int CleanUp(DateTime beforeDate);
    }
}
