using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Restaurant_Manager.Utils;

// (EN) Session extensions for storing objects (JSON) | (BG) Разширения за сесията за запазване на обекти (JSON)
public static class SessionExtensions
{
    public static void SetObjectAsJson(this ISession session, string key, object value)
    {
        session.SetString(key, JsonConvert.SerializeObject(value));
    }

    public static T GetObjectFromJson<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
    }
}
