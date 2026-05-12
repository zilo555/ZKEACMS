using System;
using Easy;
using Easy.AuditTrail;
using Easy.Extend;
using ZKEACMS.Common.Service;
using ZKEACMS.Event;
using ZKEACMS.Widget;
using ZKEACMS.Zone;
using ZKEACMS.Page;
using ZKEACMS.Layout;

namespace ZKEACMS.AuditTrail.Event
{
    public sealed class WidgetAuditTrailEventHandler : IEventHandler
    {
        private readonly IAuditTrailService _auditTrailService;
        private readonly ILocalize _localize;
        private readonly IWidgetActivator _widgetActivator;
        private readonly IWidgetBasePartService _widgetBasePartService;
        private readonly IZoneService _zoneService;
        private readonly IPageService _pageService;
        private readonly IAuditWidgetZoneValueProvider _zoneValueProvider;

        private WidgetBase _oldWidget;

        public WidgetAuditTrailEventHandler(
            IAuditTrailService auditTrailService,
            ILocalize localize,
            IWidgetActivator widgetActivator,
            IWidgetBasePartService widgetBasePartService,
            IZoneService zoneService,
            IPageService pageService,
            IAuditWidgetZoneValueProvider zoneValueProvider)
        {
            _auditTrailService = auditTrailService;
            _localize = localize;
            _widgetActivator = widgetActivator;
            _widgetBasePartService = widgetBasePartService;
            _zoneService = zoneService;
            _pageService = pageService;
            _zoneValueProvider = zoneValueProvider;
        }

        public void Handle(object entity, EventArg e)
        {
            WidgetBase widgetBase = entity as WidgetBase;
            if (widgetBase == null) return;

            if (e.Name == Events.OnWidgetUpdating)
            {
                _oldWidget = GetFullWidget(widgetBase);
            }
            else if (e.Name == Events.OnWidgetUpdated)
            {
                var newWidget = GetFullWidget(widgetBase);
                AuditWidgetUpdate(newWidget);
            }
            else if (e.Name == Events.OnWidgetAdded)
            {
                AuditWidgetCreate(widgetBase);
            }
            else if (e.Name == Events.OnWidgetDeleted)
            {
                AuditWidgetDelete(widgetBase);
            }
        }

        private WidgetBase GetFullWidget(WidgetBase widgetBase)
        {
            var widgetBasePart = _widgetBasePartService.Get(widgetBase.ID);
            var widgetDriver = _widgetActivator.Create(widgetBasePart);
            return widgetDriver.GetWidget(widgetBasePart);
        }

        private void AuditWidgetCreate(WidgetBase widget)
        {
            if (widget.PageId.IsNotNullAndWhiteSpace())
            {
                _auditTrailService.AuditCreate<PageEntity>(widget.PageId, _localize.Get("Widget"), widget.WidgetName);
            }
            else if (widget.LayoutId.IsNotNullAndWhiteSpace())
            {
                _auditTrailService.AuditCreate<LayoutEntity>(widget.LayoutId, _localize.Get("Widget"), widget.WidgetName);
            }
        }

        private void AuditWidgetDelete(WidgetBase widget, string remark = null)
        {
            if (widget.PageId.IsNotNullAndWhiteSpace())
            {
                _auditTrailService.AuditDelete<PageEntity>(widget.PageId, _localize.Get("Widget"), widget.WidgetName, remark);
            }
            else if (widget.LayoutId.IsNotNullAndWhiteSpace())
            {
                _auditTrailService.AuditDelete<LayoutEntity>(widget.LayoutId, _localize.Get("Widget"), widget.WidgetName, remark);
            }
        }

        private void AuditWidgetUpdate(WidgetBase newWidget)
        {
            if (_oldWidget == null) return;

            try
            {
                if (_oldWidget.PageId.IsNotNullAndWhiteSpace())
                {
                    var page = _pageService.Get(_oldWidget.PageId);
                    if (page != null)
                    {
                        _zoneValueProvider.SetZones(_zoneService.GetByPage(page));
                    }
                }
                else if (_oldWidget.LayoutId.IsNotNullAndWhiteSpace())
                {
                    _zoneValueProvider.SetZones(_zoneService.GetByLayoutId(_oldWidget.LayoutId));
                }

                _auditTrailService.AuditUpdate(_oldWidget.GetType(), _oldWidget, newWidget);
            }
            finally
            {
                _oldWidget = null;
            }
        }
    }
}
