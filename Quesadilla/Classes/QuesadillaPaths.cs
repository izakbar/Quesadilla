using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes
{
	internal static class QuesadillaPaths
	{
		public static readonly string DataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Quesadilla");

		public static readonly string LogDirectory = Path.Combine(DataDirectory, "Logs");

		public static readonly string LocationDirectory = Path.Combine(DataDirectory, "Locations");
		public static readonly string SoundDirectory = Path.Combine(DataDirectory, "Sounds");
		public static readonly string ImageDirectory = Path.Combine(DataDirectory, "Images");

		public static readonly string ExportDirectory = Path.Combine(DataDirectory, "Exports");
		
		public static readonly string ExternalDownloadDirectory = Path.Combine(DataDirectory, "External");

		public static readonly string ConfigurationFile = Path.Combine(DataDirectory, "Quesadilla.config.json");

		public static readonly string CharacterCacheFile = Path.Combine(DataDirectory, "Quesadilla.CharacterCache.Json");

		public static readonly string LogFile = Path.Combine(LogDirectory, "Quesadilla-.log");

		public static void Initialise()
		{
			Directory.CreateDirectory(DataDirectory);
			Directory.CreateDirectory(LogDirectory);
			Directory.CreateDirectory(LocationDirectory);
			Directory.CreateDirectory(ExportDirectory);
			Directory.CreateDirectory(ExternalDownloadDirectory);
		}
	}
}
