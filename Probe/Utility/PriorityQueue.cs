using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Probe.Utility
{
    /// <summary>
    /// Provides queued of elements by priority.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PriorityQueue<T>
    {
        private readonly SortedDictionary<int, Queue<T>> priority2Queues = new SortedDictionary<int, Queue<T>>();

        /// <summary>
        /// Gets the count.
        /// </summary>
        public long Count { get; private set; }

        /// <summary>
        /// Enqueues the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="priority">The priority.</param>
        public void Enqueue(T item, int priority = 0)
        {
            if (!priority2Queues.ContainsKey(priority)) priority2Queues.Add(priority, new Queue<T>());

            priority2Queues[priority].Enqueue(item);

            Count++;
        }

        /// <summary>
        /// Dequeues next item instance. The most priority items first.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws when queue is empty.</exception>
        /// <returns>Queued element.</returns>
        public T Dequeue()
        {
            if (Count > 0)
            {
                var keys = priority2Queues.Keys.ToList();

                for (var i = keys.Count - 1; i >= 0; i--)
                {
                    if (priority2Queues[keys[i]].Count > 0)
                    {
                        Count--;

                        return priority2Queues[keys[i]].Dequeue();
                    }
                }
            }

            throw new InvalidOperationException("Queue is empty");
        }

        /// <summary>
        /// Removes all elements.
        /// </summary>
        public void Clear()
        {
            Count = 0;
            priority2Queues.Clear();
        }
    }
}
