Shader "Unlit/MarkingSphereOutsideShader" {
	Properties{
		_OutsideColor("Outside Color", Color) = (1, 0.7, 0.7, .1)
		_InsideColor("Inside Color", Color) = (0.2, 0.2, 0.2, 0.2)
		_IntersectionIntensity("Intersection Intensity", Float) = 0.1
		_FresnelPower("Fresnel Power", Float) = 2.0
	}
		SubShader{
			Tags { "Queue" = "Overlay" "RenderType" = "Transparent" }
			LOD 100
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

			Pass {
				Cull Off

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 3.0

				#include "UnityCG.cginc"
				#include "Assets/Util/SimplexNoise3D.hlsl"
				#include "Assets/Warps/MarkingSphere/MarkingSphereShared.hlsl"

				fixed4 _OutsideColor;
				fixed4 _InsideColor;
				float _FresnelPower;
				float _IntersectionIntensity;

				fixed4 frag(v2f i, fixed face : VFACE) : SV_Target{
					float depthLinear = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrPos)));
					float intersect = pow(1 - saturate(abs(depthLinear - i.scrPos.z) / _IntersectionIntensity), 15);

					float fresnel = pow(i.fresnel, _FresnelPower);
					if (face > 0) {
						fresnel = fresnel * 2 + 0.02;
					} else {
						fresnel = 0;
					}

					float4 col = _OutsideColor;
					col.a *= (fresnel + intersect);

					return col;
				}

				ENDCG
			}
	}
}