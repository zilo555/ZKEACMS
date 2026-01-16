using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZKEACMS.Widget
{
    public interface IWidgetCacheService
    {
        void CacheWidgets(string key, List<WidgetBase> widgets);
        List<WidgetBase> GetCachedWidgets(string key);
    }
}
