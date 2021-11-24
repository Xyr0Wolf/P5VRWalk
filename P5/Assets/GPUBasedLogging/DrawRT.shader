Shader "Unlit/DrawRT"
{
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #include "Assets/GPUBasedLogging/StandardImageEffectVertexShader.cginc"

            sampler2D _MainTex;

            fixed4 frag (const v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}
