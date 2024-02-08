namespace notion_clone.Dto;

public class SigninResponseDto
{
    public string UserId { get; set; }
    public string AccessToken { get; set; }
    public double ExpireTime { get; set; }
    public string RefreshToken { get; set; }
    
    public string Role { get; set; }
}
