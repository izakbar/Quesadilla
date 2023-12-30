using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes
{
    public interface IPriorityQueue<T> : IEnumerable<T>, IEnumerable where T : PriorityQueueNode
    {
        void Remove(T node);

        void UpdatePriority(T node, double priority);

        void Enqueue(T node, double priority);

        T Dequeue();

        T First { get; }

        int Count { get; }

        int MaxSize { get; }

        void Clear();

        bool Contains(T node);

        void Resize(int maxNodes);
    }
}
