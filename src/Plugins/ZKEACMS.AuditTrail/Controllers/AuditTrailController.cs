using Easy.AuditTrail;
using Easy.Mvc.Authorize;
using Easy.RepositoryPattern;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZKEACMS.AuditTrail.Controllers
{
    [DefaultAuthorize]
    public class AuditTrailController : Controller
    {
        private readonly IAuditTrailService _auditTrailService;

        public AuditTrailController(IAuditTrailService auditTrailService)
        {
            _auditTrailService = auditTrailService;
        }

        public IActionResult History(string entityType, string entityId, int? pageIndex)
        {
            var pagin = new Pagin
            {
                BaseUrlFormat = Url.Action("History", new { entityType, entityId }) + "&pageIndex={0}",
                PageIndex = pageIndex ?? 0,
                PageSize = 10
            };
            var histories = _auditTrailService.GetByEntity(entityType, entityId, pagin);
            ViewBag.Pagin = pagin;
            return View(histories);
        }
    }
}
