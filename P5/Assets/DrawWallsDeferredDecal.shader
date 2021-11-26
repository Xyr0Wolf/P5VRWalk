Shader "Hidden/DrawWallsDeferredDecal"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        blend srcalpha OneMinusSrcAlpha

        pass
        {
            CGPROGRAM
            #include "GPUBasedLogging/StandardImageEffectVertexShader.cginc"

            sampler2D world_space_rt;
            sampler2D _MainTex;
            
            fixed4 frag(v2f input) : SV_Target
            {
                const float3 pos = tex2D(world_space_rt, input.uv).xyz;
                const float2 uv = (pos.xz+5)*0.1;
                clip(1-uv);
                clip(uv);
                
                return tex2D(_MainTex, uv)*saturate(2-pos.y);
            }
            ENDCG
        }
    }
}