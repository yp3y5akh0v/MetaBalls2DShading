Shader "Custom/MetalBallsShader"
{
	Properties
	{
		_ScaleBlob("Scale Blob", Range(1, 100)) = 1
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" "Queue" = "Transparent" }

		CGPROGRAM
		#pragma target 4.0
		#pragma surface surf Lambert vertex:vert
		#include "UnityCG.cginc"

		int blobsCount;
		float blobsRadius[100];
		float4 blobsPosition[100];
		float blobsMass[100];

		struct Input {
			float3 objPos;
		};

		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.objPos = v.vertex;
		}

		float _ScaleBlob;

		void surf(Input IN, inout SurfaceOutput o) {
			float s = 0;
			for (int i = 0; i < blobsCount; i++) {
				s += _ScaleBlob * blobsRadius[i] * blobsMass[i] / pow(length(IN.objPos - blobsPosition[i].xyz), 2);
			}
			o.Albedo = s;
			o.Alpha = 1;
		}
		ENDCG
	}
}
