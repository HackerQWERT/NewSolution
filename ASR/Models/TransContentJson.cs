namespace ASR.Models;

public class TransContentJson
{
    [JsonPropertyName("sentences")]
    public List<Sentences> Sentences { get; set; }
}
public class Sentences
{
    [JsonPropertyName("c")]
    public double C { get; set; }

    [JsonPropertyName("et")]
    public int ET { get; set; }

    [JsonPropertyName("sa")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public SA? SA { get; set; }

    [JsonPropertyName("st")]
    public int ST { get; set; }

    [JsonPropertyName("text")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Text { get; set; }

}
public class SA
{
    [JsonPropertyName("role")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Role { get; set; }
}