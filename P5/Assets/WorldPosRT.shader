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
            #pragma geometry geo
            #pragma require setrtarrayindexfromanyshader geometry

            #include "UnityCG.cginc"

            struct screen_data_vertex
            {
                float2 uv;
                float3 ray_to_far_plane;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            StructuredBuffer<screen_data_vertex> screen_data_vertex_buffer;

            struct screen_data_geometry
            {
                float2 uv : TEXCOORD0;
                // float3 ray_to_far_plane : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO // Eye
            };
            
            screen_data_geometry vert (const uint vertex_id : SV_VertexID)
            {
                // Input
                const screen_data_vertex screen_data = screen_data_vertex_buffer[vertex_id];

                // Output
                UNITY_SETUP_INSTANCE_ID(screen_data); // Setup eye from instance index
                
                screen_data_geometry o;
                UNITY_INITIALIZE_OUTPUT(screen_data_geometry, o); // Zero initialize
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); // Set eye index, which also sets SV_RenderTargetArrayIndex
                
                //o.ray_to_far_plane = screen_data.ray_to_far_plane;
                o.uv = screen_data.uv;
                
                return o;
            }

            struct screen_data_fragment
            {
                float4 clip_space_pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 ray_to_far_plane : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO // Eye
            };

            screen_data_fragment geo_to_frag(const in screen_data_geometry geo)
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(geo); // Setup eye from input
                screen_data_fragment frag;
                UNITY_INITIALIZE_OUTPUT(screen_data_fragment, frag); // Zero initialize
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(frag); // Set eye index, which also sets SV_RenderTargetArrayIndex

                frag.clip_space_pos = float4(geo.uv*2-1,0,1);

                const float3 ray_view_space = mul(unity_CameraInvProjection,frag.clip_space_pos)*_ProjectionParams.z;
                frag.ray_to_far_plane = mul(unity_CameraToWorld, ray_view_space);
                
                frag.uv = geo.uv;
                #if UNITY_UV_STARTS_AT_TOP
                frag.uv.y = 1-frag.uv.y;
                #endif
                
                // frag.ray_to_far_plane = geo.ray_to_far_plane;
                
                return frag;
            }
            
            [maxvertexcount(8)]
			void geo(triangle screen_data_geometry tri_in[3], inout TriangleStream<screen_data_fragment> tri_out)
            {
                tri_out.Append(geo_to_frag(tri_in[0]));
                tri_out.Append(geo_to_frag(tri_in[1]));
                tri_out.Append(geo_to_frag(tri_in[2]));                
            }

            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            
            fixed4 frag (const screen_data_fragment i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); // Setup eye from input

                const float depth_proportional = DecodeFloatRG(SAMPLE_RAW_DEPTH_TEXTURE(_CameraDepthTexture, i.uv).rg);
                const float depth01 = Linear01Depth(depth_proportional);
                const float3 world_pos = _WorldSpaceCameraPos + depth01*i.ray_to_far_plane;

                return float4(world_pos,1);
                // return float4(lerp(float2(1,0),float2(0,1),unity_StereoEyeIndex),0,1);
            }
            
            ENDCG
        }
    }
}