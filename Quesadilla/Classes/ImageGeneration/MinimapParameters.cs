using Quesadilla.Classes.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.ImageGeneration
{
	public class MinimapParameters
	{
		// Storm Data
		public int StormCentreOutlineAlpha { get; set; }
		public int StormStrongOutlineAlpha { get; set; }
		public int StormWeakOutlineAlpha { get; set; }
		public int StormMainAlpha { get; set; }
		public bool ShowStorm { get; set; }
		public int StormSize { get; set; }
		public int StormGlowLayers { get; set; }

		// Sec limit data
		public float SecLimitValue { get; set; }
		public int SecLimitAlphaAmount { get; set; }
		public int SecLimitCloudSize { get; set; }
		public bool SecLimitShow { get; set; }
		public Color SecLimitColour { get; set; }
		public Color SecLimitOutlineColour { get; set; }

		// Incursion
		public int IncursionAlphaAmount { get; set; }
		public int IncursionCloudSize { get; set; }
		public Color IncursionColour { get; set; }
		public Color IncursionOutlineColour { get; set; }	
		public bool IncursionShow { get; set; }

		// General Colours
		public Color BorderColor { get; set; }
		public Color LineColourNormal { get; set; }
		public Color LineColourOutOfRegion { get; set; }
		public Color LineColourRoute { get; set; }
		public Color SystemColourNormal { get; set; }
		public Color SystemColourRoute { get; set; }
		public Color SystemColourFrom { get; set; }
		public Color SystemColourTo { get; set; }
		public Color SystemColourText { get; set; }
		public Color SystemColourAllText { get; set; }
		public Color SystemColourConnectingText { get; set; }
		public Color ZKillColour { get; set; }
		public Color IntelColour { get; set; }
		public Color AltColour { get; set; }
		public Color UniHoleColour { get; set; }
		public Color IceColour { get; set; }
		public Color MercoxitColour { get; set; }

		public int CloseSystems { get; set; }
		public MapType MapType { get; set; }
		public bool ShowSystemText { get; set; }
		public bool ShowAllSystemsText { get; set; }
		public bool ShowConnectingSystemsText { get; set; }
		public bool ShowBorder { get; set; }

		public bool DefaultTextSize { get; set; }
		public bool ShowOutOfRegion { get; set; }
	}
}
