using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Quesadilla.Classes.Configuration
{
	public class ColorJsonConverter : JsonConverter<Color>
	{
		public override Color Read(
			ref Utf8JsonReader reader,
			Type typeToConvert,
			JsonSerializerOptions options)
		{
			return ColorTranslator.FromHtml(reader.GetString());
		}

		public override void Write(
			Utf8JsonWriter writer,
			Color value,
			JsonSerializerOptions options)
		{
			writer.WriteStringValue(ColorTranslator.ToHtml(value));
		}
	}
}
