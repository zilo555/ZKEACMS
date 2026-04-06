/* http://www.zkea.net/
 * Copyright (c) ZKEASOFT. All rights reserved.
 * http://www.zkea.net/licenses */

using ZKEACMS.SectionWidget.Models;
using ZKEACMS.SectionWidget.Service;
using Easy.Constant;
using Microsoft.AspNetCore.Mvc;
using Easy.Mvc.Authorize;
using Easy.Extend;
using System.Text.RegularExpressions;
using ZKEACMS.Filter;
using ZKEACMS.Widget;
using ZKEACMS.Event;

namespace ZKEACMS.SectionWidget.Controllers
{
    public partial class SectionContentVideoController : Controller
    {
        private readonly ISectionContentProviderService _sectionContentProviderService;
        private readonly IWidgetBasePartService _widgetBasePartService;
        private readonly IEventManager _eventManager;

        public SectionContentVideoController(ISectionContentProviderService sectionContentProviderService,
            IWidgetBasePartService widgetBasePartService,
            IEventManager eventManager)
        {
            _sectionContentProviderService = sectionContentProviderService;
            _widgetBasePartService = widgetBasePartService;
            _eventManager = eventManager;
        }
        [DefaultAuthorize]
        public ActionResult Create(string sectionGroupId, string sectionWidgetId)
        {
            return View("Form", new SectionContentVideo
            {
                SectionGroupId = sectionGroupId,
                SectionWidgetId = sectionWidgetId,
                ActionType = ActionType.Create
            });
        }
        [DefaultAuthorize]
        public ActionResult Edit(string Id)
        {
            var content = _sectionContentProviderService.GetContent(Id);
            content.ActionType = ActionType.Update;
            return View("Form", content);
        }
        [HttpPost, DefaultAuthorize]
        public ActionResult Save(SectionContentVideo content)
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
        [DefaultAuthorize]
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

        [Themed]
        public ActionResult Play(string Id)
        {
            SectionContentVideo video = _sectionContentProviderService.GetContent(Id) as SectionContentVideo;
            if (video == null) return NotFound();

            if (video.Code.IsNotNullAndWhiteSpace())
            {
                video.Code = RegexVideoCodeWidth().Replace(video.Code, eva =>
                {
                    return $"{eva.Groups[1].Value}{eva.Groups[2].Value}{eva.Groups[3].Value}{eva.Groups[4].Value}{eva.Groups[5].Value}{video.Width}{eva.Groups[7].Value}";
                });
                video.Code = RegexVideoCodeHeight().Replace(video.Code, eva =>
                {
                    return $"{eva.Groups[1].Value}{eva.Groups[2].Value}{eva.Groups[3].Value}{eva.Groups[4].Value}{eva.Groups[5].Value}{video.Height}{eva.Groups[7].Value}";
                });
            }

            return View(video);
        }

        [GeneratedRegex("(\\s)?(width)(\\s)?([=:])(\\s)?(\\d+)(\\s)?", RegexOptions.IgnoreCase)]
        private static partial Regex RegexVideoCodeWidth();

        [GeneratedRegex("(\\s)?(height)(\\s)?([=:])(\\s)?(\\d+)(\\s)?", RegexOptions.IgnoreCase)]
        private static partial Regex RegexVideoCodeHeight();
    }
}
