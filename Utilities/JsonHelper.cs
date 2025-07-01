using System.Text.Json;

namespace Eryth.Utilities
{
    // JSON işlemleri yardımcı sınıfı
    public static class JsonHelper
    {
        private static readonly JsonSerializerOptions DefaultOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        // Object'i JSON string'e çevir
        public static string Serialize<T>(T obj)
        {
            return JsonSerializer.Serialize(obj, DefaultOptions);
        }

        // JSON string'i object'e çevir
        public static T? Deserialize<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return default;

            try
            {
                return JsonSerializer.Deserialize<T>(json, DefaultOptions);
            }
            catch
            {
                return default;
            }
        }

        // Güvenli deserialization
        public static bool TryDeserialize<T>(string json, out T? result)
        {
            result = default;

            if (string.IsNullOrWhiteSpace(json))
                return false;

            try
            {
                result = JsonSerializer.Deserialize<T>(json, DefaultOptions);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
