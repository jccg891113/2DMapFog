using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FogAbout
{
	/// <summary>
	/// 迷雾遮罩图数据修改记录集合
	/// </summary>
	public class FogMaskFixGroup
	{
		/// <summary>
		/// 数据修改记录字典，key值对应修改数据的x坐标
		/// </summary>
		public Dictionary<int, FogMaskFixGroupX> fogMaskFixGroupX;

		public FogMaskFixGroupX [] cache;

		const int CACHENUM = 8;

		public FogMaskFixGroup ()
		{
			fogMaskFixGroupX = new Dictionary<int, FogMaskFixGroupX> ();

			cache = new FogMaskFixGroupX [CACHENUM];
		}

		public void Add (int x, int y, Color color)
		{
			int cacheID = x % CACHENUM;
			FogMaskFixGroupX ptr = cache [cacheID];
			if (ptr != null && ptr.X == x) {
				ptr.Add (y, color);
			} else {
				if (!fogMaskFixGroupX.ContainsKey (x)) {
					ptr = FogMaskFixGroupX.Get (x);
					fogMaskFixGroupX [x] = ptr;
				} else {
					ptr = fogMaskFixGroupX [x];
				}
				ptr.Add (y, color);
				cache [cacheID] = ptr;
			}
		}

		/// <summary>
		/// 数据合并
		/// </summary>
		public void DataCombo ()
		{
			foreach (var item in fogMaskFixGroupX) {
				item.Value.Combo ();
			}
		}

		public void Clear ()
		{
			foreach (var item in fogMaskFixGroupX) {
				FogMaskFixGroupX.Recover (item.Value);
			}
			fogMaskFixGroupX.Clear ();
			for (int i = 0; i < CACHENUM; i++) {
				cache [i] = null;
			}
		}
	}
}