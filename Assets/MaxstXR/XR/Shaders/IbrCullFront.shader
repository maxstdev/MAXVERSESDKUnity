Shader "Maxst/IBR Cull Front"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Texture1", 2D) = "white" {}
        _MainTex2("Texture2", 2D) = "white" {}
    }
    
    SubShader
    {
        Tags { "RenderType" = "Geometry" "Queue" = "Geometry"}
        LOD 100

        Pass
        {
            Cull Front
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
    }
}
