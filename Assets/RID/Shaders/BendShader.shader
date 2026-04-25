Shader "Custom/BendShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _BendPoint ("Bend Point (World)", Vector) = (0,0,0,0)
        _BendStrength ("Bend Strength", Range(0, 1)) = 0.2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            HLSLPROGRAM
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
                float4 pos : SV_POSITION;
                float3 normal : NORMAL;
                float3 worldPos : TEXCOORD0;
            };

            float4 _Color;
            float4 _BendPoint;
            float _BendStrength;

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                // Direction from vertex to bend point
                float3 dir = _BendPoint.xyz - worldPos;

                // Distance-based falloff (closer bends more)
                float dist = length(dir);
                float falloff = saturate(1.0 - dist * 0.1); // tweak 0.1 for radius

                // Bend offset (move vertex slightly toward point)
                float3 offset = dir * _BendStrength * falloff;

                worldPos += offset;

                o.pos = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0));
                o.worldPos = worldPos;
                o.normal = v.normal;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _Color;
            }
            ENDHLSL
        }
    }
}
