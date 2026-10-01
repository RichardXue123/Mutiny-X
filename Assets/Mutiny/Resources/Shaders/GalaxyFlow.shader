Shader "Mutiny/GalaxyFlow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Galaxy Texture", 2D) = "white" {}
        [PerRendererData] _GalaxyPhase ("Loop Phase", Float) = 0
        [PerRendererData] _BandCenter ("Bright Band UV Y", Float) = 0.69
        [PerRendererData] _OpaqueBelow ("Opaque Below UV Y", Float) = -1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="False" }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _GalaxyPhase;
            float _BandCenter;
            float _OpaqueBelow;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            v2f vert(appdata input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }
            fixed4 frag(v2f input) : SV_Target
            {
                float2 uv = input.uv;
                float phase = _GalaxyPhase * 6.2831853;
                // Keep all four texture boundaries stationary; never wrap unrelated edge pixels.
                float envelope = smoothstep(0, 0.06, uv.x) * smoothstep(0, 0.06, 1 - uv.x)
                    * smoothstep(0, 0.08, uv.y) * smoothstep(0, 0.08, 1 - uv.y);
                float band = exp(-pow((uv.y - _BandCenter) * 3.6, 2));
                float2 pixels;
                pixels.x = (4 * sin(phase - uv.y * 12) + 2 * sin(phase * 2 - uv.y * 23)) * band;
                pixels.y = 2.5 * sin(phase - uv.x * 16) * band;
                float2 offset = floor(pixels * envelope + 0.5) * _MainTex_TexelSize.xy;
                fixed4 color = tex2D(_MainTex, uv + offset) * input.color;
                if (uv.y < _OpaqueBelow) color.a = input.color.a;
                float light = max(color.r, max(color.g, color.b));
                // Local phases make bright star clusters shimmer independently.
                float twinkle = sin(phase * 2 + uv.x * 57 + uv.y * 39);
                float strength = smoothstep(0.12, 0.65, light) * envelope;
                color.rgb *= 1 + 0.22 * twinkle * strength;
                return color;
            }
            ENDCG
        }
    }
}
