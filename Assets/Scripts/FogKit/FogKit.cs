using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FogAbout
{
	/// <summary>
	/// 迷雾组件入口
	/// </summary>
	public class FogKit
	{
		FogBaseData baseData;
		FogViewBaseData viewData;
		FogMaskData maskData;

		public FogKit (int world_w, int world_h, int view_w, int view_h, int mask_w, int mask_h)
		{
			baseData = new FogBaseData (world_w, world_h);
			viewData = new FogViewBaseData (view_w, view_h);
			maskData = new FogMaskData (mask_w, mask_h);
		}

		/// <summary>
		/// 更新透明区域
		/// </summary>
		/// <param name="x">透明区域中心点x坐标.</param>
		/// <param name="y">透明区域中心点y坐标.</param>
		/// <param name="r">透明区域半径值.</param>
		public void UpdateRange (int x, int y, int r)
		{
			baseData.UpdateTransparent (x, y, r);
		}

		public void UpdateView (Material viewImageMaterial, float view_lt_x, float view_lt_y)
		{
			viewData.AfterResetVO (baseData, view_lt_x, view_lt_y);
			Color fogView = new Color (viewData.view_delta_x, viewData.view_delta_y, viewData.view_size_x, 
			                           viewData.view_size_y);
			viewImageMaterial.SetColor ("_FogViewInfo", fogView);
		}

		public void RefreshView ()
		{
			maskData.AfterViewDataChange (viewData);
		}

		public void RefreshMaskTexture2D (Texture2D maskPic)
		{
			foreach (var item in maskData.fixGroup.fogMaskFixGroupX) {
				foreach (var gx in item.Value.fogMaskFixGroupY) {
					maskPic.SetPixels (item.Key, gx.beginY, 1, gx.colorArray.Count, gx.colorArray.ToArray ());
				}
			}
			maskPic.Apply ();
		}

		/// <summary>
		/// 清空计算缓冲数据
		/// </summary>
		public void CleanData ()
		{
			baseData.CleanFixList ();
			viewData.CleanFixList ();
			maskData.CleanFixList ();
		}

		/// <summary>
		/// 判断某点是否透明
		/// </summary>
		/// <param name="x">点的x坐标.</param>
		/// <param name="y">点的y坐标.</param>
		public bool IsTransparent (int x, int y)
		{
			return baseData.IsTransparent (x, y);
		}
	}
}