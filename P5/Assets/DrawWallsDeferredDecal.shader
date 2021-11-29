Shader "Hidden/DrawWallsDeferredDecal"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        pic ("Picture", 2D) = "yellow" {}
    }
    SubShader
    {
        blendop RevSub
        blend srcalpha One

        pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D world_space_rt;
            sampler2D _MainTex;
            sampler2D pic;

            float4 hue_to_rgb(const in float h)
            {
                float r = abs(h * 6 - 3) - 1;
                float g = 2 - abs(h * 6 - 2);
                float b = 2 - abs(h * 6 - 4);
                return saturate(float4(r,g,b,1));
            }
            
            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthNormalsTexture);
            float4x4 CameraToWorld;
            
            fixed4 frag(v2f_img input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                //const float3 view_normal = DecodeViewNormalStereo(SAMPLE_RAW_DEPTH_TEXTURE(_CameraDepthNormalsTexture, input.uv));
                //const float3 world_normal = mul(unity_CameraToWorld, view_normal);
                //clip(world_normal.y-0.9);
                
                const float3 pos = tex2D(world_space_rt, input.uv).xyz;
                const float2 uv = (pos.xz+10)*0.05;
                clip(1-uv);
                clip(uv);
                
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
                return 0.4*(1-saturate(0.5*unity_FogParams.x*distance(_WorldSpaceCameraPos,pos)))*col;
            }
            ENDCG
        }
    }
}