Shader "Unlit/NewUnlitShader" {
    Properties {
		_RotationSpeed("Rotation Speed", Float) = 0.1
		_HeightCutoff("Height Cutoff", Float) = 0.3
    }
    SubShader {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100

        Pass {
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off

            CGPROGRAM
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
				float4 worldPos : TEXCOORD1;
				float3 modelPos : TEXCOORD2;
            };

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.modelPos = v.vertex.xyz;
				o.uv = v.uv;
                return o;
            }

			float _RotationSpeed;
			float _HeightCutoff;

            fixed4 frag (v2f i) : SV_Target {
				fixed4 color = fixed4(1, 1, 1, step(0.05, (i.uv.x + _RotationSpeed * _Time.y) % 0.1));
				clip(i.worldPos.y);
				clip(-i.worldPos.y + _HeightCutoff);
				clip(i.modelPos.y - 0.001);
				clip((1 - i.modelPos.y) - 0.001);
				clip(color.a - 0.5);

				color.a *= smoothstep(_HeightCutoff, 0.0, i.worldPos.y);

				return color;
            }
            ENDCG
        }
    }
}
