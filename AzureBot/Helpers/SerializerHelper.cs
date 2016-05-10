namespace AzureBot.Helpers
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    internal static class SerializerHelper
    {
        internal static string SerializeObject(object value)
        {
            var binaryState = string.Empty;

            using (MemoryStream ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, value);
                binaryState = Convert.ToBase64String(ms.ToArray());
            }

            return binaryState;
        }

        internal static T DeserializeObject<T>(string base64Object)
            where T : class
        {
            byte[] bytes = Convert.FromBase64String(base64Object);
            using (MemoryStream ms = new MemoryStream(bytes, 0, bytes.Length))
            {
                ms.Write(bytes, 0, bytes.Length);
                ms.Position = 0;
                return new BinaryFormatter().Deserialize(ms) as T;
            }
        }
    }
}
