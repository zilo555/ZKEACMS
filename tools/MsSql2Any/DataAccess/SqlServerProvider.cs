/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using Microsoft.Data.SqlClient;
using System.Data;

namespace MsSql2Any.DataAccess;

public class SqlServerProvider : ISourceDbProvider
{
    private SqlConnection? _connection;

    public void Initialize(string connectionString)
    {
        _connection = new SqlConnection(connectionString);
    }

    public async Task<List<string>> GetTableNamesAsync()
    {
        if (_connection == null) throw new InvalidOperationException("Database connection not initialized");

        var tableNames = new List<string>();

        const string sql = @"
            SELECT TABLE_SCHEMA + '.' + TABLE_NAME AS TableName
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_TYPE = 'BASE TABLE'
              AND TABLE_SCHEMA != 'sys'
            ORDER BY TABLE_SCHEMA, TABLE_NAME";

        await _connection.OpenAsync();
        using var command = new SqlCommand(sql, _connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            tableNames.Add(reader.GetString(0));
        }

        await _connection.CloseAsync();
        return tableNames;
    }

    public async Task<List<ColumnInfo>> GetColumnsAsync(string tableName)
    {
        if (_connection == null) throw new InvalidOperationException("Database connection not initialized");

        var columns = new List<ColumnInfo>();
        // Query all column information for the specified table, including primary key, identity column, etc.
        const string sql = @"
            SELECT
                c.COLUMN_NAME,
                c.DATA_TYPE,
                CASE WHEN c.IS_NULLABLE = 'YES' THEN 1 ELSE 0 END AS IsNullable,
                CASE WHEN pk.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END AS IsPrimaryKey,
                CASE WHEN ic.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END AS IsIdentity,
                COALESCE(CONVERT(int, c.CHARACTER_MAXIMUM_LENGTH), 0) AS MaxLength,
                COALESCE(c.NUMERIC_PRECISION, 0) AS Precision,
                COALESCE(c.NUMERIC_SCALE, 0) AS Scale,
                COALESCE(c.COLUMN_DEFAULT, '') AS DefaultValue
            FROM INFORMATION_SCHEMA.COLUMNS c
            LEFT JOIN (
                SELECT ku.COLUMN_NAME
                FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE ku
                    ON tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME
                WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
                  AND tc.TABLE_NAME = PARSENAME(@TableName, 1) -- Table name
                  AND tc.TABLE_SCHEMA = PARSENAME(@TableName, 2) -- Schema name
            ) pk ON c.COLUMN_NAME = pk.COLUMN_NAME
                 AND c.TABLE_NAME = PARSENAME(@TableName, 1) -- Table name
                 AND c.TABLE_SCHEMA = PARSENAME(@TableName, 2) -- Schema name
            LEFT JOIN (
                SELECT COL_NAME(ic.OBJECT_ID, ic.COLUMN_ID) AS COLUMN_NAME
                FROM sys.identity_columns ic
                INNER JOIN sys.tables t ON ic.OBJECT_ID = t.OBJECT_ID
                WHERE t.NAME = PARSENAME(@TableName, 1)  -- Table name
                  AND SCHEMA_NAME(t.schema_id) = PARSENAME(@TableName, 2)  -- Schema name
            ) ic ON c.COLUMN_NAME = ic.COLUMN_NAME
            WHERE c.TABLE_NAME = PARSENAME(@TableName, 1)  -- Table name
              AND c.TABLE_SCHEMA = PARSENAME(@TableName, 2)  -- Schema name
            ORDER BY c.ORDINAL_POSITION";

        await _connection.OpenAsync();
        using var command = new SqlCommand(sql, _connection);
        command.Parameters.AddWithValue("@TableName", tableName);
        
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            columns.Add(new ColumnInfo
            {
                Name = reader.GetString("COLUMN_NAME"),
                DataType = reader.GetString("DATA_TYPE"),
                IsNullable = Convert.ToBoolean(reader.GetValue("IsNullable")),
                IsPrimaryKey = Convert.ToBoolean(reader.GetValue("IsPrimaryKey")),
                IsIdentity = Convert.ToBoolean(reader.GetValue("IsIdentity")),
                MaxLength = reader.GetInt32("MaxLength"),
                Precision = reader.GetInt32("Precision"),
                Scale = reader.GetInt32("Scale"),
                DefaultValue = reader.IsDBNull("DefaultValue") ? string.Empty : reader.GetString("DefaultValue")
            });
        }

        await _connection.CloseAsync();
        return columns;
    }

    public async IAsyncEnumerable<object[]> GetDataAsync(string tableName, int batchSize)
    {
        if (_connection == null) throw new InvalidOperationException("Database connection not initialized");

        await _connection.OpenAsync();

        // Use query to get data, correctly handling table names with schema
        string formattedTableName;
        if (tableName.Contains("."))
        {
            // If table name contains schema (e.g. dbo.TableName), handle schema and table separately
            var parts = tableName.Split('.');
            formattedTableName = $"[{parts[0]}].[{parts[1]}]";
        }
        else
        {
            // If table name does not contain schema, just add brackets
            formattedTableName = $"[{tableName}]";
        }

        var sql = $"SELECT * FROM {formattedTableName}";

        using var command = new SqlCommand(sql, _connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var values = new object[reader.FieldCount];
            reader.GetValues(values); // Get all values from current row
            yield return values;
        }

        await _connection.CloseAsync();
    }
}