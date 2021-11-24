Shader "Hidden/SaturateRedRT"
{
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #include "Assets/GPUBasedLogging/StandardImageEffectVertexShader.cginc"
            
            sampler2D _MainTex;

            fixed4 frag (v2f i) : SV_Target
            {
                return smoothstep(0,0.05,tex2D(_MainTex, i.uv).r);
            }
            ENDCG
        }
    }
}