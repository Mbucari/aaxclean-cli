using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aaxclean_cli
{
    internal class ChapterInfo
    {
        [JsonProperty("brandIntroDurationMs")]
        public long BrandIntroDurationMs { get; set; }

        [JsonProperty("brandOutroDurationMs")]
        public long BrandOutroDurationMs { get; set; }

        [JsonProperty("chapters")]
        public Chapter[] Chapters { get; set; }

        [JsonProperty("is_accurate")]
        public bool IsAccurate { get; set; }

        [JsonProperty("runtime_length_ms")]
        public long RuntimeLengthMs { get; set; }

        [JsonProperty("runtime_length_sec")]
        public long RuntimeLengthSec { get; set; }
    }
}
