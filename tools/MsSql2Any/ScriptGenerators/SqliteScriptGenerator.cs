using MsSql2Any.DataAccess;

namespace MsSql2Any.ScriptGenerators;

public class SqliteScriptGenerator : IScriptGenerator
{
    public string DbType => "SQLite";

    public string GenerateCreateTableScript(string tableName, List<ColumnInfo> columns)
    {
        var script = new System.Text.StringBuilder();
        script.AppendLine($"CREATE TABLE [{tableName}] (");
        
        var columnDefs = new List<string>();
        foreach (var col in columns)
        {
            var columnDef = $"  [{col.Name}] {MapDataType(col)}";
            
            if (!col.IsNullable && !col.IsIdentity)
                columnDef += " NOT NULL";
                
            if (!string.IsNullOrEmpty(col.DefaultValue))
            {
                columnDef += $" DEFAULT {col.DefaultValue}";
            }
            
            columnDefs.Add(columnDef);
        }
        
        script.Append(string.Join(",\n", columnDefs));
        
        // Add primary key constraint
        var primaryKeys = columns.Where(c => c.IsPrimaryKey).Select(c => c.Name).ToList();
        if (primaryKeys.Any())
        {
            script.Append($",\n  PRIMARY KEY ([{string.Join("], [", primaryKeys)}])");
        }
        
        script.AppendLine("\n);");
        
        return script.ToString();
    }

    public string GenerateInsertScript(string tableName, List<ColumnInfo> columns, IEnumerable<object[]> dataRows)
    {
        var script = new System.Text.StringBuilder();

        foreach (var row in dataRows)
        {
            var values = new List<string>();
            for (int i = 0; i < columns.Count; i++)
            {
                values.Add(FormatValue(row[i], columns[i].DataType));
            }

            // For SQLite, if there are identity/rowid columns, we might want to specify column names
            var identityColumns = columns.Where(c => c.IsIdentity).ToList();
            if (identityColumns.Any())
            {
                var columnNames = string.Join(", ", columns.Select(c => $"[{c.Name}]"));
                script.AppendLine($"INSERT INTO [{tableName}] ({columnNames}) VALUES ({string.Join(", ", values)});");
            }
            else
            {
                script.AppendLine($"INSERT INTO [{tableName}] VALUES ({string.Join(", ", values)});");
            }
        }

        return script.ToString();
    }

    private string MapDataType(ColumnInfo column)
    {
        // SQLite uses dynamic type system, here provide approximate mappings
        return column.DataType.ToLower() switch
        {
            "int" or "smallint" or "tinyint" or "bigint" => "INTEGER",
            "bit" => "INTEGER", // SQLite does not have boolean type, use INTEGER to store 0/1
            "decimal" or "numeric" or "money" or "smallmoney" => "REAL",
            "float" or "real" => "REAL",
            "datetime" or "datetime2" or "smalldatetime" or "date" or "time" => "TEXT", // Store as ISO8601 string
            "char" or "nchar" or "varchar" or "nvarchar" or "text" or "ntext" or "xml" => "TEXT",
            "binary" or "varbinary" or "image" => "BLOB",
            "uniqueidentifier" => "TEXT", // Store as string format GUID
            "timestamp" or "rowversion" => "BLOB",
            _ => "TEXT" // Use TEXT type by default
        };
    }

    private string FormatValue(object value, string dataType)
    {
        if (value == DBNull.Value)
            return "NULL";

        return dataType.ToLower() switch
        {
            "char" or "nchar" or "varchar" or "nvarchar" or
            "text" or "ntext" or "xml" or "uniqueidentifier" =>
                $"'{EscapeString(value.ToString())}'",
            "datetime" or "datetime2" or "smalldatetime" or "date" or "time" => FormatDateTimeValue(value)!,
            "binary" or "varbinary" or "image" =>
                $"X'{BitConverter.ToString((byte[])value).Replace("-", "")}'",
            "bit" => ((bool)value) ? "1" : "0",
            _ => value.ToString() ?? string.Empty
        };
    }

    private string FormatDateTimeValue(object value)
    {
        if (value is DateTime dateTime)
            return $"'{dateTime:yyyy-MM-dd HH:mm:ss.fff}'";
        else
            return $"'{EscapeString(value?.ToString() ?? string.Empty)}'";
    }

    private string EscapeString(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        return input.Replace("'", "''"); // In SQLite, use two single quotes for escaping
    }
}