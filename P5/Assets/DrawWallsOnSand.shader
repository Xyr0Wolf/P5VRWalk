Shader "Custom/DrawWallsOnSand"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "Queue"="Geometry+1" "ForceNoShadowCasting" = "True"
        }
        Offset -1, -1
        blend srcalpha OneMinusSrcAlpha

        pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 world_space_pos : POSITION1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.world_space_pos = mul(unity_ObjectToWorld, v.vertex);
                o.vertex = mul(unity_MatrixVP, o.world_space_pos);
                return o;
            }

            #ifdef SHADER_API_D3D11
            StructuredBuffer<float2> points : register(t1);
            #endif

            fixed4 frag(v2f input) : SV_Target
            {
                float val = 0;

                for (int i = 0; i < 256; i++)
                {
                    val += saturate(1-2*distance(points[i], input.world_space_pos.xz));
                }
                
                return val;
            }
            ENDCG
        }
        //        
        //        // Physically based Standard lighting model, and enable shadows on all light types
        //        //#pragma surface surf Standard decal:blend
        //        //#pragma target 3.5
        //
        //        struct Input {
        //            float2 uv_MainTex;
        //            float3 worldPos;
        //        };
        //
        //        sampler2D _MainTex;
        //        #ifdef SHADER_API_D3D11
        //        StructuredBuffer<float2> points : register(t1);
        //        #endif
        //        
        //        void surf (Input IN, inout SurfaceOutputStandard o) {
        //            // Albedo comes from a texture tinted by color
        //            const float2 uv = IN.worldPos.xz;
        //            float4 pointA = 0;
        //            //const float dist = distance(IN.worldPos.xz, points[20]);
        //            float4 col = 1;
        //
        //            #ifdef SHADER_API_D3D11
        //            col = float4(uv,0,1);
        //            #endif
        //            
        //            o.Albedo = col.rgb;
        //            o.Alpha = col.a;
        //        }
        //        
        //        ENDCG
    }
    FallBack "Diffuse"
}