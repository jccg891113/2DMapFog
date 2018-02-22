using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FogAbout
{
	/// <summary>
	/// 场景整体的迷雾数据
	/// 
	/// 场景整体根据设定的场景尺寸分为 长*宽 个单元方格
	/// 
	/// 设计上，通过ulong类型存储8*8方格的alpha数据，每个方格仅分为透明与不透明两种
	/// 通过ulong的二维数组记录场景整体的迷雾数据
	/// 
	/// 具体小格的数据运算通过移位获取，求逻辑与获取alpha结果
	/// </summary>
	public class FogBaseData
	{
		const ulong ALPHABASE = 0x0000000000000001;
		const ulong TRANSPARENT = 0xffffffffffffffff;

		public const int AlphaArraySize = 8;
		public const int AlphaArraySizeMask = 0x07;
		public const int AlphaArrayBitSize = 3;

		/// <summary>
		/// 地图压缩数据存储数组
		/// </summary>
		public ulong[,] totalAlphaArray;

		/// <summary>
		/// 场景数据的实际宽、高
		/// </summary>
		public int w, h;

		/// <summary>
		/// 场景数据压缩后的宽、高，既ulong二维数组的尺寸
		/// </summary>
		public int array_w, array_h;

		/// <summary>
		/// 视野范围查询字典
		/// </summary>
		Dictionary<int, FogTransparentRange> fogRangeDic;

		/// <summary>
		/// 地图实际数据变更队列
		/// </summary>
		public FogDataFixList fixList;
		/// <summary>
		/// 地图压缩数据变更队列，存储压缩后数据变更过的8*8数据块所在数组位置
		/// </summary>
		public FogDataFixList fixGroupList;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:BabelTime.GD.UI.BattleUISub.FogAbout.FogBaseData"/> class.
		/// </summary>
		/// <param name="w">世界地图数据存储宽度.</param>
		/// <param name="h">世界地图数据存储高度.</param>
		public FogBaseData (int w, int h)
		{
			this.w = w;
			this.h = h;

			array_w = (int) System.Math.Ceiling ((double) w / AlphaArraySize);
			array_h = (int) System.Math.Ceiling ((double) h / AlphaArraySize);

			totalAlphaArray = new ulong[array_w, array_h];

			FTools.FogDebug.LogAllocSize ("[BaseData]", default (ulong), array_w * array_h);

			fogRangeDic = new Dictionary<int, FogTransparentRange> ();
			fixList = new FogDataFixList ();
			fixGroupList = new FogDataFixList ();
		}

		private ulong _AlphaPos (int x, int y)
		{
			return ALPHABASE << ((x << AlphaArrayBitSize) + y);
		}

		#region Set Data Methods

		public void SetTransparent (int x, int y)
		{
			SetTransparent (x, y, x >> AlphaArrayBitSize, y >> AlphaArrayBitSize, x & AlphaArraySizeMask,
							y & AlphaArraySizeMask);
		}

		private void SetTransparent (int worldX, int worldY, int arrayX, int arrayY, int x, int y)
		{
			ulong _tmp = _AlphaPos (x, y);
			if ((totalAlphaArray[arrayX, arrayY] & _tmp) <= 0) {
				totalAlphaArray[arrayX, arrayY] |= _AlphaPos (x, y);
				fixList.Add (worldX, worldY, true);
			}
		}

		private void SetArrayTransparent (int arrayX, int arrayY)
		{
			if (totalAlphaArray[arrayX, arrayY] != TRANSPARENT) {
				fixGroupList.Add (arrayX, arrayY, true);
				totalAlphaArray[arrayX, arrayY] |= TRANSPARENT;
			}
		}

		public void SetOpaque (int x, int y)
		{
			SetOpaque (x >> AlphaArrayBitSize, y >> AlphaArrayBitSize, x & AlphaArraySizeMask, y & AlphaArraySizeMask);
			fixList.Add (x, y, false);
		}

		private void SetOpaque (int arrayX, int arrayY, int x, int y)
		{
			totalAlphaArray[arrayX, arrayY] &= ~_AlphaPos (x, y);
		}

		#endregion

		#region Check Methods

		public bool IsTransparent (int x, int y)
		{
			return IsTransparent (x >> AlphaArrayBitSize, y >> AlphaArrayBitSize, x & AlphaArraySizeMask,
								  y & AlphaArraySizeMask);
		}

		public bool IsTransparent (int arrayX, int arrayY, int subX, int subY)
		{
			if (arrayX >= 0 && arrayX < array_w && arrayY >= 0 && arrayY < array_h) {
				return (totalAlphaArray[arrayX, arrayY] & _AlphaPos (subX, subY)) > 0;
			} else {
				Debug.LogError ("FogBaseData.IsTransparent DATA ERROR!\r\n" +
								string.Format ("w:{0}, x:{1}, h:{2}, y:{3}, sub_x:{4}, sub_y:{5}", array_w, arrayX,
											   array_h, arrayY, subX, subY));
				return false;
			}
		}

		#endregion

		#region Update Range Methods

		public void UpdateTransparent (int x, int y, int r)
		{
			FogTransparentRange rangeData = _GetFogTransparentRange (r);
			/// 先求取视野范围内的有效坐标
			/// 即根据视点坐标求出视野范围内有效用于战场的尺寸范围
			/// 有效坐标奉行左闭右开原则
			//if (x - r >= 0) {
			//	vl = 0;
			//} else {
			//	vl = r - x;
			//}
			/// 视野范围左侧有效值
			int view_l = (x - r >= 0) ? 0 : (r - x);
			//if (x + r < w) {
			//	vr = 2 * r + 1;
			//} else {
			//	vr = r - x + w;
			//}
			/// 视野范围右侧有效值
			int view_r = (x + r < w) ? (2 * r + 1) : (r - x + w);
			//if (y - r >= 0) {
			//	vt = 0;
			//} else {
			//	vt = r - y;
			//}
			/// 视野范围上侧有效值（上侧为y值小的一侧）
			int view_t = (y - r >= 0) ? 0 : (r - y);
			//if (y + r < h) {
			//	vb = 2 * r + 1;
			//} else {
			//	vb = r - y + h;
			//}
			/// 视野范围下侧有效值（下侧为y值大的一侧）
			int view_b = (y + r < h) ? (2 * r + 1) : (r - y + h);
			/// 视野范围最小点x值
			int view_lt_x = x - r;
			/// 视野范围最小点y值
			int view_lt_y = y - r;

			/// 操作九宫格四边 上（y值为0开始）
			_UpdateTransparentTop (rangeData, view_lt_x, view_lt_y, view_l, view_r, view_t, view_b);
			/// 操作九宫格四边 左（x值为0开始）
			_UpdateTransparentLeft (rangeData, view_lt_x, view_lt_y, view_l, view_r, view_t, view_b);
			/// 操作九宫格中心全部为透明区域
			_UpdateTransparentCenter (rangeData, view_lt_x, view_lt_y, view_l, view_r, view_t, view_b);
			/// 操作九宫格四边 右
			_UpdateTransparentRight (rangeData, view_lt_x, view_lt_y, view_l, view_r, view_t, view_b);
			/// 操作九宫格四边 下
			_UpdateTransparentBottom (rangeData, view_lt_x, view_lt_y, view_l, view_r, view_t, view_b);

			/// 忽略九宫格四角操作
		}

		private FogTransparentRange _GetFogTransparentRange (int r)
		{
			FogTransparentRange rangeData = null;
			if (!fogRangeDic.ContainsKey (r)) {
				rangeData = new FogTransparentRange (r);
				fogRangeDic[r] = rangeData;
			} else {
				rangeData = fogRangeDic[r];
			}
			return rangeData;
		}

		private void _UpdateTransparentTop (FogTransparentRange rangeData, int view_lt_x, int view_lt_y, int view_left,
											int view_right, int view_top, int view_bottom)
		{
			int xMin = Max (view_left, rangeData.l);
			int xMax = Min (view_right, rangeData.r + 1);
			int yMin = Max (view_top, 0);
			int yMax = Min (view_bottom, rangeData.t + 1);
			_UpdateTransparentCustom (rangeData, view_lt_x, view_lt_y, xMin, xMax, yMin, yMax);
		}

		private void _UpdateTransparentCustom (FogTransparentRange rangeData, int view_lt_x, int view_lt_y, int xMin,
											   int xMax, int yMin, int yMax)
		{
			if (xMin < xMax && yMin < yMax) {
				for (int i = xMin, j = 0; i < xMax; i++) {
					for (j = yMin; j < yMax; j++) {
						if (rangeData.array[i, j]) {
							SetTransparent (view_lt_x + i, view_lt_y + j);
						}
					}
				}
			}
		}

		private void _UpdateTransparentLeft (FogTransparentRange rangeData, int view_lt_x, int view_lt_y, int view_left,
											 int view_right, int view_top, int view_bottom)
		{
			int xMin = Max (view_left, 0);
			int xMax = Min (view_right, rangeData.l + 1);
			int yMin = Max (view_top, rangeData.t);
			int yMax = Min (view_bottom, rangeData.b + 1);
			_UpdateTransparentCustom (rangeData, view_lt_x, view_lt_y, xMin, xMax, yMin, yMax);
		}

		private void _UpdateTransparentCenter (FogTransparentRange rangeData, int view_lt_x, int view_lt_y,
											   int view_left, int view_right, int view_top, int view_bottom)
		{
			int xMin = Max (view_left, rangeData.l) + view_lt_x;
			int xMax = Min (view_right, rangeData.r + 1) + view_lt_x;
			int yMin = Max (view_top, rangeData.t) + view_lt_y;
			int yMax = Min (view_bottom, rangeData.b + 1) + view_lt_y;
			if (xMin < xMax && yMin < yMax) {
				/// 旧方案，使用遍历依次对各点设置alpha为0
				//for (int i = xMin, j = 0; i < xMax; i++) {
				//	for (j = yMin; j < yMax; j++) {
				//		SetTransparent (view_lt_x + i, view_lt_y + j);
				//	}
				//}

				/// 新方案，计算当前区域具体方格数据，减少遍历次数，提高设置alpha的效率
				/// 求出方形透明区域内的几个临界值，然后采用for循环与位运算结合求值
				int _left_array_x = xMin >> AlphaArrayBitSize;
				int _left_bit_x = xMin & AlphaArraySizeMask;
				int _right_array_x = (xMax - 1) >> AlphaArrayBitSize;
				int _right_bit_x = (xMax - 1) & AlphaArraySizeMask;
				int _top_array_y = yMin >> AlphaArrayBitSize;
				int _top_bit_y = yMin & AlphaArraySizeMask;
				int _bottom_array_y = (yMax - 1) >> AlphaArrayBitSize;
				int _bottom_bit_y = (yMax - 1) & AlphaArraySizeMask;

				/// 刷新求出区域后的中间场景
				int _c_array_l, _c_array_r, _c_array_t, _c_array_b;
				_c_array_l = (_left_bit_x == 0) ? _left_array_x : (_left_array_x + 1);
				_c_array_r = (_right_bit_x == AlphaArraySize - 1) ? _right_array_x : (_right_array_x - 1);
				_c_array_t = (_top_bit_y == 0) ? _top_array_y : (_top_array_y + 1);
				_c_array_b = (_bottom_bit_y == AlphaArraySize - 1) ? _bottom_array_y : (_bottom_array_y - 1);

				/// top
				for (int i = xMin, j = yMin, jmax = _c_array_t * AlphaArraySize; i < xMax; i++) {
					for (j = yMin; j < jmax; j++) {
						SetTransparent (i, j);
					}
				}

				/// left
				for (int i = xMin, j = 0, jbegin = _c_array_t * AlphaArraySize, imax = _c_array_l * AlphaArraySize,
					 jmax = (_c_array_b + 1) * AlphaArraySize; i < imax; i++) {
					for (j = jbegin; j < jmax; j++) {
						SetTransparent (i, j);
					}
				}

				/// center
				for (int i = _c_array_l, j = _c_array_t, imax = _c_array_r + 1, jmax = _c_array_b + 1; i < imax; i++) {
					for (j = _c_array_t; j < jmax; j++) {
						SetArrayTransparent (i, j);
					}
				}

				/// right
				for (int i = (_c_array_r + 1) * AlphaArraySize, j = 0, jbegin = _c_array_t * AlphaArraySize,
					 jmax = (_c_array_b + 1) * AlphaArraySize; i < xMax; i++) {
					for (j = jbegin; j < jmax; j++) {
						SetTransparent (i, j);
					}
				}

				/// bottom
				for (int i = xMin, j = 0, jBegin = (_c_array_b + 1) * AlphaArraySize; i < xMax; i++) {
					for (j = jBegin; j < yMax; j++) {
						SetTransparent (i, j);
					}
				}

			}
		}

		private void _UpdateTransparentRight (FogTransparentRange rangeData, int view_lt_x, int view_lt_y,
											  int view_left, int view_right, int view_top, int view_bottom)
		{
			int xMin = Max (view_left, rangeData.r);
			int xMax = Min (view_right, rangeData.size);
			int yMin = Max (view_top, rangeData.t);
			int yMax = Min (view_bottom, rangeData.b + 1);
			_UpdateTransparentCustom (rangeData, view_lt_x, view_lt_y, xMin, xMax, yMin, yMax);
		}

		private void _UpdateTransparentBottom (FogTransparentRange rangeData, int view_lt_x, int view_lt_y,
											   int view_left, int view_right, int view_top, int view_bottom)
		{
			int xMin = Max (view_left, rangeData.l);
			int xMax = Min (view_right, rangeData.r + 1);
			int yMin = Max (view_top, rangeData.b);
			int yMax = Min (view_bottom, rangeData.size);
			_UpdateTransparentCustom (rangeData, view_lt_x, view_lt_y, xMin, xMax, yMin, yMax);
		}

		private int Max (int val1, int val2)
		{
			return val1 > val2 ? val1 : val2;
		}

		private int Min (int val1, int val2)
		{
			return val1 > val2 ? val2 : val1;
		}

		#endregion

		public void CleanFixList ()
		{
			fixList.Clear ();
			fixGroupList.Clear ();
		}
	}
}