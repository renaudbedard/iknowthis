// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Mosaic"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_StartTime ("Start Time", Float) = 0
	}

	SubShader
	{
		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				half2 texcoord  : TEXCOORD0;
			};
			
			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
#ifdef UNITY_HALF_TEXEL_OFFSET
				OUT.vertex.xy += (_ScreenParams.zw-1.0)*float2(-1,1);
#endif
				return OUT;
			}

			sampler2D _MainTex;
			float _StartTime;

			fixed4 frag(v2f IN) : SV_Target
			{
				float2 uv = IN.texcoord;
				float t = _Time.y - _StartTime;
				t -= fmod(t, 0.1);

				float2 f = fmod(uv, t * float2(0.01 * 9.0f / 16, 0.01));
				uv = uv - f + (t * float2(0.01 * 9.0f / 16, 0.01) * 0.5);
    
				float4 color = tex2D(_MainTex, uv);

				return color;
			}
		ENDCG
		}
	}
}