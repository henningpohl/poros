Shader "Unlit/MarkingSphereInsideShader" {
	Properties{
		_OutsideColor("Outside Color", Color) = (1, 0.7, 0.7, .1)
		_InsideColor("Inside Color", Color) = (0.2, 0.2, 0.2, 0.2)
		_IntersectionIntensity("Intersection Intensity", Float) = 0.1
		_FresnelPower("Fresnel Power", Float) = 2.0
		_SwipeWidth("Swipe Width", Float) = 200.0
		_SwipeSpeed("Swipe Speed", Float) = 10.0
	}
		SubShader{
			Tags { "Queue" = "Transparent+200" "RenderType" = "Transparent" }
			LOD 100
			Blend SrcAlpha OneMinusSrcAlpha

			Pass {
				Cull Front

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 3.0

				#include "UnityCG.cginc"
				#include "Assets/Util/SimplexNoise3D.hlsl"
				#include "Assets/Warps/MarkingSphere/MarkingSphereShared.hlsl"

				fixed4 _OutsideColor;
				fixed4 _InsideColor;
				float _IntersectionIntensity;
				float _FresnelPower;
				float _SwipeSpeed;
				float _SwipeWidth;

				float random(half2 st) {
					return frac(sin(dot(st.xy,
						half2(12.9898, 78.233)))*
						43758.5453123);
				}

				float GetSwipeIntensity(half3 worldNormal, half2 uv) {
					half2 a = normalize(worldNormal.xz);
					half2 b = half2(cos(_SwipeSpeed * _Time.y), sin(_SwipeSpeed * _Time.y));
					float swipe = saturate(abs(dot(a, b)));
					float swipeBase = pow(swipe, _SwipeWidth);
					float swipeNoise = random(floor(uv * _SwipeWidth));
					return lerp(swipeBase, pow(swipe, 0.5 * _SwipeWidth) * swipeNoise, 1 - swipeBase);
				}

				fixed4 frag(v2f i) : SV_Target{
					half3 worldViewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
					half3 worldNormal = normalize(i.worldNormal) * -1;

					float depthLinear = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrPos)));
					float intersect = pow(1 - saturate(abs(depthLinear - i.scrPos.z) / _IntersectionIntensity), 15);

					fixed4 col = _InsideColor;
					col.a += intersect;

					float swipe = GetSwipeIntensity(worldNormal, i.uv);
					col.a = lerp(col.a, swipe, swipe * col.a * 5);

					return col;
				}

				ENDCG
			}
	}
}