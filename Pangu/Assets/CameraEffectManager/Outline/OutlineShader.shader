// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

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
				return tex2D(_MainTex, i.texcoord);
			}
			ENDCG
		}

		Pass{
			ZWrite off Fog{ mode off } Cull off ZTest Always Blend Off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			uniform float4 _MainTex_TexelSize;
			float4 offsets;
			struct v2f {
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;

				float4 uv01 : TEXCOORD1;
				float4 uv23 : TEXCOORD2;
				float4 uv45 : TEXCOORD3;
			};
			v2f vert(appdata_img v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv.xy = v.texcoord.xy;

				o.uv01 = o.uv.xyxy + offsets.xyxy * float4(1, 1, -1, -1);
				o.uv23 = o.uv.xyxy + offsets.xyxy * float4(1, 1, -1, -1) * 1.5;
				o.uv45 = o.uv.xyxy + offsets.xyxy * float4(1, 1, -1, -1) * 2;
				return o;
			}
			half4 frag(v2f i) : COLOR{
				half4 color = float4 (0,0,0,0);

				color += 0.4 * tex2D(_MainTex, i.uv);
				color += 0.2 * tex2D(_MainTex, i.uv01.xy);
				color += 0.2 * tex2D(_MainTex, i.uv01.zw);
				color += 0.05 * tex2D(_MainTex, i.uv23.xy);
				color += 0.05 * tex2D(_MainTex, i.uv23.zw);
				color += 0.05 * tex2D(_MainTex, i.uv45.xy);
				color += 0.05 * tex2D(_MainTex, i.uv45.zw);
				return color * 2;
			}
			ENDCG
		}

		Pass{
			ZWrite off Fog{ mode off } Cull off ZTest Always 
			BlendOP RevSub
			Blend One One
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#pragma fragmentoption ARB_precision_hint_fastest 

			sampler2D _MainTex;
			struct v2f {
				float4 pos : SV_POSITION;
				float4 uv:	TEXCOORD0;
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				return o;
			}

			fixed4 frag(v2f i) :COLOR
			{
				fixed4 c = tex2D(_MainTex,i.uv);
				return c;
			}
				ENDCG
			}

				Pass{
					ZWrite off Fog{ mode off } Cull off ZTest Always
					Blend SrcAlpha OneMinusSrcAlpha
					CGPROGRAM
					#pragma vertex vert
					#pragma fragment frag
					#include "UnityCG.cginc"
					#pragma fragmentoption ARB_precision_hint_fastest 

				sampler2D g_BlurSilhouette;
				sampler2D _MainTex;
				uniform float4 _MainTex_TexelSize;
				struct v2f {
					float4 pos : SV_POSITION;
					float4 uv:	TEXCOORD0;
					float4 uv2: TEXCOORD1;
				};

				v2f vert(appdata_base v)
				{
					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);
					o.uv = v.texcoord;
					o.uv2 = v.texcoord;
#if UNITY_UV_STARTS_AT_TOP
					//if (_MainTex_TexelSize.y < 0)
					{
						o.uv.y = 1 - o.uv.y;
						o.uv2.y = 1 - o.uv.y;
					}
#endif		
					return o;
				}

				fixed4 frag(v2f i) :COLOR
				{
					fixed4 c = tex2D(_MainTex,i.uv);
					fixed4 blur = tex2D(g_BlurSilhouette, i.uv2);
					return fixed4(blur.rgb * blur.a + c.rgb*(1 - blur.a), 1);
				}
				ENDCG
			}
	}
}
