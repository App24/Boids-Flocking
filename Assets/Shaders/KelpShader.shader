Shader "Unlit/KelpShader"
{
    Properties
    {
        _KelpColor("Kelp Color", Color) = (1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            float4 _KelpColor;

            v2f vert (appdata v)
            {
                v2f o;
                float4 position = v.vertex;
                float basePosition = v.vertex.y + 0.5;
                position.x += sin(_Time.y + v.vertex.y) * basePosition;
                position.z += sin(_Time.z + v.vertex.y) * basePosition;
                o.vertex = UnityObjectToClipPos(position);
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = float4(_KelpColor.xyz, 1);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
