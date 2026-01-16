/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using LiteDB;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZKEACMS.Storage
{
    public abstract class PluginData<T> : IDisposable where T : PluginBase
    {
        protected LiteDatabase Database;
        public PluginData()
        {
            InitDatabase();
        }

        protected virtual void InitDatabase()
        {
            var connectionString = Path.Combine(PluginBase.GetPath<T>(), "Data.db");
            Database = new LiteDatabase(connectionString);
        }

        public virtual ILiteCollection<TModel> GetCollection<TModel>(string name)
        {
            return Database.GetCollection<TModel>(name);
        }

        public void Dispose()
        {
            Database.Dispose();
        }
    }
}
