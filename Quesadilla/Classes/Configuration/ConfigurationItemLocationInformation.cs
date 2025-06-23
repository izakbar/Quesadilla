using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.Configuration
{
	public class ConfigurationItemLocationInformation
	{
		public string Name { get; set; }
		[DisplayName("Location inside window")]
		public Point Location { get; set; }
		[DisplayName("Size")]
		public Size Size { get; set; }
		[DisplayName("Display Colour")]
		public Color Color { get; set; }
		[DisplayName("Display Background Colour")]
		public Color BackColor { get; set; }
		[DisplayName("Alternate Display Colour")]
		public Color AlternateColor { get; set; }
		[DisplayName("Visible")]
		public bool IsVisible { get; set; }
		[DisplayName("Font")]
		public Font Font { get; set; }
	}
}
