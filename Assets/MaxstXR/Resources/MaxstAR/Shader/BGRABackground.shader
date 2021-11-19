Shader "MaxstAR/BGRABackground"
{
    Properties {
        _MainTex ("Base (BGRA)", 2D) = "black" {}
    }

    SubShader 
    {
        Pass 
        {
            Cull Off
            ZWrite Off
            Lighting Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
        
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;

            float4 frag (v2f i) : COLOR
            {
                float b = tex2D(_MainTex, i.uv).b;
                float g = tex2D(_MainTex, i.uv).g;
                float r = tex2D(_MainTex, i.uv).r;

                return float4(r, g, b, 1.0);
            }
            ENDCG
        }
    }
}