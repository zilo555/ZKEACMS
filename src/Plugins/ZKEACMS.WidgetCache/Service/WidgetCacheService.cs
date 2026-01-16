using Easy.Extend;
using Easy.Serializer;
using LiteDB;
using System.Collections.Generic;
using ZKEACMS.DataArchived;

namespace ZKEACMS.Widget.Service
{
    internal class WidgetCacheService : IWidgetCacheService
    {
        private readonly IDataArchivedService _dataArchivedService;
        public WidgetCacheService(IDataArchivedService dataArchivedService)
        {
            _dataArchivedService = dataArchivedService;
        }

        private string CreateId(string key)
        {
            return $"widgets:{key}".ToLowerInvariant();
        }

        public List<WidgetBase> GetCachedWidgets(string key)
        {
            var data = _dataArchivedService.Get(CreateId(key));
            if (data == null || data.Data.IsNullOrEmpty()) return null;

            return JsonConverter.DeserializePolymorphic<List<WidgetBase>>(data.Data);
        }

        public void CacheWidgets(string key, List<WidgetBase> widgets)
        {
            var id = CreateId(key);
            var data = _dataArchivedService.Get(id);
            if (data == null)
            {
                _dataArchivedService.Add(new DataArchived.DataArchived
                {
                    ID = id,
                    Data = JsonConverter.SerializePolymorphic(widgets)
                });
            }
            else
            {
                data.Data = JsonConverter.SerializePolymorphic(widgets);
                _dataArchivedService.Update(data);
            }
        }
    }
}