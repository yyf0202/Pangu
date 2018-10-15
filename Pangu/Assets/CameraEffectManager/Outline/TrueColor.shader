
Shader "Custom/Effect/Outline"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Outline_Color("Outline_Color", Vector) = (1,1,1,1)
    }
    SubShader
    {
        Pass{
            ZTest Always Cull Off ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord.xy;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return Outline_Color;
            }
            ENDCG}
    }
}