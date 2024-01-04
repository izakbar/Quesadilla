using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes
{
    public class PriorityQueueNode
    {
        public double Priority { get; set; }

        public long InsertionIndex { get; set; }

        public int QueueIndex { get; set; }
    }
}