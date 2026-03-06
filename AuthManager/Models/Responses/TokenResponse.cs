namespace AuthManager.Models.Responses
{
    public class TokenResponse
    {
        public string Token { get; set; }
        public DateTime ValidUntil { get; set; } 
    }
}
