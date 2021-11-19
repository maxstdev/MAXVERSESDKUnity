Shader "Maxst/IBR Cull Back"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Texture1", 2D) = "white" {}
        _MainTex2("Texture2", 2D) = "white" {}
    }
    
    SubShader
    {
        Tags { "RenderType" = "Geometry" "Queue" = "Geometry-1000"}
        LOD 100

        Pass
        {
            Cull Back
            ZTest LEqual
            ZWrite On
            Blend Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Ibr.cginc"

            ENDCG
        }

        Pass
        {
            Cull Off
            ZTest Always
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Vis.cginc"

            ENDCG
        }
    }
}
