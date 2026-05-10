Shader "Custom/SpriteOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (0.4,0.26,0.13,1)
        _OutlineWidth ("Outline Width", Float) = 0.5
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

        // ========== Pass 1: 顶点外扩描边（棕色影子）==========
        // 利用 UV 坐标推导四个顶点的外扩方向，将矩形整体向外撑开
        // 膨胀区域全部涂成棕色，为 Pass 2 的 sprite 本体提供底层影子
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
            };

            fixed4 _OutlineColor;
            float _OutlineWidth;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                float4 pos = IN.vertex;

                // 基于 UV 推导外扩方向：将 [0,1] UV 映射到 [-1,1]
                // 四个顶点分别得到 (-1,-1)、(1,-1)、(-1,1)、(1,1)
                // sign 后即为矩形四个角的外法线方向
                float2 uvDir = (IN.texcoord - 0.5) * 2.0;
                float2 extrude = sign(uvDir) * _OutlineWidth;
                pos.xy += extrude;

                OUT.vertex = UnityObjectToClipPos(pos);
                OUT.color = IN.color * _OutlineColor;

                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif

                return OUT;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 c = i.color;
                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }

        // ========== Pass 2: 正常渲染 Sprite（覆盖本体，保留影子外框）==========
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
