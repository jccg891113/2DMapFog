Shader "Fog/RightMapAsyn"
{
	Properties
	{
		[PerRendererData] _MainTex ("Texture", 2D) = "white" {}

		_FogTex ("Fog Texture", 2D) = "white" { }

		_FogViewInfo ("Fog View Texture Info. (r=delta x,g= delta y,b=w,a=h)", Color) = (0,0,0,0)

		_TexSizeInfo ("Texture Size Info. (r=minX,g=minY,b=xSize,a=ySize)", Color) = (0, 0, 1, 1)
		_ViewSizeX ("View Size X.", Range(0,1)) = 1
		_ViewSizeY ("View Size Y.", Range(0,1)) = 1
	}
	SubShader
	{
		Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="true" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="true" }

		Pass
		{
			Cull Off 
			ZWrite Off 
			ZTest Always
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 uv : TEXCOORD0;
			};
			
			sampler2D _MainTex;
			half4 _MainTex_TexelSize;
			sampler2D _FogTex;
			fixed4 _TexSizeInfo;
			fixed _ViewSizeX;
			fixed _ViewSizeY;

			fixed4 _FogViewInfo;

			fixed _ViewX;
			fixed _ViewY;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				/// 首先根据图片自身的uv属性转换至0-1坐标范围内
				half2 uv = (v.uv - _TexSizeInfo.xy) / _TexSizeInfo.zw;
				/// 之后根据迷雾范围属性求取真实的尺寸数据
				/// 再根据范围确定遮罩图片的uv数据
				half2 texUV = uv * half2(_ViewSizeX, _ViewSizeY) + half2(_ViewX, _ViewY);
				half2 fogUV = uv * _FogViewInfo.zw + _FogViewInfo.xy;
				o.uv.xy = texUV;
				o.uv.zw = fogUV;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv.xy);
				fixed4 fogCol = tex2D(_FogTex, i.uv.zw);
				return fixed4(col.rgb, fogCol.r);
			}
			ENDCG
		}
	}
}
