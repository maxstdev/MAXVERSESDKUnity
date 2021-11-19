#include "UnityCG.cginc"

struct appdata
{
    float4 vertex : POSITION;
};

struct v2f
{
    UNITY_FOG_COORDS(1)
    float4 vertex : SV_POSITION;
};

fixed4 _Color;
fixed4 _runtimeData = fixed4(1, 0, 0, 0);

v2f vert(appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    UNITY_TRANSFER_FOG(o,o.vertex);
    return o;
}

fixed4 frag(v2f i) : SV_Target
{
    // sample the texture
    fixed4 col = _Color;
    col.a *= _runtimeData.x;
    // apply fog
    UNITY_APPLY_FOG(i.fogCoord, col);
    return col;
}