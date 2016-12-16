using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using System.Threading;

namespace AzureBot
{
    [Serializable]
    public class AzureBotLuisDialog<R> : LuisDialog<R>
    {
        public async Task<bool> CanHandle(string query)
        {
            var tasks = services.Select(s => s.QueryAsync(query, CancellationToken.None)).ToArray();
            var winner = BestResultFrom(await Task.WhenAll(tasks));
            return winner != null && winner.BestIntent.Intent != "None";
        }
    }
}