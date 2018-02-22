using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainTest : MonoBehaviour
{
	private FogAbout.FogKit kit;

	/// <summary>
	/// 视界迷雾遮罩图
	/// </summary>
	[HideInInspector] public Texture2D fogTexture;

	public Image maskPic;

	const int w = 1334;
	const int h = 750;
	const int hw = 667;
	const int hh = 375;

	// Use this for initialization
	void Start ()
	{
		kit = new FogAbout.FogKit (w, h, w, h, w, h);

		fogTexture = new Texture2D (w, h, TextureFormat.RGB24, false, true);
		fogTexture.wrapMode = TextureWrapMode.Clamp;
		fogTexture.name = "Tmp fog mask";

		Color black = new Color (1, 0, 0);
		for (int i = 0, j = 0; i < w; i++) {
			for (j = 0; j < h; j++) {
				fogTexture.SetPixel (i, j, black);
			}
		}
		fogTexture.Apply ();

		maskPic.material.SetTexture ("_FogTex", fogTexture);
	}

	public int view_r = 40;
	public float x = 0;
	public float y = 0;
	public float moveSpeed = 1;

	public RectTransform viewTrans;

	// Update is called once per frame
	void Update ()
	{
		// Update position (x,y)
		if (_move_left) {
			x -= moveSpeed;
		}
		if (_move_right) {
			x += moveSpeed;
		}
		if (_move_top) {
			y += moveSpeed;
		}
		if (_move_bottom) {
			y -= moveSpeed;
		}

		// Check position (x,y)
		if (x < -hw) {
			x = -hw;
		} else if (x > hw) {
			x = hw;
		}
		if (y < -hh) {
			y = -hh;
		} else if (y > hh) {
			y = hh;
		}

		// Update transform view
		viewTrans.localPosition = new Vector3 (x, y, 0);

		float lt_x = x + hw;
		float lt_y = y + hh;

		kit.UpdateRange ((int) lt_x, (int) lt_y, view_r);
		/// 为了测试，令视野大小等于地图大小，且固定视野最小点为(0,0)点
		kit.UpdateView (maskPic.material, 0, 0);

		kit.RefreshView ();
		kit.RefreshMaskTexture2D (fogTexture);
		kit.CleanData ();
	}

	private bool _move_left = false;
	private bool _move_right = false;
	private bool _move_top = false;
	private bool _move_bottom = false;

	private void OnGUI ()
	{
		_move_left = GUILayout.RepeatButton ("Left");
		_move_right = GUILayout.RepeatButton ("Right");
		_move_top = GUILayout.RepeatButton ("Top");
		_move_bottom = GUILayout.RepeatButton ("Bottom");
	}
}
