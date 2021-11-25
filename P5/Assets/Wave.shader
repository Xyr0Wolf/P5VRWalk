Shader "Unlit/Wave"
{
    Properties
    {
        _ColorA ("Color A", color) = (0.1,0.1,.9,1)
        _ColorB ("Color B", color) = (0.2,0.2,1,0.2)
        _Frequency ("Frequency", float) = 50
        _Speed ("Speed", float) = 1
        _FalloffGain ("Falloff Gain", float) = 1
        _FalloffOffset ("Falloff Offset", float) = 1
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        cull off
        zwrite off
        blend SrcAlpha one
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 clip_space_pos : SV_POSITION;
                float4 object_space_pos : POSITION1;
                float4 world_space_pos : POSITION2;
                float3 normal : NORMAL;
            };

            float4 _ColorA;
            float4 _ColorB;
            float _Frequency;
            float _Speed;
            float _FalloffGain;
            float _FalloffOffset;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.object_space_pos = v.vertex;
                o.world_space_pos = mul(unity_ObjectToWorld, v.vertex);
                o.clip_space_pos = mul(unity_MatrixVP, o.world_space_pos);
                o.normal = v.normal;
                return o;
            }

            fixed4 frag (const v2f i) : SV_Target
            {
                clip(0.1-i.normal.y);
                
                float side_wave_val = sin((i.object_space_pos.y+1)*_Frequency*UNITY_TWO_PI-_Time.y*_Speed);
                side_wave_val *= side_wave_val; 
                side_wave_val = smoothstep(0.2,1,side_wave_val);
                side_wave_val *= saturate(1-(i.object_space_pos.y+_FalloffOffset)*_FalloffGain);
                const float4 wave_col = lerp(_ColorA,_ColorB,side_wave_val);
                
                const float is_bottom = step(i.normal.y, -0.1);
                float bottom_wave_val = 0.5+0.5*sin(length(i.object_space_pos.xz*2)*_Frequency*UNITY_TWO_PI*2-_Time.y*_Speed*1);
                bottom_wave_val *= bottom_wave_val; 
                bottom_wave_val = smoothstep(0.2,1,bottom_wave_val);
                const float4 bottom_col = lerp(_ColorA,_ColorB,bottom_wave_val);
                
                return wave_col*(1-is_bottom)+bottom_col*is_bottom;
            }
            ENDCG
        }
    }
}
