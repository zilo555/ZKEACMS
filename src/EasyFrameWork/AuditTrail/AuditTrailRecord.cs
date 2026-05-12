/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using Easy.Constant;
using Easy.MetaData;
using Easy.Models;
using Easy.Modules.User.Models;
using Easy.RepositoryPattern;
using System;
using System.ComponentModel.DataAnnotations;

namespace Easy.AuditTrail
{
    [DataTable("AuditTrail")]
    public class AuditTrailRecord : EditorEntity
    {
        [Key]
        public int ID { get; set; }        
        public string EntityType { get; set; }
        public string EntityID { get; set; }
        public string Changes { get; set; }
        public string IPAddress { get; set; }
    }
    class AuditTrailRecordMetaData : ViewMetaData<AuditTrailRecord>
    {
        protected override void ViewConfigure()
        {
            ViewConfig(p => p.ID).AsHidden();
            ViewConfig(p => p.EntityType).AsHidden();
            ViewConfig(p => p.EntityID).AsHidden();
            ViewConfig(p => p.Changes).AsTextArea();
            ViewConfig(p => p.IPAddress).AsTextBox();
        }
    }
}
