Shader "Custom/FakeDepthInset"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _HighlightColor("Highlight Color", Color) = (1,1,1,1)
        _ShadowColor("Shadow Color", Color) = (0,0,0,1)
        _DepthStrength("Depth Strength", Range(0,2)) = 1.0
        _Curvature("Curvature", Range(0.1,5)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200
        Cull Off
        Lighting Off
        ZWrite On

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
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float4 _BaseColor;
            float4 _HighlightColor;
            float4 _ShadowColor;
            float _DepthStrength;
            float _Curvature;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv * 2.0 - 1.0; // center UVs (-1..1)
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Compute radial depth: how far from center
                float2 uv = i.uv;
                float dist = length(uv);

                // Fake concave shading curve
                float depth = pow(saturate(1.0 - dist * _Curvature), 2.0);

                // Fake lighting: darker in center, lighter at edges
                float light = depth * 0.5 + 0.5;
                light = pow(light, _DepthStrength * 2.0);

                fixed3 shaded = lerp(_ShadowColor.rgb, _HighlightColor.rgb, light);
                shaded = lerp(_BaseColor.rgb, shaded, _DepthStrength);

                return fixed4(shaded, 1);
            }
            ENDCG
        }
    }

    FallBack "Unlit/Color"
}
