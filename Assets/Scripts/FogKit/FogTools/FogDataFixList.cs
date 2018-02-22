using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FogAbout
{
	public struct FogDataFixItem
	{
		public int x;
		public int y;
		public bool transparent;

		public void Reset (int x, int y, bool transparent)
		{
			this.x = x;
			this.y = y;
			this.transparent = transparent;
		}
	}

	public class FogDataFixList : FogFixListBase<FogDataFixItem>
	{
		public void Add (int x, int y, bool transparent)
		{
			BeforeAdd ();
			buffer [size++].Reset (x, y, transparent);
		}
	}
}