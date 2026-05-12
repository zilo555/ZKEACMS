/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */


using Easy.AuditTrail;
using Easy.Modules.Role;
using Easy.Modules.User.Service;
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
            services.AddAuditValueProvider<AuditNavigationValueProvider>();
            services.AddAuditValueProvider<AuditWidgetStatusValueProvider>();

            services.AddScoped<AuditWidgetZoneValueProvider>();
            services.AddScoped<IAuditWidgetZoneValueProvider, AuditWidgetZoneValueProvider>(provider => provider.GetService<AuditWidgetZoneValueProvider>());
            services.AddScoped<IAuditValueProvider, AuditWidgetZoneValueProvider>(provider => provider.GetService<AuditWidgetZoneValueProvider>());

            services.AddAuditValueProvider<AuditCarouselValueProvider>();
            services.AddAuditValueProvider<AuditWidgetRuleValueProvider>();
            services.AddAuditValueProvider<AuditLayoutValueProvider>();
            services.AddAuditValueProvider<UserDictionaryAuditValueProvider>();
            services.AddAuditValueProvider<RoleAuditValueProvider>();
        }
        public static void AddAuditValueProvider<TAuditValueProvider>(this IServiceCollection services)
            where TAuditValueProvider : class, IAuditPropertyProvider
        {
            services.AddScoped<IAuditPropertyProvider, TAuditValueProvider>();
        }
    }
}
