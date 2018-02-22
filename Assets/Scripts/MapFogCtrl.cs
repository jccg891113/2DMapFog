using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 迷雾控制
/// 
/// 依据已知UI尺寸、总图尺寸、视界尺寸、遮罩图物理尺寸
/// 等比换算出
/// 总图存储数组尺寸、视界存储数组尺寸
/// 实时依据尺寸比例刷新数据并更新遮罩图
/// </summary>
public class MapFogCtrl
{
	#region Fields

	private FogAbout.FogKit kit;

	/// <summary>
	/// 视界迷雾遮罩图
	/// </summary>
	public Texture2D fogTexture;
	/// <summary>
	/// 小地图迷雾遮罩
	/// </summary>
	public Texture2D smallFogTexture;
	/// <summary>
	/// 不透明颜色
	/// </summary>
	Color black = new Color (1, 0, 0);

	#endregion

	#region Methods Init 

	/// <summary>
	/// Init the specified view_w_pixel and view_h_pixel.
	/// </summary>
	/// <returns>The init.</returns>
	/// <param name="view_w_pixel">视界遮罩图的大小.</param>
	/// <param name="view_h_pixel">视界遮罩图的大小.</param>
	public void Init (int world_w, int world_h, int view_w, int view_h, int mask_w, int mask_h)
	{
		kit = new FogAbout.FogKit (world_w, world_h, view_w, view_h, mask_w, mask_h);

		InitPic (mask_w, mask_h);
	}

	private void InitPic (int view_w_pixel, int view_h_pixel)
	{
		fogTexture = _GetPic (view_w_pixel, view_h_pixel, "fog tmp texture");
		//root.rightMap.BindFogMask (fogTexture);
	}

	private Texture2D _GetPic (int w, int h, string name)
	{
		Texture2D pic = new Texture2D (w, h, TextureFormat.RGB24, false, true);
		pic.wrapMode = TextureWrapMode.Clamp;
		pic.name = name;

		for (int i = 0, j = 0; i < w; i++) {
			for (j = 0; j < h; j++) {
				pic.SetPixel (i, j, black);
			}
		}
		pic.Apply ();
		return pic;
	}

	#endregion

	#region Methods Update 

	/// <summary>
	/// 正常刷新迷雾状态
	/// </summary>
	public void RealUpdate (float worldX, float worldY, float worldR)
	{
		PixelUpdate ((int) worldX, (int) worldY, (int) worldR);
	}

	/// <summary>
	/// 根据刷子的半径及坐标更新battlefieldAlpha数组，并记录viewAlpha变动点
	/// </summary>
	/// <param name="x">刷新圆心x坐标</param>
	/// <param name="y">刷新圆心y坐标</param>
	/// <param name="r">刷新圆半径</param>
	private void PixelUpdate (int x, int y, int r)
	{
		kit.UpdateRange (x, y, r);
	}

	/// <summary>
	/// vo变动时的更新
	/// 需将VO变动移动至所有范围刷新之后进行
	/// </summary>
	public void TickVOChange ()
	{
		//kit.UpdateView (root.rightMap.fog.material, (root.searchData.voPos.x - root.searchData.woPos.x) * real2data, (root.searchData.voPos.y - root.searchData.woPos.y) * real2data);
	}

	#endregion

	#region Methods Apply 

	public void ApplyFog ()
	{
		kit.RefreshView ();
		kit.RefreshMaskTexture2D (fogTexture);
		kit.CleanData ();
	}

	#endregion

	#region Methods Fog Check 

	public bool FogErased (float worldX, float worldY)
	{
		return kit.IsTransparent ((int) worldX, (int) worldY);
	}

	#endregion

}