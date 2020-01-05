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
	public static class CollectionExtensions
	{
		/// <summary>
		/// Remove and return the last element of the list
		/// </summary>
		/// <returns>The former last element of the list if available - otherwise default (T)</returns>
		[CanBeNull] public static T Pop<T> ([NotNull] this List<T> list)
		{
			if (null == list || list.Count < 1)
			{
				return default (T);
			}

			T element;
			if (list.Count == 1)
			{
				element = list[0];
				list.Clear ();
			}
			else
			{
				element = list[list.Count - 1];
				list.RemoveAt (list.Count - 1);
			}

			return element;
		}


		/// <summary>
		/// Insert <paramref name="entry"/> into an already sorted list, maintaining the sort
		/// </summary>
		/// <returns>The index of <paramref name="entry"/> in the list</returns>
		public static int InsertIntoPreSorted<T> ([NotNull] this List<T> list, T entry)
		{
			int index = list.BinarySearch (entry);
			if (index < 0)
			{
				index = ~index;
			}

			list.Insert (index, entry);

			return index;
		}


		/// <summary>
		/// Remove <paramref name="value"/> from an unsorted list, shuffling the last list element into the previous position of <paramref name="value"/>
		/// </summary>
		/// <returns>Whether <paramref name="value"/> was removed or not</returns>
		public static bool FastRemove<V> ([NotNull] this IList<V> source, V value)
		{
			int idx = source.IndexOf (value);
			if (idx == -1)
			{
				return false;
			}

			FastRemoveAt (source, idx);
			return true;
		}


		/// <summary>
		/// Remove the entry at <paramref name="index"/> from an unsorted list, shuffling the last list element into the <paramref name="index"/> position
		/// </summary>
		public static void FastRemoveAt<V> ([NotNull] this IList<V> source, int index)
		{
			int last = source.Count - 1;
			if (index < last)
			{
				source[index] = source[last];
			}

			source.RemoveAt (last);
		}


		/// <summary>
		/// Remove entries (optionally starting with index <paramref name="startAt"/>) matching <paramref name="test"/> from an unsorted list, shuffling non-matching entries into their place
		/// </summary>
		/// <returns>The number of elements removed</returns>
		public static int FastRemoveAll<V> ([NotNull] this List<V> source, [NotNull] Predicate<V> test, int startAt = 0)
		{
			int finalCount = source.Count;

			if (finalCount < 1)
			{
				return 0;
			}

			if (finalCount < 2)
			{
				if (!test (source[0]))
				{
					return 0;
				}

				source.Clear ();
				return 1;
			}

			for (int index = startAt; index < finalCount;)
			{
				if (!test (source[index]))
				{
					++index;
					continue;
				}

				if (index < --finalCount)
				{
					source[index] = source[finalCount];
				}
			}

			int diff = source.Count - finalCount;
			if (diff > 0)
			{
				source.RemoveRange (finalCount, diff);
			}

			return diff;
		}
	}
}
