/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using Easy.AuditTrail.Attributes;
using Easy.MetaData;
using Easy.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ZKEACMS.Common.Models
{
    public class TabItem : EditorEntity
    {
        [AuditKey]
        public int? ID { get; set; }
    }
    class TabItemMetaData : ViewMetaData<TabItem>
    {
        protected override void ViewConfigure()
        {
            ViewConfig(m => m.ID).AsHidden();
            ViewConfig(m => m.Description).AsTextArea().AddClass(StringKeys.DynamicHtmlEditorClass);
        }
    }

}
