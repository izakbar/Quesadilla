using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Quesadilla.Classes.EVEOPreview
{
	public class EveOEnvelope
	{
		public EveOEnvelope(Guid RequestId, string Type, JsonElement Payload)
		{
			this.RequestId = RequestId;
			this.Type = Type;
			this.Payload = Payload;
		}

		public Guid RequestId { get; set; }
		public string Type { get; set; }
		public JsonElement Payload { get; set; }
	}
}
