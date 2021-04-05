Shader "Unlit/HighlightShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+100" }
		Cull Off
		ZTest Always
		ZWrite On
        LOD 100

        Pass {
			Tags{"LightMode" = "UniversalForward"}

            CGPROGRAM
			#pragma multi_compile _ CLIP_SPHERE_ON
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				float4  ws : TEXCOORD1;
            };

            sampler2D _MainTex;
			uniform float4x4 _ClipTransform = {
				1.0f, 0.0f, 0.0f, 0.0f,
				0.0f, 1.0f, 0.0f, 0.0f,
				0.0f, 0.0f, 1.0f, 0.0f,
				0.0f, 0.0f, 0.0f, 1.0f };
			uniform float4 _ClipObjPosition;
			uniform float4 _ClipObjScale;
			uniform float _ClipObjEdgeThickness;
			uniform float4 _ClipObjEdgeColor;

            v2f vert (appdata v) {
				v2f o;
				o.ws = mul(unity_ObjectToWorld, v.vertex);
#ifdef CLIP_SPHERE_ON
				o.vertex = mul(UNITY_MATRIX_VP, mul(_ClipTransform, o.ws));
#else
				o.vertex = mul(UNITY_MATRIX_VP, o.ws);
#endif
                o.uv = v.uv;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
				fixed4 col = fixed4(0, 0, 0, 0);
#ifdef CLIP_SPHERE_ON
				float dist = length((i.ws.xyz - _ClipObjPosition.xyz) / _ClipObjScale.xyz);
				if (dist > 1.0) {
					discard;
				} 
#endif
                col = tex2D(_MainTex, i.uv);
				clip(col.a - 0.1);
                return col;
            }
            ENDCG
        }
    }
		FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
