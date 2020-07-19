Shader "my_shader/tex_3dlut"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_CombinedLutTex("Texture", 3D) = "" {}
	}
		SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
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
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;
			//sampler2D _ColorBeforeTonemapTex;
			sampler3D _CombinedLutTex;

			//
			// Generic log lin transforms
			//
			float3 LogToLin1(float3 LogColor)
			{
				const float LinearRange = 14;
				const float LinearGrey = 0.18;
				const float ExposureGrey = 444;

				float3 LinearColor = exp2((LogColor - ExposureGrey / 1023.0) * LinearRange) * LinearGrey;
				return LinearColor;
			}

			float3 LinToLog1(float3 LinearColor)
			{
				const float LinearRange = 14;
				const float LinearGrey = 0.18;
				const float ExposureGrey = 444;

				float3 LogColor = log2(LinearColor) / LinearRange - log2(LinearGrey) / LinearRange + ExposureGrey / 1023.0;
				LogColor = saturate(LogColor);

				return LogColor;
			}

			static const float LUTSize = 32;
			half3 ColorLookupTable(half3 LinearColor)
			{
				float3 LUTEncodedColor;

				LUTEncodedColor = LinToLog1(LinearColor + LogToLin1(0));

				float3 UVW = LUTEncodedColor * ((LUTSize - 1) / LUTSize) + (0.5f / LUTSize);

				half3 OutDeviceColor = tex3D(_CombinedLutTex, UVW);
				return OutDeviceColor * 1.05;
			}

			float4 frag(v2f i) : SV_Target
			{
				float4 col = tex2D(_MainTex, i.uv);

				float4 finalColor = 1;
				finalColor.rgb = ColorLookupTable(col.rgb);

				if (!IsGammaSpace())
				{
					finalColor = pow(finalColor, 2.2);
				}
				return finalColor;
			}
			ENDCG
		}
	}
}