namespace VideoStreamingService
{
    public static class BusinessSettings
    {
        public const string name = "VideoStreamingService";
        public static readonly TimeSpan tokenExpiration = TimeSpan.FromHours(2);

        public const int nicknameMinLength = 3;
        public const int nicknameMaxLength = 100;

        public const int chatNameMinLength = 1;
        public const int chatNameMaxLength = 100;

        public const int messageNameMinLength = 1;
        public const int messageNameMaxLength = 20000;

        public const int passwordMinLength = 6;
        public const int passwordMaxLength = 200;
    }
}
