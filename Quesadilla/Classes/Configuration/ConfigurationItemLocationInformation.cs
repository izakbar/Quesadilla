using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Quesadilla.Classes.Configuration
{
	public class ConfigurationItemLocationInformation
	{
		[DisplayName("Name")]
		[ReadOnly(true)]
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

		private Font _font;

		[JsonIgnore]
		public Font Font
		{
			get 
			{
				if (_font != null)
					return _font;
				_font = new Font(FontFamily, FontSize, FontStyle);
				return _font;
			}
			set
			{
				FontFamily = value.FontFamily.Name;
				FontSize = value.Size;
				FontStyle = value.Style;
				_font?.Dispose();
				_font = null;
			}
		}

		public string FontFamily { get; set; }
		public float FontSize { get; set; }
		public FontStyle FontStyle { get; set; }
	}
}
