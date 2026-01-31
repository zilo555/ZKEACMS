using MsSql2Any.Config;
using MsSql2Any.DataAccess;
using MsSql2Any.ScriptGenerators;

namespace MsSql2Any.Services;

public class DatabaseExportService
{
    private readonly ISourceDbProvider _sourceDbProvider;
    private readonly IEnumerable<IScriptGenerator> _scriptGenerators;
    private readonly AppConfig _config;

    public DatabaseExportService(ISourceDbProvider sourceDbProvider, 
        IEnumerable<IScriptGenerator> scriptGenerators, 
        AppConfig config)
    {
        _sourceDbProvider = sourceDbProvider;
        _scriptGenerators = scriptGenerators;
        _config = config;
    }

    public async Task ExportAsync()
    {
        Console.WriteLine("Starting database export...");

        _sourceDbProvider.Initialize(_config.SourceConnectionString);

        var tableNames = await _sourceDbProvider.GetTableNamesAsync();
        Console.WriteLine($"Found {tableNames.Count} tables");

        foreach (var generator in _scriptGenerators)
        {
            Console.WriteLine($"Generating {generator.DbType} scripts...");

            var dbScript = new System.Text.StringBuilder();

            foreach (var tableName in tableNames)
            {
                Console.WriteLine($"  Processing table: {tableName}");

                string cleanTableName = tableName.Contains(".") ? tableName.Split('.')[1] : tableName;

                var columns = await _sourceDbProvider.GetColumnsAsync(tableName);

                dbScript.AppendLine(generator.GenerateCreateTableScript(cleanTableName, columns));

                var dataRows = new List<object[]>();
                await foreach (var row in _sourceDbProvider.GetDataAsync(tableName, _config.BatchSize))
                {
                    dataRows.Add(row);

                    if (dataRows.Count >= _config.BatchSize)
                    {
                        dbScript.AppendLine(generator.GenerateInsertScript(cleanTableName, columns, dataRows));
                        dataRows.Clear();
                    }
                }

                if (dataRows.Any())
                {
                    dbScript.AppendLine(generator.GenerateInsertScript(cleanTableName, columns, dataRows));
                }
            }

            Directory.CreateDirectory(_config.OutputDirectory);

            var fileName = Path.Combine(_config.OutputDirectory, $"{generator.DbType}.sql");
            await File.WriteAllTextAsync(fileName, dbScript.ToString());
            Console.WriteLine($"  {generator.DbType} script saved to: {fileName}");
        }

        Console.WriteLine("Export completed!");
    }
}