#include "UnityCG.cginc"

#define kMaxRefViews 2

CBUFFER_START(FrameInfo)
    float4x4 frame_MatrixV[kMaxRefViews];
    float4 frame_Data;
CBUFFER_END

inline float3 FrameObjectToViewPos(in float4x4 m, in float3 pos)
{
    return mul(m, mul(unity_ObjectToWorld, float4(pos, 1.0))).xyz;
}

struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
};

struct v2f
{
    float2 uv : TEXCOORD0;
    UNITY_FOG_COORDS(1)
    float4 vertex : SV_POSITION;
    float3 positionOS : VAR_POSITION;
};

sampler2D _MainTex;
sampler2D _MainTex2;
float4 _MainTex_ST;

v2f vert (appdata v)
{
    v2f o;
    o.positionOS = v.vertex.xyz;
    o.vertex = UnityObjectToClipPos(v.vertex.xyz);
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    UNITY_TRANSFER_FOG(o,o.vertex);
    return o;
}

fixed4 frag(v2f i) : SV_Target
{
    float t = frame_Data.x;
    float wSum = 0;
    fixed4 cSum = fixed4(0, 0, 0, 1);
    for (int refViewIdx = 0; refViewIdx < kMaxRefViews; ++refViewIdx)
    {
        float4x4 refWorldToLocal = frame_MatrixV[refViewIdx];
        float3 posRefVS = FrameObjectToViewPos(refWorldToLocal, i.positionOS);
        float theta = atan2(posRefVS.x, posRefVS.z) / UNITY_PI;
        float phi = acos(posRefVS.y / length(posRefVS)) / UNITY_PI;

        fixed4 c;
        float w;
        if (0 == refViewIdx)
        {
            c = tex2D(_MainTex, float2(0.5 + theta * 0.5, phi));
            w = 1 - t;
        }
        else
        {
            c = tex2D(_MainTex2, float2(0.5 + theta * 0.5, phi));
            w = t;
        }
        wSum += w;
        cSum += w * c;
    }
    cSum /= wSum;
    // apply fog
    UNITY_APPLY_FOG(i.fogCoord, cSum);
    return cSum;
}