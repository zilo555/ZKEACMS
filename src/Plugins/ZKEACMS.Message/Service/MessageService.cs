/* http://www.zkea.net/
 * Copyright (c) ZKEASOFT. All rights reserved.
 * http://www.zkea.net/licenses */

using Easy;
using Easy.AuditTrail;
using Easy.Constant;
using Easy.Extend;
using Easy.RepositoryPattern;
using System;
using ZKEACMS.Event;
using ZKEACMS.Message.Models;
using ZKEACMS.Setting;

namespace ZKEACMS.Message.Service
{
    public class MessageService : ServiceBase<MessageEntity, CMSDbContext>, IMessageService
    {
        private readonly IEventManager _eventManager;
        private readonly IAuditTrailService _auditTrailService;
        public MessageService(IApplicationContext applicationContext, CMSDbContext dbContext, IEventManager eventManager, IAuditTrailService auditTrailService)
            : base(applicationContext, dbContext)
        {
            _eventManager = eventManager;
            _auditTrailService = auditTrailService;
        }
        public override ErrorOr<MessageEntity> Add(MessageEntity item)
        {
            ErrorOr<MessageEntity> result = base.Add(item);
            if (!result.HasError && item.ActionType == ActionType.Continue)
            {
                _eventManager.Trigger(Events.OnMessageSubmitted, item);
            }
            return result;
        }

        public override ErrorOr<MessageEntity> Update(MessageEntity item)
        {
            var oldMessage = Get(item.ID);
            var result = base.Update(item);
            if (result.IsSuccess)
            {
                _auditTrailService.AuditUpdate(oldMessage, item);
            }
            return result;
        }
    }
}