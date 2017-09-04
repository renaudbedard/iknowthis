// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UI/Slow"
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

		_StartTime ("Start Time", Float) = 0
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
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
			};
			
			fixed4 _Color;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
#ifdef UNITY_HALF_TEXEL_OFFSET
				OUT.vertex.xy += (_ScreenParams.zw-1.0)*float2(-1,1);
#endif
				OUT.color = IN.color * _Color;
				return OUT;
			}

			sampler2D _MainTex;
			float _StartTime;

			float random(float2 p) 
			{
				float value = sin(dot(p,float2(12.9898, 78.233))) * 43758.5453;
				return frac(value);
			}

			#define V2_X float2(1.0, 0.0)
			#define V2_Y float2(0.0, 1.0)

			float valueNoise(float2 p)
			{
				float2 integer = floor(p), remainder = frac(p);
  
				float2x2 columns = float2x2(
				random(integer),        random(integer + V2_X),
				random(integer + V2_Y), random(integer + 1.0));
    
				float2 row = lerp(columns[0], columns[1], remainder.yy);
				return lerp(row[0], row[1], remainder.x);
			}

			#define OCTAVES 3
			#define DEG_TO_RAD 0.0174532925
			#define sind(x) sin(DEG_TO_RAD * x)
			#define cosd(x) cos(DEG_TO_RAD * x)
			#define THETA 30.0

			float fractalNoise(float2 p) 
			{
				float result = 0.0;
				float contribution = 0.5;

    			float2x2 rotationMatrix = float2x2( cosd(THETA), sind(THETA),
        									-sind(THETA), cosd(THETA));        
    
				for (int i = 0; i < OCTAVES; i++) 
				{
					result += valueNoise(p) * contribution;
					contribution /= 2.0;
					p *= 2.0;
					p = mul(rotationMatrix, p);
				}
    
				return result;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				float2 uv = IN.texcoord;
				float t = _Time.y - _StartTime;
    
				float4 color = tex2D(_MainTex, uv);
    
				uv.y = 1.0 - uv.y;

				float easedTime = pow(fractalNoise(float2(t, 0.0)) + t, 2.5) / 3.0;
    
				// highlight currently loading block (horrible way to do it, but ¯\_(ツ)_/¯)
				if (((uv.x - fmod(uv.x, 0.025)) +
					(uv.y - fmod(uv.y, 0.025)) * 35.0 
					> 
					easedTime) 
					&& ((uv.x - fmod(uv.x, 0.025)) +
					(uv.y - fmod(uv.y, 0.025)) * 35.0 
					< 
					easedTime + 0.025))
				{
					color *= 3.0;        
				}
    
				color.a -= 
					(uv.x - fmod(uv.x, 0.025)) +
					(uv.y - fmod(uv.y, 0.025)) * 35.0 
					< 
					easedTime ? 0.0 : 1.0;    
    
				color *= IN.color;
				clip (color.a - 0.01);

				return color;
			}
		ENDCG
		}
	}
}