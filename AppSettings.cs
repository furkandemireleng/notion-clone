namespace notion_clone;

public class AppSettings
{
    public TokenSetting? JWT { get; init; }
    public string? SendGridApiKey { get; init; }
    public string? APIKeyValue { get; set; }
    public string? NFTServiceUrl { get; set; }
    public string? ImageBlobStorageConnectionString { get; set; }
    public string? DocumentBlobStorageConnectionString { get; set; }
}

public class TokenSetting
{
    public string? Secret { get; set; }
    public string? ValidIssuer { get; set; }
    public string? ValidAudience { get; set; }
}