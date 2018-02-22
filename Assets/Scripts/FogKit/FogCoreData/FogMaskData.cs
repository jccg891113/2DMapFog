using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FogAbout
{
	/// <summary>
	/// 遮罩图数据
	/// 
	/// 根据视界数据尺寸，划分出等量的方块
	/// 按照具体尺寸比例在对应方格内进行查找
	/// 
	/// 查找方法（放大版）：
	/// 有n分为m份
	/// 求 i = n % m
	/// 求 l = n / m （运算过程及结果均为整数）
	/// 则m份中，id小于i的组有 l+1 个，id大于等于i的组有 l 个
	/// 
	/// 查找方法（缩小版）：
	/// 大份分解与放大版等同
	/// 在一次分解完成后，当前组内元素个数 v 必然小于8
	/// 则参照放大版查找方法，将8分为v份
	/// 得到小组内的按照8获取的index所处第几份，则设置该份所代表像素
	/// </summary>
	public class FogMaskData
	{
		const long IntMask = 0x00000000ffffffff;

		const int ClearCount = 0x00010000;
		const int BlackCount = 0x00000001;
		const int BlackMask = 0x0000ffff;

		public int mask_w;
		public int mask_h;

		public FogMaskFixGroup fixGroup;

		/// <summary>
		/// 不透明颜色
		/// </summary>
		Color black = new Color (1, 0, 0);
		/// <summary>
		/// 透明颜色
		/// </summary>
		Color clear = new Color (0, 0, 0);

		/// <summary>
		/// Initializes a new instance of the <see cref="T:BabelTime.GD.UI.BattleUISub.FogAbout.FogMaskData"/> class.
		/// </summary>
		/// <param name="w">遮罩图的实际宽度.</param>
		/// <param name="h">遮罩图的实际高度.</param>
		public FogMaskData (int w, int h)
		{
			this.mask_w = w;
			this.mask_h = h;

			fixGroup = new FogMaskFixGroup ();
		}

		public void AfterViewDataChange (FogViewBaseData viewData)
		{
			float ratio = (float) viewData.array_real_h / mask_h;
			if (ratio < 1) {
				Enlarge (viewData.fixList, ratio);
			} else {
				Shrink (viewData.fixList, ratio);
			}
			fixGroup.DataCombo ();
		}

		/// <summary>
		/// 换算放大操作
		/// 
		/// 因遮罩图尺寸大于视界数组尺寸，需对视界变动数据进行放大操作
		/// </summary>
		private void Enlarge (FogDataFixList fixList, float ratio)
		{
			float _tmp = 1 / ratio;
			int xMin, xMax, yMin, yMax;
			Color c = clear;
			for (int i = 0; i < fixList.Count; i++) {
				c = fixList [i].transparent ? clear : black;

				xMin = (int) (fixList [i].x * _tmp);
				xMax = (int) ((fixList [i].x + 1) * _tmp);
				xMax = xMax > mask_w ? mask_w : xMax;

				yMin = (int) (fixList [i].y * _tmp);
				yMax = (int) ((fixList [i].y + 1) * _tmp);
				yMax = yMax > mask_h ? mask_h : yMax;

				for (int x = xMin, y = 0; x < xMax; x++) {
					for (y = yMin; y < yMax; y++) {
						fixGroup.Add (x, y, c);
					}
				}
			}
		}

		/// <summary>
		/// 换算缩小操作
		/// </summary>
		private void Shrink (FogDataFixList fixList, float ratio)
		{
			// TODO:ERROR!!!
			float _tmp = 1 / ratio;
			Dictionary<long, int> fixDic = new Dictionary<long, int> ();
			int x, y;
			long posId;
			for (int i = 0; i < fixList.Count; i++) {
				x = (int) (fixList [i].x * _tmp/* + 0.44445f*/);
				y = (int) (fixList [i].y * _tmp/* + 0.44445f*/);
				if (x < mask_w && y < mask_h) {
					posId = (((long) x) << 32) + y;
					if (!fixDic.ContainsKey (posId)) {
						fixDic [posId] = fixList [i].transparent ? ClearCount : BlackCount;
					} else {
						fixDic [posId] += fixList [i].transparent ? ClearCount : BlackCount;
					}
				}
			}
			int clearCount, blackCount;
			foreach (var item in fixDic) {
				x = (int) (item.Key >> 32);
				y = (int) (item.Key & IntMask);
				clearCount = item.Value >> 16;
				blackCount = item.Value & BlackMask;
				if (clearCount > blackCount) {
					fixGroup.Add (x, y, clear);
				} else {
					fixGroup.Add (x, y, black);
				}
			}
		}

		public void CleanFixList ()
		{
			fixGroup.Clear ();
		}

	}
}