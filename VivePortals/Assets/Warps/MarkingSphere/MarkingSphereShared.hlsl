#ifndef MARKINGSPHERESHARED_INCLUDED
#define MARKINGSPHERESHARED_INCLUDED

struct appdata {
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	float2 uv     : TEXCOORD0;
};

struct v2f {
	float4 vertex     : SV_POSITION;
	float2 uv         : TEXCOORD0;
	float3 worldPos   : TEXCOORD1;
	float4 scrPos     : TEXCOORD2;
	half3 worldNormal : TEXCOORD3;
	float fresnel     : TEXCOORD4;
};

v2f vert(appdata v) {
	v2f o;
	o.vertex = UnityObjectToClipPos(v.vertex);
	o.worldPos = mul(unity_ObjectToWorld, v.vertex.xyz).xyz;
	o.scrPos = ComputeScreenPos(o.vertex);
	COMPUTE_EYEDEPTH(o.scrPos.z);
	o.worldNormal = UnityObjectToWorldNormal(v.normal);
	o.uv = v.uv;
	fixed3 viewDir = normalize(ObjSpaceViewDir(v.vertex));
	o.fresnel = 1 - saturate(dot(v.normal, viewDir));
	return o;
}

// https://docs.unity3d.com/Manual/SL-UnityShaderVariables.html
// https://docs.unity3d.com/Manual/SL-DepthTextures.html
// https://halisavakis.com/shader-bits-camera-depth-texture/
// https://forum.unity.com/threads/how-to-sample-_cameradepthtexture-with-calculateobliquematrix-projection-matrix.578125/
// https://github.com/vux427/ForceFieldFX/blob/master/ForceFieldFX/Assets/Shader/ShieldFX.shader
// https://cyangamedev.wordpress.com/2019/09/04/forcefield-shader-breakdown-simple/

sampler2D _CameraDepthTexture;



#endif