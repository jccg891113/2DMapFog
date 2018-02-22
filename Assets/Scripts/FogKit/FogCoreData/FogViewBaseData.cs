using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FogAbout
{
	/// <summary>
	/// 地图在视野内的迷雾数据
	/// 记录视野范围的大小，并把视野内的迷雾数据采用8*8的单元格进行存储
	/// </summary>
	public class FogViewBaseData
	{
		const bool TRANSPARENT = true;
		const bool OPAQUE = false;

		public int real_w;
		public int real_h;

		public int array_w;
		public int array_h;

		public int array_real_w;
		public int array_real_h;

		public ulong[,] array;

		/// <summary>
		/// 视野的真实范围对应视野的数据范围的偏移量（百分比）
		/// </summary>
		public float view_delta_x, view_delta_y;
		/// <summary>
		/// 视野的真实范围对应视野的数据范围的比值（百分比）
		/// </summary>
		public float view_size_x, view_size_y;

		public FogDataFixList fixList;

		public FogViewBaseData (int w, int h)
		{
			this.real_w = w;
			this.real_h = h;
			/// 多存储两个单元数据以解决视野偏移的情况
			this.array_w = (w / FogBaseData.AlphaArraySize) + 2;
			this.array_h = (h / FogBaseData.AlphaArraySize) + 2;

			array_real_w = this.array_w * FogBaseData.AlphaArraySize;
			array_real_h = this.array_h * FogBaseData.AlphaArraySize;

			view_size_x = (float) real_w / array_real_w;
			view_size_y = (float) real_h / array_real_h;

			this.array = new ulong[array_w, array_h];

#if UNITY_EDITOR
			FTools.FogDebug.LogAllocSize ("[ViewData]", default (ulong), array_w * array_h);
#endif

			fixList = new FogDataFixList ();
		}

		public void AfterResetVO (FogBaseData fogBaseData, float world_x, float world_y)
		{
			_AfterResetVO_2 (fogBaseData, world_x, world_y);
		}

		private void _AfterResetVO_2 (FogBaseData fogBaseData, float world_x, float world_y)
		{
			if (world_x < 0) {
				world_x = 0;
			}
			if (world_y < 0) {
				world_y = 0;
			}

			int _tmp_world_x = (int) world_x;
			int _tmp_world_y = (int) world_y;

			int world_begin_x = _tmp_world_x >> FogBaseData.AlphaArrayBitSize;
			int world_begin_y = _tmp_world_y >> FogBaseData.AlphaArrayBitSize;

			float world_begin_delta_x_f = world_x - world_begin_x * FogBaseData.AlphaArraySize;
			float world_begin_delta_y_f = world_y - world_begin_y * FogBaseData.AlphaArraySize;

			ulong _base_0x01 = 0x01;

			ulong _tmp_world_8x8, _tmp_xor;
			int _index;
			int baseX, baseY;

			int _tmp_array_x, _tmp_array_y;

			/// 遍历刷新视野的迷雾数据
			for (int i = 0, j = 0; i < array_w; i++) {
				_tmp_array_x = world_begin_x + i;
				for (j = 0; j < array_h; j++) {
					_tmp_array_y = world_begin_y + j;
					/// 获取当前世界单元格迷雾数据
					_tmp_world_8x8 = (_tmp_array_x < fogBaseData.array_w && _tmp_array_y < fogBaseData.array_h) ?
						fogBaseData.totalAlphaArray[_tmp_array_x, _tmp_array_y] : 0x0;
					/// 获取当前世界单元格与当前视野单元格的迷雾数据的异或值
					_tmp_xor = _tmp_world_8x8 ^ array[i, j];
					/// 当当前世界单元格与当前视野单元格的迷雾数据不一致时，在fixList中存储发生数据变化的视野单元的数据
					if (_tmp_xor > 0) {
						_index = 0;
						baseX = i << FogBaseData.AlphaArrayBitSize;
						baseY = j << FogBaseData.AlphaArrayBitSize;
						while (_index < 64) {
							if ((_tmp_xor & _base_0x01) > 0) {
								fixList.Add (baseX + (_index >> FogBaseData.AlphaArrayBitSize),
											 baseY + (_index & FogBaseData.AlphaArraySizeMask),
											 (_tmp_world_8x8 & (_base_0x01 << _index)) > 0 ? TRANSPARENT : OPAQUE);
							}
							_tmp_xor = _tmp_xor >> 1;
							_index++;
						}
					}
					array[i, j] = _tmp_world_8x8;
				}
			}

			view_delta_x = world_begin_delta_x_f / array_real_w;
			view_delta_y = world_begin_delta_y_f / array_real_h;

		}

		public void CleanFixList ()
		{
			fixList.Clear ();
		}
	}
}