using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using MsSql2Any.Config;
using MsSql2Any.DataAccess;
using MsSql2Any.ScriptGenerators;
using MsSql2Any.Services;

await MainAsync(args);

static async Task MainAsync(string[] args)
{
    // 设置配置
    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .AddCommandLine(args);

    var configuration = builder.Build();

    // 绑定配置到对象
    var appConfig = configuration.Get<AppConfig>() ?? new AppConfig();
    // 验证必需的配置项
    if (string.IsNullOrEmpty(appConfig.SourceConnectionString))
    {
        Console.WriteLine("错误: 必须提供 SourceConnectionString 配置项");
        return;
    }

    // 设置依赖注入容器
    var services = new ServiceCollection();
    services.AddSingleton(appConfig);
    services.AddSingleton<ISourceDbProvider, SqlServerProvider>();
    services.AddSingleton<IScriptGenerator, MysqlScriptGenerator>();
    services.AddSingleton<IScriptGenerator, SqliteScriptGenerator>();
    services.AddSingleton<IScriptGenerator, PostgreSqlScriptGenerator>();
    services.AddSingleton<IScriptGenerator, DamengScriptGenerator>();
    services.AddScoped<DatabaseExportService>();

    var serviceProvider = services.BuildServiceProvider();

    try
    {
        var exportService = serviceProvider.GetRequiredService<DatabaseExportService>();
        await exportService.ExportAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"导出过程中发生错误: {ex.Message}");
        Console.WriteLine(ex.StackTrace);
    }
}
