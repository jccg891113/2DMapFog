using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FogAbout
{
	/// <summary>
	/// 迷雾遮罩图数据修改记录
	/// </summary>
	public class FogMaskFixGroupY
	{
		#region Pool
		private static Queue<FogMaskFixGroupY> _poolY = new Queue<FogMaskFixGroupY> ();
		public static FogMaskFixGroupY Get (int y, Color color)
		{
			if (_poolY.Count > 0) {
				FogMaskFixGroupY tmp = _poolY.Dequeue ();
				tmp._SetYColor (y, color);
				return tmp;
			} else {
				return new FogMaskFixGroupY (y, color);
			}
		}
		public static void Recover (FogMaskFixGroupY item)
		{
			item.colorArray.Clear ();
			_poolY.Enqueue (item);
		}
		#endregion

		/// <summary>
		/// 被修改数据起始的y值
		/// </summary>
		public int beginY;
		/// <summary>
		/// 被修改数据终止的y值
		/// </summary>
		public int endY;

		/// <summary>
		/// 连续被修改数据的修改后的值
		/// </summary>
		public List<Color> colorArray;

		private FogMaskFixGroupY (int y, Color color)
		{
			colorArray = new List<Color> ();
			_SetYColor (y, color);
		}

		private void _SetYColor (int y, Color color)
		{
			beginY = y;
			endY = y;
			colorArray.Clear ();
			colorArray.Add (color);
		}

		public void AddAfter (Color color)
		{
			colorArray.Add (color);
		}
	}
}