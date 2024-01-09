using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.Resources
{
    public class QuesadillaSound
    {
        public string SoundName {  get; set; }
        public UnmanagedMemoryStream stream {  get; set; }
    }
}
