using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.Processes
{
    public class ProcessInfo
    {
        public IntPtr Handle { get; }
        public string Title { get; }
        public ProcessInfo(IntPtr handle, string title)
        {
            this.Handle = handle;
            this.Title = title;
        }
    }
}
