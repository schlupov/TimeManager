namespace TimeManager.configuration
{
    public record Configuration
    {
        public string Password { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string ChatLog { get; set; }
    }
}