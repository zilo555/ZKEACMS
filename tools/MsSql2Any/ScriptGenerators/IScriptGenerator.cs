/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using MsSql2Any.DataAccess;

namespace MsSql2Any.ScriptGenerators;

public interface IScriptGenerator
{
    string DbType { get; }
    
    string GenerateCreateTableScript(string tableName, List<ColumnInfo> columns);

    string GenerateInsertScript(string tableName, List<ColumnInfo> columns, IEnumerable<object[]> dataRows);
}