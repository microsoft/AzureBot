namespace AzureBot.Models
{
    using System;

    [Serializable]
    public class AuthResult
    {
        public string AccessToken { get; set; }

        public long ExpiresOnUtcTicks { get; set; }

        public string UserDisplayableId { get; set; }
    }
}