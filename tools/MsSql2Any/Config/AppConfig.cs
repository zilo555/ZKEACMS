using System.ComponentModel.DataAnnotations;

namespace MsSql2Any.Config;

public class AppConfig
{
    [Required]
    public string SourceConnectionString { get; set; } = string.Empty;

    public string OutputDirectory { get; set; } = Environment.CurrentDirectory;

    public int BatchSize { get; set; } = 1000;
}