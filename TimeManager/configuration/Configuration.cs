namespace TimeManager.configuration
{
    public record Configuration
    {
        public string PathToToken { get; init; }
        public string Password { get; init; }
        public string Email { get; init; }
    }
}