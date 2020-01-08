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
