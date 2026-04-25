Shader "UI/GradientSelector"
{
    Properties
    {
        _ColorA ("Color A", Color) = (1,0,0,1)
        _ColorB ("Color B", Color) = (1,1,0,1)
        _ColorC ("Color C (optional)", Color) = (0,1,0,1)
        _ColorD ("Color D (optional)", Color) = (0,1,1,1)
        _Stops   ("Stops A,B,C,D (0-1)", Vector) = (0, 0.33, 0.66, 1)
        _UseCD   ("Use C/D (0/1)", Float) = 1
        _Angle   ("Angle (deg, linear)", Range(0,360)) = 0
        _Radial  ("Radial (0/1)", Float) = 0
        _Gamma   ("Gamma Correct Blend (0/1)", Float) = 1
        _Alpha   ("Global Alpha", Range(0,1)) = 1

        // UI stencil/clip props (match Unity UI-Default)
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "UI-GradientSelector"
            Tags { "LightMode" = "UniversalForward" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;   // we’ll use this as normalized rect UV
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float4 color    : COLOR;
                float2 uv       : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
            };

            fixed4 _ColorA, _ColorB, _ColorC, _ColorD;
            float4 _Stops;  // x=a, y=b, z=c, w=d
            float _UseCD;
            float _Angle;
            float _Radial;
            float _Gamma;
            float _Alpha;

            // UI includes
            float4 _ClipRect;
            float _UseUIAlphaClip;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex   = UnityObjectToClipPos(v.vertex);
                o.uv       = v.texcoord;      // 0..1 across rect
                o.color    = v.color;
                o.worldPos = v.vertex;
                return o;
            }

            // gamma helpers
            float3 toLinear(float3 c)  { return GammaToLinearSpace(c); }
            float3 toSRGB(float3 c)    { return LinearToGammaSpace(c); }

            // piecewise gradient evaluate (supports 2 or 4 stops)
            fixed4 evalGradient(float t)
            {
                t = saturate(t);

                if (_UseCD < 0.5) // 2-color
                {
                    float3 a = _ColorA.rgb;
                    float3 b = _ColorB.rgb;

                    if (_Gamma > 0.5) { a = toLinear(a); b = toLinear(b); }
                    float3 c = lerp(a, b, t);
                    if (_Gamma > 0.5) c = toSRGB(c);
                    return fixed4(c, lerp(_ColorA.a, _ColorB.a, t));
                }
                else // 4-color with stops
                {
                    float sA = saturate(_Stops.x);
                    float sB = saturate(_Stops.y);
                    float sC = saturate(_Stops.z);
                    float sD = saturate(_Stops.w);

                    // ensure monotonic (defensive)
                    sB = max(sB, sA + 1e-4);
                    sC = max(sC, sB + 1e-4);
                    sD = max(sD, sC + 1e-4);

                    fixed4 cols[4] = { _ColorA, _ColorB, _ColorC, _ColorD };
                    float stops[4] = { sA, sB, sC, sD };

                    int seg = 0;
                    if (t <= stops[1]) seg = 0;
                    else if (t <= stops[2]) seg = 1;
                    else if (t <= stops[3]) seg = 2;
                    else seg = 2; // clamp to last segment (C->D)

                    float t0 = stops[seg];
                    float t1 = stops[seg+1];
                    float u  = saturate((t - t0) / max(1e-5, (t1 - t0)));

                    float3 c0 = cols[seg].rgb;
                    float3 c1 = cols[seg+1].rgb;
                    float  a0 = cols[seg].a;
                    float  a1 = cols[seg+1].a;

                    if (_Gamma > 0.5) { c0 = toLinear(c0); c1 = toLinear(c1); }
                    float3 c = lerp(c0, c1, u);
                    if (_Gamma > 0.5) c = toSRGB(c);

                    return fixed4(c, lerp(a0, a1, u));
                }
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // UI rect clip
                #ifdef UNITY_UI_CLIP_RECT
                float2 inside = UnityGet2DClipping(i.worldPos.xy, _ClipRect);
                if (_UseUIAlphaClip > 0.5 && inside < 0.001) discard;
                #endif

                // compute t along gradient
                float t;
                if (_Radial > 0.5)
                {
                    // center at 0.5,0.5
                    float2 d = i.uv - float2(0.5, 0.5);
                    t = saturate(length(d) * 2.0); // radius ~0.5->1
                }
                else
                {
                    // rotate uv by angle and project on x
                    float ang = radians(_Angle);
                    float2x2 R = float2x2(cos(ang), -sin(ang), sin(ang), cos(ang));
                    float2 p = mul(R, (i.uv - 0.5));
                    t = saturate(p.x + 0.5);
                }

                fixed4 col = evalGradient(t);
                col.a *= _Alpha;
                return col;
            }
            ENDCG
        }
    }

    FallBack "UI/Unlit/Transparent"
}
