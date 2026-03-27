/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using System.Collections.Generic;
using System.Linq;
using Easy;
using Easy.Extend;
using ZKEACMS.Common.Models;
using ZKEACMS.Widget;
using ZKEACMS.Page;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZKEACMS.Extend;

namespace ZKEACMS.Common.Service
{
    public class TabWidgetService : SimpleWidgetService<TabWidget>
    {
        public TabWidgetService(IWidgetBasePartService widgetBasePartService, IApplicationContext applicationContext, CMSDbContext dbContext) :
            base(widgetBasePartService, applicationContext, dbContext)
        {
        }

        public override ErrorOr<TabWidget> Add(TabWidget item)
        {
            item.TabItems = item.TabItems.RemoveDeletedItems().ToList();
            int id = 0;
            item.TabItems.ForEach(t => t.ID = ++id);
            return base.Add(item);
        }
        public override ErrorOr<TabWidget> Update(TabWidget item)
        {
            item.TabItems = item.TabItems.RemoveDeletedItems().ToList();
            int id = 0;
            if (item.TabItems.Any())
            {
                id = item.TabItems.Max(o => o.ID) ?? 0;
            }
            item.TabItems.ForEach(m =>
            {
                if (m.ID == null)
                {
                    m.ID = ++id;
                }
            });
            return base.Update(item);
        }

        protected override IEnumerable<string> GetFilesInWidget(TabWidget widget)
        {
            return base.GetFilesInWidget(widget);
        }
    }
}