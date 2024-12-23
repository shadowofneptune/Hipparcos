﻿    Shader "Particles/Infinite Depth Unlit"
    {
      Properties
      {
        _MainTex ("Texture", 2D) = "white" {}
      }
      SubShader
      {
        Tags { "RenderType"="Background" "Queue"="Background" }
        LOD 100
        Blend One One
        Zwrite Off
        Cull Back
        Pass
        {
          CGPROGRAM
          #pragma vertex vert
          #pragma fragment frag
          // make fog work
          #pragma multi_compile_fog
          
          #include "UnityCG.cginc"
          struct appdata
          {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
            float4 color : COLOR;
          };
          struct v2f
          {
            float2 uv : TEXCOORD0;
            UNITY_FOG_COORDS(1)
            float4 vertex : SV_POSITION;
            float4 color : COLOR;
          };
          sampler2D _MainTex;
          float4 _MainTex_ST;
          
          v2f vert (appdata v)
          {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = TRANSFORM_TEX(v.uv, _MainTex);
            UNITY_TRANSFER_FOG(o,o.vertex);
            o.color = v.color;
			#if defined(UNITY_REVERSED_Z)
				// when using reversed-Z, make the Z be just a tiny bit above 0.0
				o.vertex.z = 1.0e-9f;
			#else
			// when not using reversed-Z, make Z/W be just a tiny bit below 1.0
			o.vertex.z = o.vertex.w - 1.0e-6f;
			#endif
			
            return o;
          }
          
          fixed4 frag (v2f i) : SV_Target
          {
            // sample the texture
            fixed4 col = tex2D(_MainTex, i.uv) * i.color;
            // apply fog
            UNITY_APPLY_FOG(i.fogCoord, col);
            return col;
          }
          ENDCG
        }
      }
    }