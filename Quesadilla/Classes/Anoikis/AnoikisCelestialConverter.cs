using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Quesadilla.Classes.Anoikis
{
	public class AnoikisCelestialConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(AnoikisCelestialDefinition);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JArray array = JArray.Load(reader);

			AnoikisCelestialDefinition result = new AnoikisCelestialDefinition();

			if (array.Count > 0)
				result.GroupID = array[0].Value<int>();

			if (array.Count > 1)
				result.TypeID = array[1].Value<long>();

			if (array.Count > 2)
				result.X = array[2].Value<double>();

			if (array.Count > 3)
				result.Y = array[3].Value<double>();

			if (array.Count > 4)
				result.Z = array[4].Value<double>();

			if (array.Count > 5 && array[5].Type != JTokenType.Null)
				result.CelestialIndex = array[5].Value<int>();

			if (array.Count > 6 && array[6].Type != JTokenType.Null)
				result.MoonCount = array[6].Value<int>();

			if (array.Count > 7 && array[7].Type != JTokenType.Null)
				result.OrbitIndex = array[7].Value<int>();

			if (array.Count > 8 && array[8].Type != JTokenType.Null)
				result.ItemName = array[8].Value<string>();

			return result;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotSupportedException();
		}

		public override bool CanWrite
		{
			get
			{
				return false;
			}
		}
	}
}