using System;
using System.Collections;
using System.Threading;
using System.Collections.Generic; // For PooledList only


namespace framebunker
{
	/// <summary>
	/// General pool interface, to be implemented by types responsible for releasing <see cref="PoolItem"/> entries
	/// </summary>
	public interface IPool
	{
		void Release ([CanBeNull] object instance);
	}


	/// <summary>
	/// Represents item to be retained from an <see cref="IPool"/> and released back into it
	/// </summary>
	public abstract class PoolItem : IDisposable
	{
		internal int Live = 0;


		/// <summary>
		/// The <see cref="IPool"/> responsible for this <see cref="PoolItem"/>
		/// </summary>
		[NotNull] public IPool Pool { get; private set; }


		protected PoolItem ([NotNull] IPool pool)
		{
			Pool = pool;
		}


		/// <summary>
		/// Release this item back into <see cref="Pool"/>
		/// </summary>
		public void Release ()
		{
			if (Pool == null || Live == 0)
			{
				return;
			}

			Pool.Release (this);
		}


		/// <summary>
		/// Release this item back into <see cref="Pool"/>
		/// </summary>
		public virtual void Dispose ()
		{
			Release ();
		}


		/// <summary>
		/// Handler invoked when this item was just retained from <see cref="Pool"/>
		/// </summary>
		/// <param name="reused">Indicates whether the item was newly created or reused from <see cref="Pool"/></param>
		public virtual void OnRetained (bool reused) {}


		/// <summary>
		/// Handler invoked when this item is released back into <see cref="Pool"/> (or fully released if it did not fit)
		/// </summary>
		public virtual void OnRelease () {}
	}


	/// <summary>
	/// <see cref="PoolItem"/> wrapper for types not able to implement it directly
	/// </summary>
	public class PoolItem<T> : PoolItem
	{
		/// <summary>
		/// The wrapped value
		/// </summary>
		[NotNull] public T Value { get; protected set; }


		public PoolItem (IPool owner) : base (owner)
		{}


		/// <summary>
		/// Set the wrapped value and return the wrapper instance
		/// </summary>
		/// <returns>This wrapper instance</returns>
		public PoolItem<T> Initialize ([NotNull] T value)
		{
			Value = value;

			return this;
		}


		/// <summary>
		/// Resets the wrapped value to default (<see cref="T"/>)
		/// </summary>
		public override void OnRelease ()
		{
			Value = default (T);
		}
	}


	/// <summary>
	/// <see cref="PoolItem"/> wrapper for an <see cref="IList"/>
	/// </summary>
	public class PooledIList<TList> : PoolItem<TList>
		where TList : IList
	{
		public PooledIList ([NotNull] IPool owner) : base (owner)
		{
			Value = Activator.CreateInstance<TList> ();
		}


		/// <summary>
		/// Clears the wrapped IList
		/// </summary>
		public override void OnRelease ()
		{
			Value.Clear ();
		}
	}


	/// <summary>
	/// <see cref="PoolItem"/> wrapper for a <see cref="List&lt;T&gt;"/>
	/// </summary>
	public class PooledList<TItem> : PooledIList<List<TItem>>
	{
		public PooledList ([NotNull] IPool owner) : base (owner)
		{}
	}


	/// <summary>
	/// <see cref="Pool&lt;T&gt;"/> of <see cref="PooledIList&lt;T&gt;"/> entries
	/// </summary>
	public class IListPool<T> : Pool<PooledIList<T>> where T : IList
	{
		public IListPool (int size, [NotNull] Func<Pool<PooledIList<T>>, PooledIList<T>> itemConstructor) : base (size, itemConstructor)
		{}
	}


	/// <summary>
	/// <see cref="Pool&lt;T&gt;"/> of <see cref="PooledList&lt;T&gt;"/> entries
	/// </summary>
	public class ListPool<T> : Pool<PooledList<T>> where T : class
	{
		public ListPool (int size, [NotNull] Func<Pool<PooledList<T>>, PooledList<T>> itemConstructor) : base (size, itemConstructor)
		{}
	}


	/// <summary>
	/// Default <see cref="IPool"/> implementation wrapping a fixed-size, lock-free multi-threading-compatible buffer
	/// </summary>
	public class Pool<T> : IPool where T : class
	{
		private class RestrainedPool : Pool<T>
		{
			// Additional buffer space to combat losing references when mass-releasing in heavily threaded environment - apologies for magic number
			private const int kAsyncSizeBuffer = 10;


			protected override T RetainDefault { get { return null; } }


			public RestrainedPool (int size, [NotNull] Func<Pool<T>, T> itemConstructor) : base (size + kAsyncSizeBuffer, itemConstructor)
			{
				Allocate (size);
			}
		}


		/// <summary>
		/// Constructs a restrained <see cref="Pool&lt;T&gt;"/> - preallocating <see cref="size"/> entries and not auto-creating more on <see cref="Retain"/>
		/// </summary>
		/// <param name="size">The maximum number of entries in the pool</param>
		/// <param name="itemConstructor">Constructor for new pool entries</param>
		/// <returns></returns>
		public static Pool<T> Restrained (int size, [NotNull] Func<Pool<T>, T> itemConstructor)
		{
			return new RestrainedPool (size, itemConstructor);
		}


		private readonly int m_Size;
		[NotNull] private readonly T[] m_Pool;
		[NotNull] private readonly Func<Pool<T>, T> m_ItemConstructor;


		/// <summary>
		/// The fallback value if Retain is unable to locate an existing item in the pool
		/// </summary>
		protected virtual T RetainDefault { get { return m_ItemConstructor (this); } }


		/// <summary>
		/// Create a new pool
		/// </summary>
		/// <param name="size">The maximum number of entries in the pool</param>
		/// <param name="itemConstructor">Constructor for new pool entries</param>
		public Pool (int size, [NotNull] Func<Pool<T>, T> itemConstructor)
		{
			m_Size = size;
			m_Pool = new T[size];
			m_ItemConstructor = itemConstructor;
		}


		/// <summary>
		/// The fixed size of the pool
		/// </summary>
		public int Size { get { return m_Size; } }


		/// <summary>
		/// Try to add up to <see cref="count"/> or <see cref="Pool&lt;T&gt;"/> size items
		/// </summary>
		/// <returns>The number of items actually added</returns>
		public int Allocate (int count)
		{
			count = count <= m_Size ? count : m_Size;

			int allocated;
			for (allocated = 0; allocated < count && Add (m_ItemConstructor (this)); ++allocated)
			{}

			return allocated;
		}


		/// <summary>
		/// Get the first entry matching the <paramref name="match"/> predicate (null predicate matching anything), optionally starting at a given <paramref name="offset"/>, optionally popping the entry from the pool
		/// </summary>
		/// <param name="match">The predicate indicating a valid entry (null value meaning anything is valid)</param>
		/// <param name="pop">Whether the found entry should be popped from the pool</param>
		/// <param name="offset">The offset in the pool from which the search should start</param>
		/// <returns>The found entry</returns>
		[CanBeNull] public T Find ([CanBeNull] Predicate<T> match, bool pop = false, int offset = 0)
		{
			T instance = null;
			offset %= m_Size;

			for (int index = 0; index < m_Size; ++index)
			{
				instance = m_Pool[(index + offset) % m_Size];

				if (instance != null && (match == null || match (instance)))
				{
					if (!pop || Interlocked.CompareExchange (ref m_Pool[index], null, instance) == instance)
					{
						break;
					}

					instance = null;
				}
			}

			return instance;
		}


		/// <summary>
		/// Perform an <see cref="Action&lt;T&gt;"/> with each entry in the pool
		/// </summary>
		public void Foreach ([NotNull] Action<T> action)
		{
			for (int index = 0; index < m_Size; ++index)
			{
				T instance = m_Pool[index];

				if (instance != null)
				{
					action (instance);
				}
			}
		}


		/// <summary>
		/// Insert an entry into the first available spot in the pool (adding null fails)
		/// </summary>
		/// <returns>Whether the entry was non-null, fit in the pool, and was added</returns>
		public bool Add ([CanBeNull] T instance)
		{
			if (instance == null)
			{
				return false;
			}

			for (int index = 0; index < m_Size; ++index)
			{
				if (Interlocked.CompareExchange (ref m_Pool[index], instance, null) == null)
				{
					return true;
				}
			}

			return false;
		}


		/// <summary>
		/// Extract the first available entry from the pool or (unless restrained) a new one
		/// </summary>
		[CanBeNull] public T Retain ()
		{
			T instance = null;

			for (int index = 0; index < m_Size; ++index)
			{
				instance = m_Pool[index];

				if (instance == null)
				{
					continue;
				}

				if (Interlocked.CompareExchange (ref m_Pool[index], null, instance) == instance)
				{
					break;
				}

				instance = null;
			}

			bool reuse = instance != null;
			if (!reuse)
			{
				instance = RetainDefault;
			}

			// Special treatment of PoolItem - marking as live, issuing callback
			PoolItem poolItem = instance as PoolItem;
			if (poolItem != null)
			{
				poolItem.Live = 1;
				poolItem.OnRetained (reuse);
			}

			return instance;
		}


		/// <summary>
		/// Release pool item, adding it the pool if it fits (releasing null fails)
		/// </summary>
		/// <returns>Whether the released item was added to the pool</returns>
		public bool Release ([CanBeNull] T instance)
		{
			if (instance == null)
			{
				return false;
			}

			// Special treatment of PoolItem - taking special care to not double-release, issuing callback
			PoolItem poolItem = instance as PoolItem;
			if (poolItem != null)
			{
				if (0 == Interlocked.CompareExchange (ref poolItem.Live, 0, 1))
				// If we were already not live, we should do nothing here
				{
					return true;
				}

				poolItem.OnRelease ();
			}

			return Add (instance);
		}


		void IPool.Release ([CanBeNull] object instance)
		{
			Release (instance as T);
		}
	}
}
