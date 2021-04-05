Shader "Unlit/ProxyConnectorShader" {
    Properties {
		_ShrinkFactor("Shrink Factor", Range(0.0, 1.0)) = 0.5
		_ShrinkStart("Shrink Start", Range(0.0, 0.75)) = 0.2
		_ConnectorTex("Connector Texture", 2D) = "white" {}
    }
    SubShader {
        Tags { "RenderType"="Transparent" "Queue" = "Overlay" }
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
            };

            struct v2f {
                float4 vertex : SV_POSITION;
				float3 uv     : TEXCOORD0;
				float4 tint   : TEXCOORD1;
            };

			// https://docs.unity3d.com/Packages/com.unity.shadergraph@6.9/manual/Gradient-Noise-Node.html
			float2 unity_gradientNoise_dir(float2 p) {
				p = p % 289;
				float x = (34 * p.x + 1) * p.x % 289 + p.y;
				x = (34 * x + 1) * x % 289;
				x = frac(x / 41) * 2 - 1;
				return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
			}

			// https://docs.unity3d.com/Packages/com.unity.shadergraph@6.9/manual/Gradient-Noise-Node.html
			float unity_gradientNoise(float2 p) {
				float2 ip = floor(p);
				float2 fp = frac(p);
				float d00 = dot(unity_gradientNoise_dir(ip), fp);
				float d01 = dot(unity_gradientNoise_dir(ip + float2(0, 1)), fp - float2(0, 1));
				float d10 = dot(unity_gradientNoise_dir(ip + float2(1, 0)), fp - float2(1, 0));
				float d11 = dot(unity_gradientNoise_dir(ip + float2(1, 1)), fp - float2(1, 1));
				fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
				return lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x);
			}

			uniform float4 _From;
			uniform float _FromRadius;
			uniform float4 _FromColor;
			uniform float4 _To;
			uniform float _ToRadius;
			uniform float4 _ToColor;
			uniform float4x4 _ConnectionMatrix;
			float _ShrinkFactor;
			float _ShrinkStart;
			sampler2D _ConnectorTex;
			fixed4 _ConnectorTint;

            v2f vert (appdata v) {
				v2f o;
				float t = v.vertex.z;
				float l = length(_To - _From);
				float startOffset = _FromRadius / (l + 2);
				float endOffset = _ToRadius / (l + 2);

				float4 pos = lerp(_From, _To, t);
				float4 offset = mul(_ConnectionMatrix, float4(v.vertex.xy, 0, 1));
				float offsetFactor = (1.0 - _ShrinkFactor) + _ShrinkFactor * (smoothstep(_ShrinkStart + startOffset, 0, t) + smoothstep(1 - _ShrinkStart - endOffset, 1, t));
				float radius = lerp(_FromRadius, 0, t) + lerp(0, _ToRadius, t);
				pos.xyz += radius * offsetFactor * offset.xyz;
				o.vertex = mul(UNITY_MATRIX_VP, float4(pos.xyz, 1.0));

				o.uv = float3(l * t, 2 * atan2(v.vertex.y, v.vertex.x), t);
				o.tint = lerp(_FromColor, _ToColor, t);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
				float alpha = smoothstep(0.1, 0.3, i.uv.z) * smoothstep(0.9, 0.7, i.uv.z);
				float2 uvTime = float2(_Time.y * 2, 0);
				float2 uvNoise = unity_gradientNoise(i.uv + uvTime);
				float4 col = tex2D(_ConnectorTex, i.uv + uvTime + 0.75 * uvNoise) * i.tint;
				col.a = alpha * (1 - col.r);
                return col;
            }
            ENDCG
        }
    }
}
