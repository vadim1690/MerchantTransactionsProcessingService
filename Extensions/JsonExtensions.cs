using System.Text;
using System.Text.Json;

namespace MerchantTransactionProcessing.Extensions
{
    public static class JsonExtensions
    {
        public static async Task<T?> FromJsonAsync<T>(this string json, T? defaultVal = default)
        {
            using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            return await JsonSerializer.DeserializeAsync<T>(memoryStream) ?? defaultVal;
        }

        public static T? FromJson<T>(this string json, T? defaultVal = default)
        {
            return JsonSerializer.Deserialize<T>(json) ?? defaultVal;
        }

        public static async Task<string> ToJsonAsync(this object obj)
        {
            using var memoryStream = new MemoryStream();
            await JsonSerializer.SerializeAsync(memoryStream, obj);
            memoryStream.Position = 0;
            using var reader = new StreamReader(memoryStream);
            return await reader.ReadToEndAsync();
        }

        public static string ToJson(this object obj,string defaultVal = "{}")
        {
            return JsonSerializer.Serialize(obj) ?? defaultVal;
        }

    }
}
