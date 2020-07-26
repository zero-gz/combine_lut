// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "my_shader/debug_3d_tex" {
	Properties{
		_Volume("Texture", 3D) = "" {}
	}
		SubShader{

		Cull Off ZWrite Off ZTest Always
		Pass {

		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag


		#include "UnityCG.cginc"

		struct vs_input {
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};

		struct ps_input {
			float4 pos : SV_POSITION;
			float3 uv : TEXCOORD0;
		};


		ps_input vert(vs_input v)
		{
			ps_input o;
			o.pos = UnityObjectToClipPos(v.vertex);
			//o.uv = v.vertex.xyz*0.5 + 0.5;
			o.uv = float3(v.uv, fmod(_Time.y, 1.0f) );
			return o;
		}

		sampler3D _Volume;

		float4 frag(ps_input i) : COLOR
	{

			float3 color = tex3D(_Volume, i.uv);
			return float4(color, 1.0);
		}

		ENDCG

		}
	}

		Fallback "VertexLit"
}