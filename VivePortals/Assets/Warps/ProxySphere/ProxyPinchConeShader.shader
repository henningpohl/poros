Shader "Unlit/ProxyConnectorShader" {
    Properties {
		_From("From", Vector) = (0, 0, 0, 1)
		_FromRadius("From Radius", Float) = 1.0
		_To("To", Vector) = (1, 1, 1, 1)
		_Color("Color", Color) = (0.5,0.5,0.5,1)
    }
    SubShader {
        Tags { "RenderType"="Transparent" "Queue" = "Overlay+200" }
        LOD 100

        Pass {
			Blend SrcAlpha One
			Cull Back

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
				float3 uv     : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
				float3 uv     : TEXCOORD0;
				float4 color  : TEXCOORD1;
            };

			uniform float4 _From;
			uniform float _FromRadius;
			uniform float4 _To;
			uniform float4x4 _ConeMatrix;
			fixed4 _Color;

            v2f vert (appdata v) {
				v2f o;
				float t = pow(v.vertex.z, 3);
				float4 pos = lerp(_From, _To, t);
				float4 offset = mul(_ConeMatrix, float4(0.75 * _FromRadius * v.vertex.xy, 0, 1));
				pos.xyz += offset.xyz;
				o.vertex = mul(UNITY_MATRIX_VP, float4(pos.xyz, 1.0));
				o.uv = float3(v.uv.xy, t);
				o.color = _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
				float2 uvTime = float2(_Time.y * 2, 0);
				float4 col = lerp(i.color, float4(1, 1, 1, 1), i.uv.z * 0.8);
				col.a = i.uv.z * 0.9;
                return col;
            }
            ENDCG
        }
    }
}
