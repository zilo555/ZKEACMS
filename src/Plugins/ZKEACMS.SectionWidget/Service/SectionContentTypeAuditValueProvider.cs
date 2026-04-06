/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using Easy;
using Easy.AuditTrail;
using Easy.Options;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ZKEACMS.SectionWidget.Models;

namespace ZKEACMS.SectionWidget.Service
{
    internal class SectionContentTypeAuditValueProvider : IAuditValueProvider
    {
        private readonly ILocalize _localize;

        public SectionContentTypeAuditValueProvider(ILocalize localize)
        {
            _localize = localize;
        }

        public int Priority => 10;

        public bool CanHandle(PropertyInfo property, Type entityType)
        {
            return typeof(SectionContent).IsAssignableFrom(entityType) &&
                nameof(SectionContent.SectionContentType) == property.Name;
        }

        public string GetDisplayValue(PropertyInfo property, object rawValue)
        {
            if (rawValue is int contentType)
            {
                if (contentType == (int)SectionContentBase.Types.CallToAction)
                {
                    return _localize.Get("Link");

                }
                else if (contentType == (int)SectionContentBase.Types.Image)
                {
                    return _localize.Get("Picture");
                }
                else if (contentType == (int)SectionContentBase.Types.Paragraph)
                {
                    return _localize.Get("Paragraph");
                }
                else if (contentType == (int)SectionContentBase.Types.Title)
                {
                    return _localize.Get("Title");
                }
                else if (contentType == (int)SectionContentBase.Types.Video)
                {
                    return _localize.Get("Video");
                }
            }
            return rawValue?.ToString();
        }
    }
}
