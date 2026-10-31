using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.EVEOPreview
{
	public class EveOAgression
	{
		public const string MessageType = "ClientAgression";
		public string Client { get; set; }
		public bool Agression { get; set; }
	}
}
