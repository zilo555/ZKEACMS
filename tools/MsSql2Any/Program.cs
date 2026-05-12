/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using MsSql2Any.Config;
using MsSql2Any.DataAccess;
using MsSql2Any.ScriptGenerators;
using MsSql2Any.Services;

await MainAsync(args);

static async Task MainAsync(string[] args)
{
    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .AddCommandLine(args);

    var configuration = builder.Build();

    var appConfig = configuration.Get<AppConfig>() ?? new AppConfig();

    if (string.IsNullOrEmpty(appConfig.SourceConnectionString))
    {
        Console.WriteLine("错误: 必须提供 SourceConnectionString 配置项");
        return;
    }

    var services = new ServiceCollection();
    services.AddSingleton(appConfig);
    services.AddSingleton<ISourceDbProvider, SqlServerProvider>();
    services.AddSingleton<IScriptGenerator, MysqlScriptGenerator>();
    services.AddSingleton<IScriptGenerator, SqliteScriptGenerator>();
    services.AddSingleton<IScriptGenerator, PostgreSqlScriptGenerator>();
    services.AddSingleton<IScriptGenerator, DamengScriptGenerator>();
    services.AddSingleton<IScriptGenerator, OpenGaussScriptGenerator>();
    services.AddSingleton<IScriptGenerator, VastbaseScriptGenerator>();
    services.AddSingleton<IScriptGenerator, OceanBaseScriptGenerator>();
    services.AddScoped<DatabaseExportService>();

    var serviceProvider = services.BuildServiceProvider();

    try
    {
        var exportService = serviceProvider.GetRequiredService<DatabaseExportService>();
        await exportService.ExportAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        Console.WriteLine(ex.StackTrace);
    }
}
