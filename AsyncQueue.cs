/*
 *
Copyright 2019 framebunker

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
documentation files (the "Software"), to deal in the Software without restriction, including without limitation the
rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit
persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the
Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 */


using System.Threading;


namespace framebunker
{
	/// <summary>
	/// Multi-producer, single-consumer lock-free. Read lock depends on choice of inherited type <see cref="SingleConsumerQueue&lt;T&gt;"/> or <see cref="ReadLockQueue&lt;T&gt;"/>.
	/// </summary>
	/// <typeparam name="T">Item type</typeparam>
	internal abstract class AsyncQueue<T> where T : class
	{
		private readonly int m_Size;
		private readonly T[] m_Items;
		private int m_LastRead, m_LastWrite;


		protected AsyncQueue (int size)
		{
			m_Size = size;
			m_Items = new T[size];
		}


		[CanBeNull] protected T UnsafePop ()
		{
			if (m_LastRead >= m_LastWrite)
			{
				return null;
			}

			int newRead = m_LastRead + 1;
			int effectiveIndex = newRead % m_Size;

			T item = m_Items[effectiveIndex];
			m_Items[effectiveIndex] = null;

			// If this reads runs between m_LastWrite being updated and an actual item being written, this read should fail
			if (item != null)
			{
				m_LastRead = newRead;
			}

			return item;

		}


		/// <summary>
		/// Dequeue the next item.
		/// </summary>
		/// <returns>The next element or null if empty.</returns>
		[CanBeNull] public abstract T Dequeue ();


		/// <summary>
		/// Add an item to the end of the queue.
		/// </summary>
		/// <param name="item">The item to add.</param>
		/// <returns>true if the item was added</returns>
		/// <remarks>Enqueuing null values will always succeed, but they can never be dequeued.</remarks>
		public bool Enqueue ([NotNull] T item)
		{
			if (item == null)
			{
				return true;
			}

			int newWrite, effectiveIndex, maxEffectiveIndex = m_LastRead % m_Size;
			do
			{
				newWrite = m_LastWrite + 1;
				effectiveIndex = newWrite % m_Size;

				if (effectiveIndex == maxEffectiveIndex)
				{
					return false;
				}
			}
			while (Interlocked.CompareExchange (ref m_LastWrite, newWrite, newWrite - 1) != newWrite - 1);

			// A read could happen between setting m_LastWrite above and assigning the value here, so read needs to verify non-null
			m_Items[effectiveIndex] = item;

			return true;
		}
	}


	/// <summary>
	/// Multi-produce, single-consumer, lock-free queue of fixed size.
	/// </summary>
	/// <typeparam name="T">Item type</typeparam>
	internal class SingleConsumerQueue<T> : AsyncQueue<T> where T : class
	{
		/// <summary>
		/// Allocate a new queue.
		/// </summary>
		/// <param name="size">The maximum size of the queue.</param>
		public SingleConsumerQueue (int size) : base (size)
		{}


		/// <summary>
		/// Dequeue the next item.
		/// </summary>
		/// <returns>The next element or null if empty.</returns>
		[CanBeNull] public override T Dequeue ()
		{
			return UnsafePop ();
		}
	}


	/// <summary>
	/// Multi-producer, multi-consumer, of fixed size, locking on <see cref="Dequeue"/>.
	/// </summary>
	/// <typeparam name="T">Item type</typeparam>
	internal class ReadLockQueue<T> : AsyncQueue<T> where T : class
	{
		private readonly object m_ReadLock = new object ();


		/// <summary>
		/// Allocate a new queue.
		/// </summary>
		/// <param name="size">The maximum size of the queue.</param>
		public ReadLockQueue (int size) : base (size)
		{}


		/// <summary>
		/// Dequeue the next item (locks).
		/// </summary>
		/// <returns>The next element or null if empty.</returns>
		[CanBeNull] public override T Dequeue ()
		{
			lock (m_ReadLock)
			{
				return UnsafePop ();
			}
		}
	}
}
