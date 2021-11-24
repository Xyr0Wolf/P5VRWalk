Shader "Hidden/DrawWithBellCurve"
{
    SubShader
    {
        Tags {"RenderType"="Opaque"}
        
        // No depth
        ZWrite Off 
        ZTest Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 screen_pos : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screen_pos = o.vertex;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                const float3 screen_pos = i.screen_pos.xyz/i.screen_pos.w;
                const float x = distance(screen_pos.xy, 0);
                const float gauss = exp(-(x*x)/0.2);
                return gauss*saturate(screen_pos.z*screen_pos.z*1000); // simplified gauss func
            }
            ENDCG
        }
    }
}