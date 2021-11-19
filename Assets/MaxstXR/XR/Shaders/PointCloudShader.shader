// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MaxstAR/PointCloudShader"
{
    Properties {
        _BlueTex ("IMAGE (RGBA)", 2D) = "black" {}
    }
    SubShader 
    {
        Tags { "Queue" = "Transparent" }
        Pass 
        {
        	ZWrite Off
            Cull Off
            Lighting Off

            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            float _FeatureSize;
            v2f vert (appdata v,unsigned int vid : SV_VertexID)
            {

                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            sampler2D _BlueTex;
            float4 frag (v2f i) : COLOR
            {
                return i.color;
                //return float4(1.0, 0.0, 0.0, 1.0);
                //return tex2D(_BlueTex, i.uv);
            }
            ENDCG
        }
    }
}