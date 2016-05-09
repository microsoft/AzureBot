namespace AzureBot.Helpers
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    public static class SerializerHelper
    {
        public static string SerializeObject(object value)
        {
            var binaryState = string.Empty;

            using (MemoryStream ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, value);
                binaryState = Convert.ToBase64String(ms.ToArray());
            }

            return binaryState;
        }
    }
}
