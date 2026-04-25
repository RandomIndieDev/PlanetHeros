Shader "Unlit/AdvancedGradient"
{
    Properties
    {
        _ColorA ("Color A (Start)", Color) = (0.2, 0.3, 0.7, 1)
        _ColorB ("Color B (End)", Color) = (0.8, 0.9, 1.0, 1)
        _GradientType ("Gradient Type", Range(0,3)) = 0 // 0=Vertical, 1=Horizontal, 2=Diagonal, 3=Radial
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Background" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; };

            float4 _ColorA, _ColorB;
            float _GradientType;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float t;

                if (_GradientType < 0.5)
                    t = i.uv.y;                         // Vertical
                else if (_GradientType < 1.5)
                    t = i.uv.x;                         // Horizontal
                else if (_GradientType < 2.5)
                    t = (i.uv.x + i.uv.y) * 0.5;        // Diagonal
                else
                {
                    // Radial / Spherical gradient
                    float2 center = float2(0.5, 0.5);
                    float dist = distance(i.uv, center);
                    t = saturate(dist * 2.0);           // Adjust falloff
                    t = pow(t, 1.2);                    // Optional curve tweak
                }

                return lerp(_ColorA, _ColorB, t);
            }
            ENDCG
        }
    }
}
