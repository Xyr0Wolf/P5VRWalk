Shader "Unlit/VRTest"
{
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma require setrtarrayindexfromanyshader
            
            #include "UnityCG.cginc"
            
            fixed4 frag (const v2f_img i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                
                return float4(lerp(float2(1,0),float2(0,1),unity_StereoEyeIndex),0,1);
            }
            ENDCG
        }
    }
}