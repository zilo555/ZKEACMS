/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */


using Microsoft.Extensions.DependencyInjection;
using ZKEACMS.Common.Service;

namespace ZKEACMS
{
    public static class BuilderAuditTrial
    {
        public static void ConfigAuditTrial(this IServiceCollection services)
        {
            services.AddScoped<IAuditValueProvider, AuditRecordStatusValueProvider>();
            services.AddScoped<IAuditValueProvider, AuditLocalizeDisplayProvider>();
        }
    }
}
