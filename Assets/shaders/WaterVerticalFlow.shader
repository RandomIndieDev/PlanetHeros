Shader "UI/Water Vertical Range Fill"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _HeadProgress ("Head Progress", Range(0,1)) = 0
        _TailProgress ("Tail Progress", Range(0,1)) = 0
        _Softness ("Edge Softness", Range(0.001,0.2)) = 0.02
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;

            float _HeadProgress;
            float _TailProgress;
            float _Softness;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;

                // Convert UV to top-down progress.
                // top = 0, bottom = 1
                float yProgress = 1.0 - i.uv.y;

                // Visible only between tail and head.
                float afterTail = smoothstep(_TailProgress - _Softness, _TailProgress + _Softness, yProgress);
                float beforeHead = 1.0 - smoothstep(_HeadProgress - _Softness, _HeadProgress + _Softness, yProgress);

                float mask = afterTail * beforeHead;

                col.a *= mask;
                return col;
            }
            ENDCG
        }
    }
}