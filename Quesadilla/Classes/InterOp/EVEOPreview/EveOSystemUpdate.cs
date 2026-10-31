using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.EVEOPreview
{
	public class EveOSystemUpdate
	{
		public const string MessageType = "ClientSystemUpdate";

		public string Client { get; set; }
		public string SystemName { get; set; }
	}
}
