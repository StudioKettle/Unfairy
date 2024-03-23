Shader "Paperticket/AlphaTransColorCutout" {
    Properties{
        _Color("Color", Color) = (1,1,1,1)
        //_TransparentColor("Transparent Color", Color) = (1,1,1,1)
        //_Threshold("Threshhold", Float) = 0.1
        _MainTex("Albedo (RGB)", 2D) = "white" {}
		_Alpha("Alpha", Float) = 1
        //[Toggle(USE_ALPHA_OFFSET)]
        _UseAlphaOffset("Use Alpha Offset?", Float) = 0
		_AlphaOffset("Alpha Offset", Vector) = (0,0,0,0)
		//_AlphaTex("Alpha Texture", 2D) = "white" {}
    }
        SubShader{
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent"  }



            LOD 200

            CGPROGRAM
            #pragma surface surf Lambert alpha
			//allow instancing
			#pragma multi_compile_instancing


            //#pragma shader_feature USE_ALPHA_OFFSET


            sampler2D _MainTex;

            struct Input {
                float2 uv_MainTex;
            };

            fixed4 _Color;
            //fixed4 _TransparentColor;
            //half _Threshold;
			half _Alpha;
            half _UseAlphaOffset;
            half4 _AlphaOffset;
            
			
            void surf(Input IN, inout SurfaceOutput o) {

                // Read color from the texture
                half4 c = tex2D(_MainTex, IN.uv_MainTex);

                // Output colour will be the texture color * the vertex colour
                half4 output_col = c * _Color;

                ////calculate the difference between the texture color and the transparent color
                ////note: we use 'dot' instead of length(transparent_diff) as its faster, and
                ////although it'll really give the length squared, its good enough for our purposes!
                //half3 transparent_diff = c.xyz - _TransparentColor.xyz;
                //half transparent_diff_squared = dot(transparent_diff,transparent_diff);

                ////if colour is too close to the transparent one, discard it.
                ////note: you could do cleverer things like fade out the alpha
                //if (transparent_diff_squared < _Threshold)
                //    discard;

				float2 alphaUV = IN.uv_MainTex;
				alphaUV.x = alphaUV.x + _AlphaOffset.x;
				alphaUV.y = alphaUV.y + _AlphaOffset.y;
				float alphaCutout = tex2D(_MainTex, alphaUV).r;

                //output albedo and alpha just like a normal shader
                //o.Albedo = output_col.rgb;
                o.Emission = output_col.rgb;
				//o.texcoord.xy = i.texcoord.xy + frac(_Time.y * float2(_speedX, _speedY));
                //o.Alpha = alphaCutout * _Alpha; // _Alpha; // output_col.a;
                o.Alpha = (_UseAlphaOffset * alphaCutout * _Alpha) + (1 - _UseAlphaOffset);

                //#ifdef USE_ALPHA_OFFSET
                //    o.Alpha = alphaCutout * _Alpha; // _Alpha; // output_col.a;
                //#else
                //    o.Alpha = _Alpha; // _Alpha; // output_col.a;
                //#endif

            }
            ENDCG
    }
        FallBack "Diffuse"
}