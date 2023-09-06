Shader "Paperticket/AlphaTransColor3D" {
    Properties{
        _Color("Color", Color) = (1,1,1,1)
        _TransparentColor("Transparent Color", Color) = (1,1,1,1)
        _Threshold("Threshhold", Float) = 0.1
        _MainTex("Albedo (RGB)", 2D) = "white" {}

        [Toggle(_)]_SwapEyes("Swap-Eyes", Int) = 0
        [HideInInspector] __dirty("", Int) = 1
    }
        SubShader{
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IsEmissive" = "true"   }
            Cull Back
            LOD 200

            CGPROGRAM

            #pragma target 3.5
            #pragma surface surf Unlit keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 

            //#pragma surface surf Lambert alpha

            uniform sampler2D _MainTex;
            uniform int _SwapEyes;

            // sampler2D _MainTex;

            //struct Input {
            //    float2 uv_MainTex;
            //};
            struct Input
            {
                float2 vertexToFrag62;
                float2 uv_MainTex;
            };


            fixed4 _Color;
            fixed4 _TransparentColor;
            half _Threshold;

            void vertexDataFunc(inout appdata_full v, out Input o)
            {
                UNITY_INITIALIZE_OUTPUT(Input, o);
                float2 temp_cast_0 = (0.5).xx;
                float2 temp_cast_1 = (0.5).xx;
                float2 uv_TexCoord37 = v.texcoord.xy * temp_cast_0 + temp_cast_1;
                float2 temp_cast_2 = (0.5).xx;
                float2 temp_cast_3 = (0.0).xx;
                float2 uv_TexCoord36 = v.texcoord.xy * temp_cast_2 + temp_cast_3;
                float localStereoEyeIndex63 = (unity_StereoEyeIndex);
                float lerpResult67 = lerp(localStereoEyeIndex63, (-localStereoEyeIndex63 + 1.0), (float)_SwapEyes);
                float lerpResult34 = lerp(uv_TexCoord37.y, uv_TexCoord36.y, lerpResult67);
                float2 appendResult53 = (float2(v.texcoord.xy.x, lerpResult34));
                o.vertexToFrag62 = appendResult53;
            }

            inline half4 LightingUnlit(SurfaceOutput s, half3 lightDir, half atten)
            {
                return half4 (0, 0, 0, s.Alpha);
            }



            void surf(Input IN, inout SurfaceOutput o) {
                // Read color from the texture
                //half4 c = tex2D(_MainTex, IN.uv_MainTex);
                half4 c = tex2D(_MainTex, IN.vertexToFrag62);

                // Output colour will be the texture color * the vertex colour
                half4 output_col = c * _Color;

                //calculate the difference between the texture color and the transparent color
                //note: we use 'dot' instead of length(transparent_diff) as its faster, and
                //although it'll really give the length squared, its good enough for our purposes!
                half3 transparent_diff = c.xyz - _TransparentColor.xyz;
                half transparent_diff_squared = dot(transparent_diff,transparent_diff);

                //if colour is too close to the transparent one, discard it.
                //note: you could do cleverer t$$anonymous$$ngs like fade out the alpha
                if (transparent_diff_squared < _Threshold)
                    discard;

                //output albedo and alpha just like a normal shader
                ////o.Albedo = output_col.rgb;
                
                o.Emission = tex2D(_MainTex, IN.vertexToFrag62).rgb;
                //o.Emission = output_col.rgb;
                o.Alpha = output_col.a;
            }
            ENDCG
        }
            FallBack "Diffuse"
}