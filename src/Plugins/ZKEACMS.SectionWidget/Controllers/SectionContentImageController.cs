using ZKEACMS.SectionWidget.Models;
using ZKEACMS.SectionWidget.Service;
using Easy.Constant;
using Easy.Mvc.Authorize;
using Microsoft.AspNetCore.Mvc;
using Easy.Extend;
using ZKEACMS.Widget;
using ZKEACMS.Event;

namespace ZKEACMS.SectionWidget.Controllers
{
    [DefaultAuthorize]
    public class SectionContentImageController : Controller
    {
        private readonly ISectionContentProviderService _sectionContentProviderService;
        private readonly IWidgetBasePartService _widgetBasePartService;
        private readonly IEventManager _eventManager;

        public SectionContentImageController(ISectionContentProviderService sectionContentProviderService,
            IWidgetBasePartService widgetBasePartService,
            IEventManager eventManager)
        {
            _sectionContentProviderService = sectionContentProviderService;
            _widgetBasePartService = widgetBasePartService;
            _eventManager = eventManager;
        }

        public ActionResult Create(string sectionGroupId, string sectionWidgetId)
        {
            return View("Form", new SectionContentImage
            {
                SectionGroupId = sectionGroupId,
                SectionWidgetId = sectionWidgetId,
                ActionType = ActionType.Create
            });
        }

        public ActionResult Edit(string Id)
        {
            var content = _sectionContentProviderService.GetContent(Id);
            content.ActionType = ActionType.Update;
            return View("Form", content);
        }
        [HttpPost]
        public ActionResult Save(SectionContentImage content)
        {
            if (!ModelState.IsValid)
            {
                return View("Form", content);
            }
            
            var widget = _widgetBasePartService.Get(content.SectionWidgetId);
            _eventManager.Trigger(Events.OnWidgetUpdating, widget);
            
            if (content.ActionType.HasFlag(ActionType.Create))
            {
                _sectionContentProviderService.Add(content);
            }
            else
            {
                _sectionContentProviderService.Update(content);
            }
            
            _eventManager.Trigger(Events.OnWidgetUpdated, widget);
            
            ViewBag.Close = true;
            return View("Form", content);
        }

        public JsonResult Delete(string Id)
        {
            var content = _sectionContentProviderService.Get(Id);
            var widgetId = content.SectionWidgetId;
            
            var widget = _widgetBasePartService.Get(widgetId);
            _eventManager.Trigger(Events.OnWidgetUpdating, widget);
            
            _sectionContentProviderService.Remove(Id);
            
            _eventManager.Trigger(Events.OnWidgetUpdated, widget);
            
            return Json(true);
        }
    }
}
