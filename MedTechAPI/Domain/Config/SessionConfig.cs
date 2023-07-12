namespace MedTechAPI.Domain.Config
{
    public class SessionConfig
    {
        public Auth Auth { get; set; }
    }

    public class Auth
    {
        public int ExpireMinutes { get; set; }
        public bool HttpOnly { get; set; }
        public bool Secure { get; set; }
        public bool IsEssential { get; set; }
        public string token { get; set; }
    }
}
