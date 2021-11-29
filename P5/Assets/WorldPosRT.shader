Shader "Hidden/WorldPosRT"
{
    SubShader
    {
        Pass
        {
            cull off
            zwrite off
            ztest always
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct screen_data_vertex
            {
                float2 uv;
                float3 ray_to_far_plane;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            StructuredBuffer<screen_data_vertex> screen_data_vertex_buffer;

            struct screen_data_fragment
            {
                float4 clip_space_pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 ray_to_far_plane : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            screen_data_fragment vert (const uint vertex_id : SV_VertexID)
            {
                // Input
                const screen_data_vertex screen_data = screen_data_vertex_buffer[vertex_id];

                // Output
                screen_data_fragment o;

                UNITY_SETUP_INSTANCE_ID(screen_data);
                UNITY_INITIALIZE_OUTPUT(screen_data_fragment, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                o.clip_space_pos = float4(screen_data.uv*2-1,0,1);

                o.uv = screen_data.uv;
                #if UNITY_UV_STARTS_AT_TOP
                o.uv.y = 1-o.uv.y;
                #endif
                
                o.ray_to_far_plane = screen_data.ray_to_far_plane;
                
                return o;
            }

            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            
            fixed4 frag (const screen_data_fragment i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                const float depth_proportional = DecodeFloatRG(SAMPLE_RAW_DEPTH_TEXTURE(_CameraDepthTexture, i.uv).rg);
                const float depth01 = Linear01Depth(depth_proportional);
                const float3 world_pos = _WorldSpaceCameraPos + depth01*i.ray_to_far_plane;
                
                return float4(world_pos, 1);
            }
            ENDCG
        }
    }
}