using System;
namespace Secretary.Options
{
    public class SecretOptions
    {
        public int DefaultAccessAttempts { get; set; }
        public int FindExpiredSecretsInMinute { get; set; }

    }
}

