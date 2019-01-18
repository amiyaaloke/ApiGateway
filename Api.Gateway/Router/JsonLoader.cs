using System;
using System.IO;
using Newtonsoft.Json;

namespace Api.Gateway.Router
{
    public class JsonLoader
    {
        public T LoadFromFile<T>(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            {
                string json = reader.ReadToEnd();
                T result = JsonConvert.DeserializeObject<T>(json);
                return result;
            }
        }

        public T Deserialize<T>(object jsonObject)
        {
            return JsonConvert.DeserializeObject<T>(Convert.ToString(jsonObject));
        }
    }
}
