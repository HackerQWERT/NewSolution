namespace ASR.Models;

class PostParameters
{
    [JsonPropertyName("model_type")] public string ModelType { get; set; }
    [JsonPropertyName("task")] public string Task { get; set; }
    [JsonPropertyName("language")] public string Language { get; set; }
    [JsonPropertyName("output")] public string OutPut { get; set; }
    [JsonPropertyName("initial_prompt")] public string InitialPrompt { get; set; }
    [JsonPropertyName("encode")] public bool Encode { get; set; }
    [JsonPropertyName("word_timestamps")] public bool WordTimestamps { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)] public string AudioFile { get; set; }
}
