namespace UrlShortener.Api.Models;

public class Link
{
    public int Id { get; set; }

    public string OriginalUrl { get; set; } = string.Empty;

    public string ShortCode { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } //когда создали ссылку

    public int ClickCount { get; set; }
}