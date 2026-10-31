using System;
using System.Collections;
using System.Collections.Generic;

namespace Quesadilla.Classes
{
    public sealed class HeapPriorityQueue<T> : IPriorityQueue<T>, IEnumerable<T>, IEnumerable where T : PriorityQueueNode
    {
        private int _numNodes;
        private T[] _nodes;
        private long _numNodesEverEnqueued;

        public HeapPriorityQueue(int maxNodes)
        {
            this._numNodes = 0;
            this._nodes = new T[maxNodes + 1];
            this._numNodesEverEnqueued = 0L;
        }

        public int Count => this._numNodes;

        public int MaxSize => this._nodes.Length - 1;

        public void Clear()
        {
            Array.Clear((Array)this._nodes, 1, this._numNodes);
            this._numNodes = 0;
        }

        public bool Contains(T node)
        {
			return node.QueueIndex > 0 &&
				   node.QueueIndex <= _numNodes &&
				   ReferenceEquals(_nodes[node.QueueIndex], node);
        } 

        public void Enqueue(T node, double priority)
        {
            node.Priority = priority;
            ++this._numNodes;
            this._nodes[this._numNodes] = node;
            node.QueueIndex = this._numNodes;
            node.InsertionIndex = this._numNodesEverEnqueued++;
            this.CascadeUp(this._nodes[this._numNodes]);
        }

        private void Swap(T node1, T node2)
        {
            this._nodes[node1.QueueIndex] = node2;
            this._nodes[node2.QueueIndex] = node1;
            int queueIndex = node1.QueueIndex;
            node1.QueueIndex = node2.QueueIndex;
            node2.QueueIndex = queueIndex;
        }

        private void CascadeUp(T node)
        {
            for (int index = node.QueueIndex / 2; index >= 1; index = node.QueueIndex / 2)
            {
                T node1 = this._nodes[index];
                if (this.HasHigherPriority(node1, node))
                    break;
                this.Swap(node, node1);
            }
        }

        private void CascadeDown(T node)
        {
            int index1 = node.QueueIndex;
            while (true)
            {
                T lower = node;
                int index2 = 2 * index1;
                if (index2 <= this._numNodes)
                {
                    T node1 = this._nodes[index2];
                    if (this.HasHigherPriority(node1, lower))
                        lower = node1;
                    int index3 = index2 + 1;
                    if (index3 <= this._numNodes)
                    {
                        T node2 = this._nodes[index3];
                        if (this.HasHigherPriority(node2, lower))
                            lower = node2;
                    }
                    if ((object)lower != (object)node)
                    {
                        this._nodes[index1] = lower;
                        int queueIndex = lower.QueueIndex;
                        lower.QueueIndex = index1;
                        index1 = queueIndex;
                    }
                    else
                        goto label_10;
                }
                else
                    break;
            }
            node.QueueIndex = index1;
            this._nodes[index1] = node;
            return;
        label_10:
            node.QueueIndex = index1;
            this._nodes[index1] = node;
        }

        private bool HasHigherPriority(T higher, T lower)
        {
			return higher.Priority < lower.Priority ||
				   (higher.Priority == lower.Priority &&
					higher.InsertionIndex < lower.InsertionIndex);
		}

        public T Dequeue()
        {
			if (_numNodes == 0)
			{
				throw new InvalidOperationException(
					"Queue is empty");
			}

			T node = this._nodes[1];
            this.Remove(node);
            return node;
        }

        public void Resize(int maxNodes)
        {

			if (maxNodes < _numNodes)
			{
				throw new InvalidOperationException(
					"Cannot resize below current node count.");
			}

			T[] objArray = new T[maxNodes + 1];
            int num = Math.Min(maxNodes, this._numNodes);
            for (int index = 1; index <= num; ++index)
                objArray[index] = this._nodes[index];
            this._nodes = objArray;
        }

        public T First
		{
			get
			{
				if (_numNodes == 0)
					throw new InvalidOperationException();

				return _nodes[1];
			}
		}

		public void UpdatePriority(T node, double priority)
        {
            node.Priority = priority;
            this.OnNodeUpdated(node);
        }

        private void OnNodeUpdated(T node)
        {
            int index = node.QueueIndex / 2;
            T node1 = this._nodes[index];
            if (index > 0 && this.HasHigherPriority(node, node1))
                this.CascadeUp(node);
            else
                this.CascadeDown(node);
        }

        public void Remove(T node)
        {
            if (this._numNodes <= 1)
            {
                this._nodes[1] = default(T);
                this._numNodes = 0;
            }
            else
            {
                bool flag = false;
                T node1 = this._nodes[this._numNodes];
                if (node.QueueIndex != this._numNodes)
                {
                    this.Swap(node, node1);
                    flag = true;
                }
                --this._numNodes;
                this._nodes[node.QueueIndex] = default(T);
                if (!flag)
                    return;
                this.OnNodeUpdated(node1);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 1; i <= this._numNodes; ++i)
                yield return this._nodes[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => (IEnumerator)this.GetEnumerator();

        public bool IsValidQueue()
        {
            for (int index1 = 1; index1 < this._nodes.Length; ++index1)
            {
                if ((object)this._nodes[index1] != null)
                {
                    int index2 = 2 * index1;
                    if (index2 < this._nodes.Length && (object)this._nodes[index2] != null && this.HasHigherPriority(this._nodes[index2], this._nodes[index1]))
                        return false;
                    int index3 = index2 + 1;
                    if (index3 < this._nodes.Length && (object)this._nodes[index3] != null && this.HasHigherPriority(this._nodes[index3], this._nodes[index1]))
                        return false;
                }
            }
            return true;
        }
    }
}
