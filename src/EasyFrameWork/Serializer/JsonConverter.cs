/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;

namespace Easy.Serializer
{
    public static class JsonConverter
    {
        public static string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
        public static T Deserialize<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new ArgumentNullException(nameof(json));
            }

            return JsonConvert.DeserializeObject<T>(json);
        }

        public static object Deserialize(string json, Type returnType)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new ArgumentNullException(nameof(json));
            }

            if (returnType is null)
            {
                throw new ArgumentNullException(nameof(returnType));
            }

            return JsonConvert.DeserializeObject(json, returnType);
        }

        public static string SerializePolymorphic(object obj)
        {
            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All,
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                ContractResolver = new IgnoreJsonIgnoreContractResolver()
            };
            return JsonConvert.SerializeObject(obj, settings);
        }

        public static T DeserializePolymorphic<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new ArgumentNullException(nameof(json));
            }

            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All,
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                ContractResolver = new IgnoreJsonIgnoreContractResolver()
            };
            
            return JsonConvert.DeserializeObject<T>(json, settings);
        }

        private class IgnoreJsonIgnoreContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(System.Reflection.MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);
                if (property.Ignored)
                {
                    property.Ignored = false;
                }
                return property;
            }
        }
    }
}