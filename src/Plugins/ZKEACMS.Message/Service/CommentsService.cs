/* http://www.zkea.net/
 * Copyright (c) ZKEASOFT. All rights reserved.
 * http://www.zkea.net/licenses */

using Easy;
using Easy.AuditTrail;
using Easy.Extend;
using Easy.Notification;
using Easy.RepositoryPattern;
using System;
using ZKEACMS.Event;
using ZKEACMS.Message.Models;
using ZKEACMS.Setting;

namespace ZKEACMS.Message.Service
{
    public class CommentsService : ServiceBase<Comments, CMSDbContext>, ICommentsService
    {
        private readonly IEventManager _eventManager;
        private readonly IAuditTrailService _auditTrailService;
        public CommentsService(IApplicationContext applicationContext, CMSDbContext dbContext, IEventManager eventManager = null, IAuditTrailService auditTrailService = null)
            : base(applicationContext, dbContext)
        {
            _eventManager = eventManager;
            _auditTrailService = auditTrailService;
        }
        public override ErrorOr<Comments> Add(Comments item)
        {
            ErrorOr<Comments> result = base.Add(item);
            if (!result.HasError)
            {
                _eventManager.Trigger(Events.OnCommentsSubmitted, item);
            }
            return result;
        }

        public override ErrorOr<Comments> Update(Comments item)
        {
            var oldComments = Get(item.ID);
            var result = base.Update(item);
            if (result.IsSuccess)
            {
                _auditTrailService.AuditUpdate(oldComments, item);
            }
            return result;
        }
    }
}
