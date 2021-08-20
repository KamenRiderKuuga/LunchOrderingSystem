using System.Text.Json;

namespace LunchOrderingSystem.Server
{
    public static class Utils
    {
        public static T DeepClone<T>(this T source)
        {
            if (ReferenceEquals(source, null)) return default;

            return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(source));
        }
    }
}
