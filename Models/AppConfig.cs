using System.Text.Json.Serialization;

namespace WebNavigator.Models;

public class AppConfig
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0.0";

    [JsonPropertyName("websites")]
    public List<WebsiteItem> Websites { get; set; } = [];

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = [];

    [JsonPropertyName("lastModified")]
    public DateTime LastModified { get; set; } = DateTime.Now;
}
