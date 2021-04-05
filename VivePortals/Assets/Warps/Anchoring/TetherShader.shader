Shader "Unlit/TetherShader" {
    Properties {
        _Color ("Color", Color) = (0.6, 0.5, 0.4, 1.0)
		_PulseColor ("Pulse color", Color) = (0.8, 0.7, 0.4, 1.0)
		_Alpha ("Alpha", Range(0, 1)) = 1.0
		_PulseSpeed ("Pulse speed", Float) = 6.0
    }
    SubShader {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+100" "LightMode" = "UniversalForward"}
        LOD 100
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off

        Pass {
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

			fixed4 _Color;
			fixed4 _PulseColor;
			float _Alpha;
			float _PulseSpeed;

            fixed4 frag (v2f i) : SV_Target {
				float t = 0.5 * (1 + sin(_Time.y * _PulseSpeed));
				fixed4 col = lerp(_Color, _PulseColor, t);
				col.a = _Alpha;
#ifdef CLIP_SPHERE_ON
				float dist = length((i.ws.xyz - _ClipObjPosition.xyz) / _ClipObjScale.xyz);
				if (dist > 1.0) {
					discard;
				}
#endif
                return col;
            }
            ENDCG
        }
    }
}
