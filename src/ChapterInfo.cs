﻿using System.Text.Json.Serialization;

namespace aaxclean_cli
{
	internal class RootObject
	{
		[JsonPropertyName("chapter_info")]
		public ChapterInfo ChapterInfo { get; set; }
	}

	internal class ChapterInfo
    {
        [JsonPropertyName("brandIntroDurationMs")]
        public long BrandIntroDurationMs { get; set; }

        [JsonPropertyName("brandOutroDurationMs")]
        public long BrandOutroDurationMs { get; set; }

        [JsonPropertyName("chapters")]
        public Chapter[] Chapters { get; set; }

        [JsonPropertyName("is_accurate")]
        public bool IsAccurate { get; set; }

        [JsonPropertyName("runtime_length_ms")]
        public long RuntimeLengthMs { get; set; }

        [JsonPropertyName("runtime_length_sec")]
        public long RuntimeLengthSec { get; set; }
    }
}
