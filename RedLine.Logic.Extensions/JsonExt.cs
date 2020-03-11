using RedLine.Logic.Json;
using System.IO;

namespace RedLine.Logic.Extensions
{
	public static class JsonExt
	{
		public static JsonValue FromJSON(this string json)
		{
			return JsonValue.Load(new StringReader(json));
		}

		public static string ToJSON<T>(this T instance)
		{
			return JsonValue.ToJsonValue(instance);
		}
	}
}
