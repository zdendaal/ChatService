namespace ChatService
{
    public class JWTConf
    {
        public string JWTSecret { get; set; } = string.Empty;
        public string JWTValidIssuer { get; set; } = string.Empty;
        public string JWTValidAudience { get; set; } = string.Empty;
    }
}
