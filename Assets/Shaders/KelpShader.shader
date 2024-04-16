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
            
            #pragma target 4.5

            #include "UnityCG.cginc"

            #include "UnityPBSLighting.cginc"
            #include "AutoLight.cginc"

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

            struct KelpData{
                float3 positon;
                float scale;
            };

            float4 _KelpColor;

            StructuredBuffer<KelpData> kelpData;

            v2f vert (appdata v, uint instanceID : SV_InstanceID)
            {
                v2f o;
                float4 position = v.vertex;
                position.y += 0.5;

                position.z += sin((v.vertex.z + instanceID + _Time.y * .5)*2) * position.y;
                position.x += sin((v.vertex.x + instanceID + _Time.y * 1)*0.2) * position.y;
                position.y *= kelpData[instanceID].scale;

                float4 worldPosition = float4(kelpData[instanceID].positon + position, 1);

                o.vertex = UnityObjectToClipPos(worldPosition);
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 lightDir = _WorldSpaceLightPos0.xyz;
                float ndotl = DotClamped(lightDir, normalize(float3(0, 1, 0)));
                // sample the texture
                fixed4 col = float4(_KelpColor.xyz, 1);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col * ndotl;
            }
            ENDCG
        }
    }
}
