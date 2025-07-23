// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "UI/Default"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255

		_ColorMask ("Color Mask", Float) = 15
		_ColorInside ("Color inside", Color) = (1,1,1,1)
		_ColorOutside ("Color outside", Color) = (1,1,1,1)
		_ThresholdInside("Threshold inside", Range(0,1)) = 0
		_ThresholdOutside("Threshold outside", Range(0,1)) = 1
		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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
		
		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]

		Pass
		{
			Name "Default"
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			#pragma multi_compile __ UNITY_UI_ALPHACLIP
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};
			
			fixed4 _Color;
			fixed4 _TextureSampleAdd;
			float4 _ClipRect;
			float4 _ColorInside;
			float4 _ColorOutside;
			float _ThresholdInside;
			float _ThresholdOutside;
			v2f vert(appdata_t IN)
			{
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
				OUT.worldPosition = IN.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

				OUT.texcoord = IN.texcoord;
				
				OUT.color = IN.color * _Color;
				return OUT;
			}

			sampler2D _MainTex;

			fixed4 frag(v2f IN) : SV_Target
			{
				float2 center = float2(0.5,0.5);
				float f = length(IN.texcoord-center)*2;
				f = clamp((f-_ThresholdInside)/(_ThresholdOutside-_ThresholdInside) ,0,1);
				float4 gradientOverlayColor = lerp(_ColorInside, _ColorOutside, f);

				half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color*gradientOverlayColor;
				
				color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
				
				#ifdef UNITY_UI_ALPHACLIP
				clip (color.a - 0.001);
				#endif

				return color;
			}
		ENDCG
		}
	}
}
/*
Shader "SgLib/RadialGradientOverlay" {
	Properties {
		_MainTex ("_MainTex", 2D) = "white" {}
		_Color ("Color tint", Color) = (1,1,1,1)
		_ColorInside ("Color inside", Color) = (1,1,1,1)
		_ColorOutside ("Color outside", Color) = (1,1,1,1)
		_ThresholdInside("Threshold inside", Range(0,1)) = 0
		_ThresholdOutside("Threshold outside", Range(0,1)) = 1
	}
	SubShader {
		Tags {"Queue" = "Transparent"}
		Blend SrcAlpha OneMinusSrcAlpha
		Pass { 
		
			CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			uniform sampler _MainTex;
			uniform float4 _Color;
			uniform float4 _ColorInside;
			uniform float4 _ColorOutside;
			uniform float _ThresholdInside;
			uniform float _ThresholdOutside;

			struct vertexInput 
			{
				float4 vertex : POSITION;
				float4 texCoord : TEXCOORD0;
			};
			struct vertexOutput 
			{
				float4 pos : SV_POSITION;
				float4 texCoord : TEXCOORD0;
			};

			vertexOutput vert(vertexInput i) 
			{
				vertexOutput o;
				o.texCoord = i.texCoord;
				o.pos = UnityObjectToClipPos(i.vertex);
				return o;
            }

            float4 frag(vertexOutput o) : COLOR 
			{
				float2 center = float2(0.5,0.5);
				float f = length(o.texCoord-center)*2;
				f = clamp((f-_ThresholdInside)/(_ThresholdOutside-_ThresholdInside) ,0,1);
				float4 color = lerp(_ColorInside, _ColorOutside, f);
				float4 texColor = tex2D(_MainTex, o.texCoord);

				return color*texColor*_Color;
            }
			ENDCG
		}
	}
	Fallback "Unlit/Color"
}
*/