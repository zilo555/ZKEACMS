/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */


using Easy.AuditTrail;
using Microsoft.Extensions.DependencyInjection;
using ZKEACMS.Common.Service;

namespace ZKEACMS
{
    public static class BuilderAuditTrial
    {
        public static void ConfigAuditTrial(this IServiceCollection services)
        {
            services.AddAuditValueProvider<AuditRecordStatusValueProvider>();
            services.AddAuditValueProvider<AuditLocalizeDisplayProvider>();
            services.AddAuditValueProvider<AuditNavigationParentValueProvider>();
            services.AddAuditValueProvider<AuditWidgetStatusValueProvider>();

            services.AddScoped<AuditWidgetZoneValueProvider>();
            services.AddScoped<IAuditWidgetZoneValueProvider, AuditWidgetZoneValueProvider>(provider => provider.GetService<AuditWidgetZoneValueProvider>());
            services.AddScoped<IAuditValueProvider, AuditWidgetZoneValueProvider>(provider => provider.GetService<AuditWidgetZoneValueProvider>());
        }
        public static void AddAuditValueProvider<TAuditValueProvider>(this IServiceCollection services)
            where TAuditValueProvider : class, IAuditValueProvider
        {
            services.AddScoped<IAuditValueProvider, TAuditValueProvider>();
        }
    }
}
