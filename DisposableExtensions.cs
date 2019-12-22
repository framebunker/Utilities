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


using System;
using System.Collections.Generic;


namespace framebunker
{
	public static class DisposableExtensions
	{
		/// <summary>
		/// Dispose a given <see cref="IDisposable"/> if it passes the given <paramref name="test"/>
		/// </summary>
		/// <returns>Whether the <see cref="IDisposable"/> was disposed or not</returns>
		public static bool DisposeIf<T> ([NotNull] this T instance, [NotNull] Func<T, bool> test)
			where T : IDisposable
		{
			if (!test (instance))
			{
				return false;
			}

			instance.Dispose ();
			return true;
		}


		/// <summary>
		/// Remove from a given list and dispose all <see cref="IDisposable"/> entries passing the given <paramref name="test"/>
		/// </summary>
		/// <returns>The number of entries disposed and removed from the list</returns>
		public static int RemoveAndDisposeAll<T> ([NotNull] this List<T> list, [NotNull] Predicate<T> test)
			where T : IDisposable
		{
			return list.RemoveAll (item => item.DisposeIf (i => test (i)));
		}


		/// <summary>
		/// Remove from a given unsorted list and dispose all <see cref="IDisposable"/> entries passing the given <paramref name="test"/>, shuffling non-matching entries into their place
		/// </summary>
		/// <returns>The number of entries disposed and removed from the list</returns>
		public static int FastRemoveAndDisposeAll<T> ([NotNull] this List<T> list, [NotNull] Predicate<T> test)
			where T : IDisposable
		{
			return list.FastRemoveAll (item => item.DisposeIf (i => test (i)));
		}


		/// <summary>
		/// Remove and dispose all <see cref="IDisposable"/> entries from a given list
		/// </summary>
		/// <returns>The number of entries disposed and removed from the list</returns>
		public static int RemoveAndDisposeAll<T> ([NotNull] this List<T> list)
			where T : IDisposable
		{
			int size = list.Count;

			foreach (T instance in list)
			{
				if (instance != null)
				{
					instance.Dispose();
				}
			}

			list.Clear ();

			return size;
		}


		/// <summary>
		/// Dispose all <see cref="IDisposable"/> entries in a list and clear it. Direct mapping to <see cref="RemoveAndDisposeAll&lt;T&gt; (List&lt;T&gt;)"/>.
		/// </summary>
		/// <returns>The number of entries disposed and removed from the list</returns>
		public static int ClearAndDisposeAll<T> ([NotNull] this List<T> list)
			where T : IDisposable
		{
			return list.RemoveAndDisposeAll ();
		}


		/// <summary>
		/// Dispose all <see cref="IDisposable"/> entries in a queue and clear it
		/// </summary>
		/// <returns>The number of entries disposed and removed from the queue</returns>
		public static int DequeueAndDisposeAll<T> ([NotNull] this Queue<T> queue)
			where T : IDisposable
		{
			int size = queue.Count;

			for (int index = 0; index < size; ++index)
			{
				T current = queue.Dequeue ();

				if (current == null)
				{
					continue;
				}

				current.Dispose ();
			}

			return size;
		}


		/// <summary>
		/// Dispose all <see cref="IDisposable"/> entries in a queue and clear it. Direct mapping to <see cref="DequeueAndDisposeAll&lt;T&gt; (Queue&lt;T&gt;)"/>.
		/// </summary>
		/// <returns>The number of entries disposed and removed from the queue</returns>
		public static int ClearAndDisposeAll<T> ([NotNull] this Queue<T> queue)
			where T : IDisposable
		{
			return queue.DequeueAndDisposeAll ();
		}


		/// <summary>
		/// Remove and dispose from <paramref name="start"/> index of a given list all <see cref="IDisposable"/> entries
		/// </summary>
		/// <returns>The number of entries disposed and removed from the list</returns>
		public static int RemoveAndDisposeRange<T> ([NotNull] this List<T> list, int start)
			where T : IDisposable
		{
			return list.RemoveAndDisposeRange (start, list.Count - start);
		}


		/// <summary>
		/// Remove and dispose from <paramref name="start"/> index of a given list <paramref name="count"/> <see cref="IDisposable"/> entries if available
		/// </summary>
		/// <returns><paramref name="count"/> or 0 if <paramref name="start"/> + <paramref name="count" /> is out of range</returns>
		public static int RemoveAndDisposeRange<T> ([NotNull] this List<T> list, int start, int count)
			where T : IDisposable
		{
			int end = start + count;

			if (start < 0 || end > list.Count)
			{
				return 0;
			}

			for (int index = start; index < end; ++index)
			{
				list[index].Dispose ();
			}

			list.RemoveRange (start, count);

			return count;
		}


		/// <summary>
		/// Remove and dispose <paramref name="item"/> from a given list
		/// </summary>
		/// <returns>Whether <paramref name="item"/> was removed and disposed from the list</returns>
		public static bool RemoveAndDispose<T> ([NotNull] this List<T> list, T item)
			where T : IDisposable
		{
			bool removed = list.Remove (item);
			item.Dispose ();

			return removed;
		}


		/// <summary>
		/// Remove and dispose <see cref="item"/> from an unsorted list, shuffling the last list element into the previous position of <paramref name="item"/>
		/// </summary>
		/// <returns>Whether <paramref name="item"/> was removed and disposed from the list</returns>
		public static bool FastRemoveAndDispose<T> ([NotNull] this List<T> list, T item)
			where T : IDisposable
		{
			bool removed = list.FastRemove (item);
			item.Dispose ();

			return removed;
		}


		/// <summary>
		/// Remove and dispose the entry at <paramref name="index"/> from a given list
		/// </summary>
		/// <returns>Whether an entry at <paramref name="index"/> was removed and disposed from the list</returns>
		public static bool RemoveAndDisposeAt<T> ([NotNull] this List<T> list, int index)
			where T : IDisposable
		{
			if (index < 0 || index >= list.Count)
			{
				return false;
			}

			list[index].Dispose ();
			list.RemoveAt (index);

			return true;
		}
	}
}
