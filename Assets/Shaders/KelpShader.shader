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
                float3 color : TEXCOORD1;
            };

            struct KelpData{
                float2 positon;
                float scale;
                float3 color;
                float3 tipColor;
            };

            float4 _KelpColor;

            StructuredBuffer<KelpData> storedData;
            
            float _SpeedX;
	        float _FrequencyX;
	        float _AmplitudeX;
            
            float _SpeedZ;
	        float _FrequencyZ;
	        float _AmplitudeZ;

            v2f vert (appdata v, uint instanceID : SV_InstanceID)
            {
                v2f o;
                float4 position = v.vertex;
                position.y += 0.5;

                KelpData kelp = storedData[instanceID];

                float3 color = lerp(kelp.color, kelp.tipColor, clamp(position.y, 0, 1));

                position.z += sin((v.vertex.z + instanceID + _Time.y * _SpeedX)*_FrequencyX) * position.y * _AmplitudeX;
                position.x += sin((v.vertex.x + instanceID + _Time.y * _SpeedZ)*_FrequencyZ) * position.y * _AmplitudeZ;
                position.y *= kelp.scale;

                float4 worldPosition = float4(float3(kelp.positon.x, 0, kelp.positon.y) + position, 1);

                o.vertex = UnityObjectToClipPos(worldPosition);
                o.uv = v.uv;
                o.color = color;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 lightDir = _WorldSpaceLightPos0.xyz;
                float ndotl = DotClamped(lightDir, normalize(float3(0, 1, 0)));
                // sample the texture
                fixed4 col = float4(i.color, 1);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col * ndotl;
            }
            ENDCG
        }
    }
}
