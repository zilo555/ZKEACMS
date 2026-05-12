/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using Easy.AuditTrail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ZKEACMS.SiteSearch.Models;

namespace ZKEACMS.SiteSearch.Service
{
    internal class SearchEngineAuditValueProvider : IAuditValueProvider
    {
        private readonly IEnumerable<SearchEngine> _searchEngines;

        public SearchEngineAuditValueProvider(IEnumerable<SearchEngine> searchEngines)
        {
            _searchEngines = searchEngines;
        }

        public int Priority => 10;

        public bool CanHandle(PropertyInfo property, Type entityType)
        {
            return entityType == typeof(SiteSearchWidget) && property.Name == nameof(SiteSearchWidget.SearchEngine);
        }

        public string GetDisplayValue(PropertyInfo property, object rawValue)
        {
            if (rawValue is string searchEngineKey)
            {
                var searchEngine = _searchEngines.FirstOrDefault(se => se.SearchQuery == searchEngineKey);
                return searchEngine?.Name ?? rawValue.ToString();
            }
            return rawValue?.ToString() ?? string.Empty;
        }
    }
}
