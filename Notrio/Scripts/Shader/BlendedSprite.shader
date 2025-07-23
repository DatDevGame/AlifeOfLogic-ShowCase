// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "SgLib/BlendedSprite"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		[PerRendererData] _SecondaryTex("Secondary Texture", 2D) = "white"{}
		_Color ("Tint", Color) = (1,1,1,1)
		[PerRendererData] _BlendFraction ("Fraction", Range(0,1)) = 0
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
		[HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
		[PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
		[PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
		[PerRendererData] _MainScale ("Main scale", Float) = 1
		[PerRendererData] _SecondaryScale ("Secondary scale", FLoat) = 1
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex SpriteVert
			#pragma fragment SpriteFrag_Custom
			#pragma target 2.0
			#pragma multi_compile_instancing
			#pragma multi_compile _ PIXELSNAP_ON
			#pragma multi_compile _ ETC1_EXTERNAL_ALPHA
			#include "UnitySprites.cginc"

			sampler2D _SecondaryTex;
			float _BlendFraction;
			float _MainScale;
			float _SecondaryScale;

			fixed4 SampleSpriteTexture_Custom (float2 uv)
			{
				float2 mainUv = (uv-float2(0.5, 0.5))*_MainScale + float2(0.5, 0.5);
				fixed4 mainColor = tex2D(_MainTex, mainUv);

				float2 secondaryUv = (uv-float2(0.5,0.5))*_SecondaryScale + float2(0.5, 0.5);
				fixed4 secondaryColor = tex2D(_SecondaryTex,secondaryUv);

				fixed4 color = lerp(mainColor, secondaryColor, _BlendFraction);

			#if ETC1_EXTERNAL_ALPHA
				fixed4 alpha = tex2D (_AlphaTex, uv);
				color.a = lerp (color.a, alpha.r, _EnableExternalAlpha);
			#endif

				return color;
			}

			fixed4 SpriteFrag_Custom(v2f IN) : SV_Target
			{
				fixed4 c = SampleSpriteTexture_Custom (IN.texcoord) * IN.color;
				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}
