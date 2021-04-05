Shader "Unlit/GrabSphereShader" {
    Properties {
        _Color ("Color", Color) = (1, 1, 1, 1)
		_FresnelPower("Fresnel Power", Float) = 2.0
    }
    SubShader {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+200" }
		ZTest Always
		Blend SrcAlpha One
        LOD 100

        Pass {
            CGPROGRAM
			#pragma multi_compile _ CLIP_SPHERE_ON
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
				float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4  ws : TEXCOORD1;
				float fresnel : TEXCOORD4;
            };

			uniform float4x4 _ClipTransform = {
				1.0f, 0.0f, 0.0f, 0.0f,
				0.0f, 1.0f, 0.0f, 0.0f,
				0.0f, 0.0f, 1.0f, 0.0f,
				0.0f, 0.0f, 0.0f, 1.0f };
			uniform float4x4 _ClipTransformInv = {
					1.0f, 0.0f, 0.0f, 0.0f,
					0.0f, 1.0f, 0.0f, 0.0f,
					0.0f, 0.0f, 1.0f, 0.0f,
					0.0f, 0.0f, 0.0f, 1.0f };
			uniform float4 _ClipObjPosition;
			uniform float4 _ClipObjScale;
			uniform float _ClipObjEdgeThickness;
			uniform float4 _ClipObjEdgeColor;

			inline float3 ClipObjSpaceViewDir(in float4 v) {
				float3 clipSpaceCameraPos = mul(_ClipTransformInv, float4(_WorldSpaceCameraPos.xyz, 1)).xyz;
				float3 objSpaceCameraPos = mul(unity_WorldToObject, float4(clipSpaceCameraPos, 1)).xyz;
				return objSpaceCameraPos - v.xyz;
			}

            v2f vert (appdata v) {
				v2f o;
				o.ws = mul(unity_ObjectToWorld, v.vertex);
#ifdef CLIP_SPHERE_ON
				o.vertex = mul(UNITY_MATRIX_VP, mul(_ClipTransform, o.ws));
				fixed3 viewDir = normalize(ClipObjSpaceViewDir(v.vertex));
#else
				o.vertex = mul(UNITY_MATRIX_VP, o.ws);
				fixed3 viewDir = normalize(ObjSpaceViewDir(v.vertex));
#endif
				o.uv = v.uv;
				o.fresnel = 1 - saturate(dot(v.normal, viewDir));

				return o;
            }

			fixed4 _Color;
			float _FresnelPower;

            fixed4 frag (v2f i) : SV_Target {
#ifdef CLIP_SPHERE_ON
				float dist = length((i.ws.xyz - _ClipObjPosition.xyz) / _ClipObjScale.xyz);
				if (dist > 1.0) {
					discard;
				}
#endif

				float fresnel = pow(i.fresnel, _FresnelPower);
				float4 col = _Color;
				col.rgb += fresnel;
				return col;
            }
            ENDCG
        }
    }
		FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
