Shader "Custom/SpriteOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineSize ("Outline Size", Float) = 2
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
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

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        // ========== Pass 1: 描边 ==========
        // 对透明像素检测周围是否有不透明像素，有则输出描边颜色
        // 由于 Pass 2 会覆盖 sprite 本体，描边只会在 sprite 外部可见
        Pass
        {
            Name "OUTLINE"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            fixed4 _OutlineColor;
            float _OutlineSize;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color;
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif
                return OUT;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 texel = _MainTex_TexelSize.xy * _OutlineSize;
                float centerAlpha = tex2D(_MainTex, i.texcoord).a;

                // 当前像素已不透明 → 交给 Pass 2 渲染本体，这里输出透明
                if (centerAlpha >= 0.5)
                    return fixed4(0,0,0,0);

                // 采样 8 邻域 alpha
                float a = 0;
                a += tex2D(_MainTex, i.texcoord + float2(texel.x, 0)).a;
                a += tex2D(_MainTex, i.texcoord + float2(-texel.x, 0)).a;
                a += tex2D(_MainTex, i.texcoord + float2(0, texel.y)).a;
                a += tex2D(_MainTex, i.texcoord + float2(0, -texel.y)).a;
                a += tex2D(_MainTex, i.texcoord + float2(texel.x, texel.y)).a;
                a += tex2D(_MainTex, i.texcoord + float2(-texel.x, texel.y)).a;
                a += tex2D(_MainTex, i.texcoord + float2(texel.x, -texel.y)).a;
                a += tex2D(_MainTex, i.texcoord + float2(-texel.x, -texel.y)).a;

                // 周围有不透明像素 → 这是边缘，输出描边
                if (a > 0)
                {
                    fixed4 c = _OutlineColor * i.color;
                    c.a = saturate(a) * c.a;
                    c.rgb *= c.a;
                    return c;
                }

                return fixed4(0,0,0,0);
            }
            ENDCG
        }

        // ========== Pass 2: 正常渲染 Sprite ==========
        Pass
        {
            Name "SPRITE"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _RendererColor;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _RendererColor;
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif
                return OUT;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.texcoord) * i.color;
                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }
}
