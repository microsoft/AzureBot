namespace AzureBot.Azure.Management.Data
{
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using AzureBot.Azure.Management.Models;
    using Newtonsoft.Json;

    public class MockData
    {
        private static MockData data;

        static MockData()
        {
            var json = ReadAllText("AzureBot.Azure.Management.Data.MockData.json");
            data = JsonConvert.DeserializeObject<MockData>(json);
        }

        public Subscription[] Subscriptions { get; set; }

        public VirtualMachine[] VirtualMachines { get; set; }

        public static IEnumerable<Subscription> GetSubscriptions()
        {
            return data.Subscriptions;
        }

        public static IEnumerable<VirtualMachine> GetVirtualMachines()
        {
            return data.VirtualMachines;
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
