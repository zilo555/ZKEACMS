/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using System.Data;

namespace MsSql2Any.DataAccess;

public interface ISourceDbProvider
{
    void Initialize(string connectionString);

    Task<List<string>> GetTableNamesAsync();

    Task<List<ColumnInfo>> GetColumnsAsync(string tableName);

    IAsyncEnumerable<object[]> GetDataAsync(string tableName, int batchSize);
}

public class ColumnInfo
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
    public bool IsPrimaryKey { get; set; }
    public bool IsIdentity { get; set; }
    public int MaxLength { get; set; }
    public int Precision { get; set; }
    public int Scale { get; set; }
    public string DefaultValue { get; set; } = string.Empty;
}