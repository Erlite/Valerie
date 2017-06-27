﻿using Newtonsoft.Json;

namespace Rick.JsonModels
{
    public class WatsonModel
    {
        [JsonProperty("word_count")]
        public int WordCount { get; set; }

        [JsonProperty("character_count")]
        public int CharacterCount { get; set; }

        [JsonProperty("translations")]
        public WatsonTranslation[] Translations { get; set; }
    }

    public class WatsonTranslation
    {
        [JsonProperty("translation")]
        public string Translation { get; set; }
    }
}