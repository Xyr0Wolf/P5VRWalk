Shader "Unlit/Line"
{
    SubShader
    {
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

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //const float a = i.uv.y-0.5;
                //const float b = a+(sin(0.1*i.uv.x*UNITY_PI)+sin(UNITY_PI * i.uv.x))*0.1;
                //const float c = exp(-b*b);
                //const float4 col = float4(0.8,0.8,0.5,1);
                //return 0.3*smoothstep(0.995,1,c);
                return float4(frac(i.uv),0,1);
            }
            ENDCG
        }
    }
}
