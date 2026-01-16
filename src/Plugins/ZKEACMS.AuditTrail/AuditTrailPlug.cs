/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using Easy.Mvc.Resource;
using Easy.Mvc.Route;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using ZKEACMS.WidgetTemplate;
using ZKEACMS.AuditTrail.Service;
using ZKEACMS.Common.Service;

namespace ZKEACMS.AuditTrail
{
    public class AuditTrailPlug : PluginBase
    {
        public override IEnumerable<RouteDescriptor> RegistRoute()
        {
            return null;
        }

        public override IEnumerable<AdminMenu> AdminMenu()
        {
            return null;
        }

        protected override void InitScript(Func<string, ResourceHelper> script)
        {

        }

        protected override void InitStyle(Func<string, ResourceHelper> style)
        {

        }

        public override IEnumerable<PermissionDescriptor> RegistPermission()
        {
            return null;
        }

        public override IEnumerable<WidgetTemplateEntity> WidgetServiceTypes()
        {
            return null;
        }

        public override void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IAuditTrailData, AuditTrailData>();
            serviceCollection.AddScoped<IAuditTrailService, AuditTrailService>();
        }

        public override void ConfigureApplication(IApplicationBuilder app, IWebHostEnvironment env)
        {
        }
    }
}