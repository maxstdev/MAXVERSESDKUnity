// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//-----------------------------------------------------------------------
// <copyright file="ARBackground.shader" company="Google LLC">
//
// Copyright 2018 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

Shader "MaxstAR/ARBackground"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
    }

    SubShader
    {
        Pass
        {
            //ZWrite Off
            //Cull Off

            GLSLPROGRAM

            #pragma only_renderers gles3 gles

            // #ifdef SHADER_API_GLES3 cannot take effect because
            // #extension is processed before any Unity defined symbols.
            // Use "enable" instead of "require" here, so it only gives a
            // warning but not compile error when the implementation does not
            // support the extension.

            #extension GL_OES_EGL_image_external_essl3 : enable
            #extension GL_OES_EGL_image_external : enable

            #ifdef VERTEX

            varying vec2 textureCoord;

            void main()
            {
                textureCoord = gl_MultiTexCoord0.xy;

                gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
            }

            #endif

            #ifdef FRAGMENT
            precision mediump float;
            varying vec2 textureCoord;
            uniform samplerExternalOES _MainTex;

            void main()
            {
                vec3 mainTexColor;

                #ifdef SHADER_API_GLES3
                mainTexColor = texture(_MainTex, textureCoord).rgb;
                #else
                mainTexColor = textureExternal(_MainTex, textureCoord).rgb;
                #endif

                gl_FragColor = vec4(mainTexColor, 1.0);
            }

            #endif

            ENDGLSL
        }
    }

    SubShader {
        Pass {
            //ZWrite Off
            Cull Off

            CGPROGRAM

            #pragma exclude_renderers gles3 gles
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos (v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed3 tt;
                tt.r = 1.0;
                tt.g = 0.0;
                tt.b = 0.0;
                return fixed4 (tt, 1);
            }
            ENDCG

        }
    }
}
