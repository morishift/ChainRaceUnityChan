Shader "UI/CircleMask"
{
    Properties
    {
        _CenterX  ("Center X", Range(0, 1)) = 0.5
        _CenterY  ("Center Y", Range(0, 1)) = 0.5
        _Radius   ("Radius",   Range(0, 2)) = 0.0
        _Softness ("Softness", Range(0, 0.5)) = 0.02
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Overlay"
            "RenderType" = "Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos      : SV_POSITION;
                float2 uv       : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            float _CenterX;
            float _CenterY;
            float _Radius;
            float _Softness;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;
                o.screenPos = ComputeScreenPos(o.pos);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Normalized screen position (0-1), independent of mesh UV
                float2 sp = i.screenPos.xy / i.screenPos.w;

                // Correct for aspect ratio so distance is measured in physical pixels
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 pos    = float2(sp.x * aspect, sp.y);
                float2 center = float2(_CenterX * aspect, _CenterY);

                float dist = distance(pos, center);
                // Disable softness when radius is zero to ensure fully black screen
                // saturate() clamps value to 0-1 range
                float softness = _Softness * saturate(_Radius * 100.0);
                float alpha = smoothstep(_Radius - softness, _Radius + softness, dist);
                return fixed4(0, 0, 0, alpha);
            }

            ENDCG
        }
    }
}
