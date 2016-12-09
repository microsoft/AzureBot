namespace AzureBot.Helpers
{
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class AutomationJobsHelper
    {
        internal static string NextFriendlyJobId(IList<RunbookJob> automationJobs)
        {
            const string FirstFriendlyJobId = "job1";

            if (automationJobs == null || !automationJobs.Any())
            {
                return FirstFriendlyJobId;
            }

            var lastFriendlyJobId = string.Empty;

            for (int i = automationJobs.Count() - 1; i >= 0; i--)
            {
                var job = automationJobs[i];

                if (!string.IsNullOrWhiteSpace(job.FriendlyJobId))
                {
                    lastFriendlyJobId = job.FriendlyJobId;
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(lastFriendlyJobId))
            {
                return FirstFriendlyJobId;
            }

            var lastJobId = int.Parse(lastFriendlyJobId.Split(new string[] { "job" }, StringSplitOptions.RemoveEmptyEntries)[0]);

            return $"job{++lastJobId}";
        }
    }
}