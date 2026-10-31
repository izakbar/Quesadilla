using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.Configuration
{
	public enum MapType
	{
		[Display(Name = "Use SDE XYZ")]
		SDE,
		[Display(Name = "use SDE 2D")]
		SDE2D,
		[Display(Name = "Dotlan layout")]
		Dotlan,
	}
}
