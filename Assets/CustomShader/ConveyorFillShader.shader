Shader "Custom/ConveyorFillShader"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _OldTex ("Old Sprite Texture", 2D) = "white" {}
        _NewTex ("New Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _FillAmount ("Fill Amount", Range(0, 1)) = 0
        _FillDirection ("Fill Direction", Vector) = (0, 1, 0, 0)
        _FillSmoothness ("Fill Smoothness", Range(0, 0.5)) = 0.1
        _FillBounds ("Fill Bounds Min/Max", Vector) = (-15, -1.71, 15, 1.71)
        _OffsetX ("Offset X", Float) = 0
        _OffsetY ("Offset Y", Float) = 0
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
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
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float3 worldPos : TEXCOORD1;
            };
            
            sampler2D _MainTex;
            sampler2D _OldTex;
            sampler2D _NewTex;
            fixed4 _Color;
            float _FillAmount;
            float4 _FillDirection;
            float _FillSmoothness;
            float4 _FillBounds; // (minX, minY, maxX, maxY)
            float _OffsetX;
            float _OffsetY;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz; // Pass world-space position for fill calculation
                
                #ifdef PIXELSNAP_ON
                o.vertex = UnityPixelSnap(o.vertex);
                #endif
                
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Apply material offset for conveyor animation
                float2 uvOffset = i.uv + float2(_OffsetX, _OffsetY);
                
                // Sample all textures with offset
                fixed4 mainCol = tex2D(_MainTex, uvOffset) * i.color;
                fixed4 oldCol = tex2D(_OldTex, uvOffset) * i.color;
                fixed4 newCol = tex2D(_NewTex, uvOffset) * i.color;
                
                // If not transitioning, just show the main texture
                if (_FillAmount < 0.001)
                {
                    return mainCol;
                }
                
                // During transition, use _OldTex as the source texture
                fixed4 sourceCol = oldCol;
                
                // Calculate fill mask based on WORLD-SPACE position instead of UV
                // This prevents per-tile filling on repeated textures
                // Normalize world position to 0-1 range based on actual world bounds
                float normalizedX = (i.worldPos.x - _FillBounds.x) / (_FillBounds.z - _FillBounds.x);
                float normalizedY = (i.worldPos.y - _FillBounds.y) / (_FillBounds.w - _FillBounds.y);
                
                float fillMask = 0;
                
                if (abs(_FillDirection.x) > abs(_FillDirection.y))
                {
                    // Horizontal fill - use X position in object space
                    fillMask = _FillDirection.x > 0 ? normalizedX : (1.0 - normalizedX);
                }
                else
                {
                    // Vertical fill - use Y position in object space
                    fillMask = _FillDirection.y > 0 ? normalizedY : (1.0 - normalizedY);
                }
                
                // Smooth transition with adjustable smoothness
                // Inverted: areas where fillMask < _FillAmount show new sprite
                float blend = 1.0 - smoothstep(_FillAmount - _FillSmoothness, _FillAmount + _FillSmoothness, fillMask);
                
                // Blend between old and new sprite
                // blend = 0: old sprite (underlayer)
                // blend = 1: new sprite (fills in)
                fixed4 finalColor = lerp(sourceCol, newCol, blend);
                
                return finalColor;
            }
            ENDCG
        }
    }
    
    Fallback "Sprites/Default"
}

