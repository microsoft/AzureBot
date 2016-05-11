namespace AzureBot.Models
{
    using System;

    [Serializable]
    public class ResumeState
    {
        public string UserId { get; set; }

        public string ConversationId { get; set; }
    }
}
