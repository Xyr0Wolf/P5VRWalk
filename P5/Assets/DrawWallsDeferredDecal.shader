Shader "Hidden/DrawWallsDeferredDecal"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        pic ("Picture", 2D) = "yellow" {}
    }
    SubShader
    {
        //blendop RevSub
        //blend srcalpha One

        pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma require setrtarrayindexfromanyshader

            #include "UnityCG.cginc"

            UNITY_DECLARE_SCREENSPACE_TEXTURE(world_space_rt);
            //sampler2D _MainTex; // Uv map floor
            //sampler2D pic; // picture to put on line
            
            //UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthNormalsTexture);
            
            fixed4 frag(const v2f_img input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                /*
                //const float3 view_normal = DecodeViewNormalStereo(SAMPLE_RAW_DEPTH_TEXTURE(_CameraDepthNormalsTexture, input.uv));
                //const float3 world_normal = mul(unity_CameraToWorld, view_normal);
                //clip(world_normal.y-0.9);
                */
                
                const float3 pos = UNITY_SAMPLE_SCREENSPACE_TEXTURE(world_space_rt, input.uv).xyz;
                const float2 uv = (pos.xz+10)*0.05;
                clip(1-uv);
                clip(uv);
                /*
                float2 line_uv = tex2D(_MainTex, uv);
                //clip(line_uv.y-0.2);
                //clip(0.25-line_uv.y);
                //line_uv.y = smoothstep(0.2,0.25,line_uv.y);
                
                const uint num = floor(line_uv.x);
                //const float2 offset = (sin(2*float2(num,num+0.1))+sin(float2(num+0.1,num)))*0.5;
                //const float4 col = tex2D(pic,line_uv.rg+offset)*line_uv.w;
                //const float dist = saturate(distance(frac(line_uv.xy+offset), 0.5));

                const float a = line_uv.y-0.5;
                const float b = a+(sin(0.1*line_uv.x*UNITY_PI)+sin(UNITY_PI * line_uv.x))*0.1;
                const float c = exp(-b*b);
                const float4 col = smoothstep(0.99,1,c);
                */
                return float4(lerp(float2(1,0),float2(0,1),unity_StereoEyeIndex),0,1);
                //return 0;
                //return 0.4*(1-saturate(0.5*unity_FogParams.x*distance(_WorldSpaceCameraPos,pos)))*float4(0,1,1,1);
            }
            ENDCG
        }
    }
}