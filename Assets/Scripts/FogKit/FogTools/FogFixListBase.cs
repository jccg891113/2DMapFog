using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FogAbout
{
	public abstract class FogFixListBase<T>
	{
		protected int size = 0;
		protected T [] buffer;

		public int Count { get { return size; } }

		protected void BeforeAdd ()
		{
			if (buffer == null || size == buffer.Length) {
				AllocateMore ();
			}
		}

		private void AllocateMore ()
		{
			T [] newList = (buffer != null) ? new T [global::System.Math.Max (buffer.Length << 1, 8)] : new T [8];
			if (buffer != null && size > 0)
				buffer.CopyTo (newList, 0);
			buffer = newList;
		}

		public T this [int i] {
			get { return buffer [i]; }
		}

		public T [] GetArray ()
		{
			return buffer;
		}

		public void Clear ()
		{
			size = 0;
		}
	}
}