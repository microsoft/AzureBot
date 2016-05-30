namespace AzureBot.Models
{
    using System;

    [Serializable]
    public class AuthResult
    {
        public string AccessToken { get; set; }

        public string UserName { get; set; }

        public string UserUniqueId { get; set; }

        public long ExpiresOnUtcTicks { get; set; }

        public byte[] TokenCache { get; set; }
    }
}