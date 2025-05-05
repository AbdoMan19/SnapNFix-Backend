public class IpRateLimitingOptions
{
    public static Dictionary<string, RateLimitRule> GetRules()
    {
        return new Dictionary<string, RateLimitRule>
        {
            // Login attempts
            {"/api/auth/login", new RateLimitRule 
                { 
                    Limit = 5,
                    WindowInMinutes = 5
                }
            },
            
            // Registration
            {"/api/auth/register", new RateLimitRule
                {
                    Limit = 3,
                    WindowInMinutes = 60
                }
            },
            
            // Password reset
            {"/api/auth/reset-password", new RateLimitRule
                {
                    Limit = 3,
                    WindowInMinutes = 60
                }
            },
            
            // File uploads
            {"/api/files/upload", new RateLimitRule
                {
                    Limit = 10,
                    WindowInMinutes = 5
                }
            },
            
            // Default rule for all other endpoints
            {"*", new RateLimitRule
                {
                    Limit = 100,
                    WindowInMinutes = 1
                }
            }
        };
    }
}

public class RateLimitRule
{
    public int Limit { get; set; }
    public int WindowInMinutes { get; set; }
}