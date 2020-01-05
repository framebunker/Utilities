namespace framebunker
{
	public static class Math
	{
		/// <summary>
		/// Calculate a combined hash code from multiple parts
		/// </summary>
		public static int GetHashCode (params object[] parts)
		{
			const int
				magicBase = 47,
				magicStep = 23;

			unchecked
			{
				int hash = magicBase;

				for (int index = 0; index < parts.Length; ++index)
				{
					object part = parts[index];

					if (null == part)
					{
						continue;
					}

					hash = hash * magicStep + part.GetHashCode ();
				}

				return hash;
			}
		}
	}
}
