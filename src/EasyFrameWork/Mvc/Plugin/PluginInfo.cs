/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Easy.Mvc.Plugin
{
    public class PluginInfo
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public bool Enable { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public string WebSite { get; set; }
        public string Description { get; set; }
        public DateTime? PublishedDate { get; set; }

        //Additional Property
        [JsonIgnore]
        public string RelativePath { get; set; }
        [JsonIgnore]
        public string DirectoryName { get; set; }
        [JsonIgnore]
        public Assembly Assembly { get; set; }
        public HashSet<string> EmbeddedResource { get; set; }

        public static string FormatResourcePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return string.Empty;

            int start = 0;
            int end = filePath.Length - 1;
            while (start <= end && (filePath[start] == '/' || filePath[start] == '\\')) start++;
            while (end >= start && (filePath[end] == '/' || filePath[end] == '\\')) end--;

            if (start > end) return string.Empty;

            int length = end - start + 1;

            int lastSeparator = -1;
            for (int i = end; i >= start; i--)
            {
                if (filePath[i] == '/' || filePath[i] == '\\')
                {
                    lastSeparator = i;
                    break;
                }
            }

            if (lastSeparator == -1)
            {
                return filePath.AsSpan(start, length).ToString();
            }

            return string.Create(length, (filePath, start, lastSeparator), (span, state) =>
            {
                string source = state.filePath;
                int offset = state.start;
                int sepIndex = state.lastSeparator;

                for (int i = 0; i < span.Length; i++)
                {
                    int originalIndex = offset + i;
                    char c = source[originalIndex];

                    if (originalIndex <= sepIndex)
                    {
                        if (c == '/' || c == '\\')
                        {
                            span[i] = '.';
                        }
                        else if (c == '-')
                        {
                            span[i] = '_';
                        }
                        else
                        {
                            span[i] = c;
                        }
                    }
                    else
                    {
                        span[i] = c;
                    }
                }
            });
        }
    }
}
