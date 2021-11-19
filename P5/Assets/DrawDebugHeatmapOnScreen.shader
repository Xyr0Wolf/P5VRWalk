Shader "HiddenShader/DrawDebugHeatmapOnScreen"
{
    Properties
    {
        heat_map ("Texture", 2D) = "clear" {}
        time_map ("Texture", 2D) = "clear" {}
        time_map_with_top ("Texture", 2D) = "clear" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2_f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            
            sampler2D heat_map;
            sampler2D time_map;
            sampler2D time_map_with_top;

            v2_f vert (const appdata v)
            {
                v2_f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (const v2_f i) : SV_Target
            {
                const float2 pixel_coord_normal = ((i.uv*_ScreenParams.xy)/_ScreenParams.x)*4;
                clip(float2(3-pixel_coord_normal.x, 1-pixel_coord_normal.y));
                
                float4 sample = tex2D(heat_map, pixel_coord_normal)*step(pixel_coord_normal.x,1);
                sample += tex2D(time_map, pixel_coord_normal-float2(1,0))*step(1,pixel_coord_normal.x)*step(pixel_coord_normal.x,2);
                sample += tex2D(time_map_with_top, pixel_coord_normal-float2(2,0))*step(2,pixel_coord_normal.x);
                
                return float4(sample.xyz, 1);
            }
            ENDCG
        }
    }
}
