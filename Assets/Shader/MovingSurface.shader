Shader "Custom/MovingSurface"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _TilingX ("Tiling X", Float) = 1
        _TilingY ("Tiling Y", Float) = 1
        _OffsetX ("Offset X", Float) = 0
        _OffsetY ("Offset Y", Float) = 0
        _Brightness ("Brightness", Range(0.5, 2)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"

            struct appdata 
            { 
                float4 vertex : POSITION; 
                float2 uv : TEXCOORD0; 
            };
            
            struct v2f 
            { 
                float2 uv : TEXCOORD0; 
                UNITY_FOG_COORDS(1) 
                float4 vertex : SV_POSITION; 
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _TilingX, _TilingY;
            float _OffsetX, _OffsetY;
            fixed4 _Color;
            float _Brightness;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Apply tiling
                float2 tiledUV = i.uv * float2(_TilingX, _TilingY);

                // Apply offset from CPU (accumulated over time)
                tiledUV += float2(_OffsetX, _OffsetY);

                // Sample texture with looping
                fixed4 col = tex2D(_MainTex, frac(tiledUV)) * _Color;
                col.rgb *= _Brightness;
                
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}