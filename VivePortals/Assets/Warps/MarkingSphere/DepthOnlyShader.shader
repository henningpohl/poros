Shader "Unlit/DepthOnlyShader" {
    Properties {
        
    }
    SubShader {
        Tags { "RenderType" = "Transparent+100" "RenderType" = "Transparent" }
        LOD 100

		Pass {
			Cull Off
			ColorMask 0
			ZWrite On

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f {
				float4 pos : SV_POSITION;
			};

			v2f vert(appdata_base v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}

			float4 frag(v2f i) : SV_Target {
				return float4(1,1,1,1);
			}
			ENDCG
		}
    }
}
