Shader "Unlit/BoidShader"
{
    Properties
    {
        _Albedo1 ("Albedo 1", Color) = (1, 1, 1)
        _Albedo2 ("Albedo 2", Color) = (1, 1, 1)
        _AOColor ("Ambient Occlusion", Color) = (1, 1, 1)
        _TipColor ("Tip Color", Color) = (1, 1, 1)
        _Scale ("Scale", Range(0.0, 2.0)) = 0.0
        _Droop ("Droop", Range(0.0, 10.0)) = 0.0
        _FogColor ("Fog Color", Color) = (1, 1, 1)
        _FogDensity ("Fog Density", Range(0.0, 1.0)) = 0.0
        _FogOffset ("Fog Offset", Range(0.0, 10.0)) = 0.0
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
            #pragma multi_compile_instancing

            #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
            #include "UnityIndirect.cginc"
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 worldPos : TEXCOORD1;
            };

            struct Boid
            {
                float3 position;
                float3 velocity;
                float3 acceleration;
                float3 dir;
                uint listIndex;
                uint boidSettingIndex;
            };

            struct BoidSettings {
                float minSpeed;
                float maxSpeed;
                float maxSteerForce;
                float viewRadius;
                float avoidanceRadius;
                float alignWeight;
                float cohesionWeight;
                float seperateWeight;
                float avoidCollisionWeight;
            };

            StructuredBuffer<Boid> boids;
            uint numBoids;

            StructuredBuffer<BoidSettings> boidSettings;

            float4 _Albedo1;

            v2f vert (appdata v, uint instanceID : SV_InstanceID)
            {
                InitIndirectDrawArgs(0);
                v2f o;

                float3 boidPosition = boids[instanceID].position;

                float4 worldPos = float4(boidPosition, 1);

                o.vertex = UnityObjectToClipPos(worldPos);
                UNITY_TRANSFER_FOG(o,o.vertex);
                o.worldPos = worldPos;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = _Albedo1;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
