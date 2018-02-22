using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FogAbout
{
	/// <summary>
	/// 圆形视野范围数据集合
	/// </summary>
	public class FogTransparentRange
	{
		const bool TRANSPARENT = true;
		const bool OPAQUE = false;

		/// <summary>
		/// 视野圆形范围，数组为方形
		/// </summary>
		public bool[,] array;
		/// <summary>
		/// 视野尺寸，标注数组范围
		/// </summary>
		public int size;
		/// <summary>
		/// 视野九宫格数据
		/// </summary>
		public int l, r, t, b;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:FogAbout.FogTransparentRange"/> class.
		/// 该版本代码视野范围半径正常，但视野范围存在凸出，可能导致范围的不平滑
		/// </summary>
		public FogTransparentRange (int r)
		{
			/// 以下版本代码，视野范围半径正常，但视野范围存在凸出，可能导致范围的不平滑
			size = 2 * r + 1;
			int sqrt2r = (int) (r / System.Math.Sqrt (2));
			this.l = r - sqrt2r;
			this.r = r + sqrt2r;
			this.t = r - sqrt2r;
			this.b = r + sqrt2r;
			array = new bool[size, size];
			int sqrR = r * r;
			for (int i = 0, j = 0; i < size; i++) {
				int sqrI = (r - i) * (r - i);
				for (j = 0; j < size; j++) {
					array[i, j] = (sqrI + (r - j) * (r - j) <= sqrR) ? TRANSPARENT : OPAQUE;
				}
			}
		}

		///// <summary>
		///// Initializes a new instance of the <see cref="T:FogAbout.FogTransparentRange"/> class.
		///// 该版本代码视野范围半径少1，目的使视野缺少一格凸出，令视野范围更平滑
		///// </summary>
		//public FogTransparentRange (int r)
		//{
		//	/// 以下版本代码，视野范围半径少1，目的使视野缺少一格凸出，令视野范围更平滑
		//	size = 2 * r - 1;
		//	int sqrt2r = (int) (r / System.Math.Sqrt (2));
		//	this.l = r - sqrt2r - 1;
		//	this.r = r + sqrt2r - 1;
		//	this.t = r - sqrt2r - 1;
		//	this.b = r + sqrt2r - 1;
		//	array = new bool [size, size];
		//	int sqrR = r * r;
		//	for (int i = 0, j = 0; i < size; i++) {
		//		int sqrI = (r - i - 1) * (r - i - 1);
		//		for (j = 0; j < size; j++) {
		//			if (sqrI + (r - j - 1) * (r - j - 1) <= sqrR) {
		//				array [i, j] = TRANSPARENT;
		//			} else {
		//				array [i, j] = OPAQUE;
		//			}
		//		}
		//	}
		//}
	}
}