using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AzureBot.Azure.Management.Models;
using Newtonsoft.Json;

namespace AzureBot.Azure.Management.Data
{
    public static class MockData
    {
        public static IEnumerable<Subscription> GetSubscriptions()
        {
            var json = ReadAllText("AzureBot.Azure.Management.Data.MockData.json");
            var subscriptions = JsonConvert.DeserializeObject<IEnumerable<Subscription>>(json);
            return subscriptions;
        }

        public static string ReadAllText(string fileName)
        {
            string end;
            using (Stream stream = Assembly.GetCallingAssembly().GetManifestResourceStream(fileName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    end = reader.ReadToEnd();
                }
            }
            return end;
        }
    }
}
