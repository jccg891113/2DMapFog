using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FogAbout.FTools
{
	/// <summary>
	/// 迷雾使用debug工具类
	/// </summary>
	public class FogDebug
	{
		[System.Diagnostics.Conditional ("UNITY_EDITOR")]
		private static void _Log (object obj)
		{
			Debug.Log ("[Fog]" + obj);
		}

		[System.Diagnostics.Conditional ("UNITY_EDITOR")]
		public static void LogAllocSize (string before, object baseSize, int num)
		{
			_Log ("[Alloc][Size]" + before + GetSize (baseSize, num));
		}

		private static string GetSize (object baseSize, int num)
		{
			double res = System.Runtime.InteropServices.Marshal.SizeOf (baseSize) * num;
			if (res > 1024) {
				res /= 1024;
			} else {
				return string.Format ("{0:####.###} B", res);
			}
			if (res > 1024) {
				res /= 1024;
			} else {
				return string.Format ("{0:####.###} KB", res);
			}
			if (res > 1024) {
				res /= 1024;
			} else {
				return string.Format ("{0:####.###} MB", res);
			}
			if (res > 1024) {
				res /= 1024;
			} else {
				return string.Format ("{0:####.###} GB", res);
			}
			return string.Format ("{0:F3} TB", res);
		}
	}
}