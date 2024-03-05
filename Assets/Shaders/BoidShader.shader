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
            
            #pragma target 4.5

            #include "UnityPBSLighting.cginc"
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
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

            float4 _Albedo1, _Albedo2;

            float4 RotateAroundYInDegrees (float4 vertex, float degrees) {
                float alpha = degrees;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                return float4(mul(m, vertex.xz), vertex.yw).xzyw;
            }
            
            float4 RotateAroundXInDegrees (float4 vertex, float degrees) {
                float alpha = degrees;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                return float4(mul(m, vertex.yz), vertex.xw).zxyw;
            }
            
            float4 RotateAroundZInDegrees (float4 vertex, float degrees) {
                float alpha = degrees;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                return float4(mul(m, vertex.xy), vertex.zw).yzxw;
            }

            float Angle(float3 from, float3 to){
                return acos(clamp(dot(normalize(from), normalize(to)), -1, 1)) * (UNITY_PI / 180.0);
            }

            float4 AngleAxis(float aAngle, float3 aAxis){
                aAxis = normalize(aAxis);
                float rad = aAngle * ((UNITY_PI * 2)/360.f) * 0.5f;
                aAxis *= sin(rad);
                return float4(aAxis, cos(rad));
            }

            float4 FromLookRotation(float3 forward, float3 up){
                forward = normalize(forward);

                float3 vec = normalize(forward);
                float3 vec2 = normalize(cross(up, vec));
                float3 vec3 = cross(vec, vec2);

                float m00 = vec2.x;
                float m01 = vec2.y;
                float m02 = vec2.z;
                float m10 = vec3.x;
                float m11 = vec3.y;
                float m12 = vec3.z;
                float m20 = vec.x;
                float m21 = vec.y;
                float m22 = vec.z;

                float num8 = (m00 + m11) + m22;
                float4 q;
                if(num8 > 0){
                    float num = sqrt(num8+1);
                    q.w = num * 0.5f;
                    num = 0.5f/ num;
                    q.x = (m12 - m21) * num;
                    q.y = (m20 - m02) * num;
                    q.z = (m01 - m10) * num;

                    return q;
                }

                if((m00 >= m11) && (m00 >= m22)){
                    float num7 = sqrt(((1+m00)-m11)-m22);
                    float num4 = 0.5f / num7;
                    q.x = 0.5f * num7;
                    q.y = (m01 + m10) * num4;
                    q.z = (m02 + m20) * num4;
                    q.w = (m12 - m21) * num4;
                    return q;
                }

                if(m11 > m22){
                    float num6 = sqrt(((1+m11)-m00)-m22);
                    float num3 = 0.5f / num6;
                    q.x = (m10 + m01) * num3;
                    q.y = 0.5f * num6;
                    q.z = (m21 + m12) * num3;
                    q.w = (m20 - m02) * num3;
                    return q;
                }

                float num5 = sqrt(((1+m22)-m00)-m11);
                float num2 = 0.5f/ num5;
                q.x = (m20 + m02) * num2;
                q.y = (m21 + m12) * num2;
                q.z = 0.5f * num5;
                q.w = (m01 - m10) * num2;
                return q;
            }

            float3 ToEuler(float4 q){
                double3 res;

                double sinr_cosp = 2 * (q.w * q.x + q.y * q.z);
                double cosr_cosp = 1 - 2 * (q.x * q.x + q.y * q.y);
                res.x = atan2(sinr_cosp, cosr_cosp);

                double sinp = 2 * (q.w * q.y - q.z * q.x);
                if(abs(sinp) >= 1){
                    res.y = UNITY_PI / 2.0 * sign(sinp);
                }else{
                    res.y = asin(sinp);
                }

                double siny_cosp = 2 * (q.w * q.z + q.x * q.y);
                double cosy_cosp = 1 - 2 * (q.y * q.y + q.z * q.z);
                res.z = atan2(siny_cosp, cosy_cosp);

                return (float3) res;
            }

            v2f vert (appdata v, uint instanceID : SV_InstanceID)
            {
                //InitIndirectDrawArgs(0);
                v2f o;

                Boid boid = boids[instanceID];

                float3 boidPosition = boid.position;

                float3 rotation = ToEuler(FromLookRotation(boid.dir, float3(0, 1, 0)));

                //float4 localPosition = float4(mul(float4x4(rotation.x, -rotation.y, -rotation.z, -rotation.w, rotation.y, rotation.x, -rotation.w, rotation.z, rotation.z, rotation.w, rotation.x, -rotation.y, rotation.w, -rotation.z, rotation.y, rotation.x), v.vertex));

                float4 localPosition = RotateAroundXInDegrees(v.vertex, -rotation.x);
                localPosition = RotateAroundYInDegrees(localPosition, rotation.y);
                localPosition = RotateAroundZInDegrees(localPosition, rotation.z);
                //localPosition = RotateAroundYInDegrees(localPosition, ((-boid.dir.y + 1)/2.f) * 360.f);
                //localPosition = RotateAroundZInDegrees(localPosition, ((-boid.dir.z + 1)/2.f) * 360.f);

                float4 worldPos = float4(boidPosition + localPosition, 1);

                o.vertex = UnityObjectToClipPos(worldPos);
                //UNITY_TRANSFER_FOG(o,o.vertex);
                o.worldPos = worldPos;
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 lightDir = _WorldSpaceLightPos0.xyz;
                float ndotl = DotClamped(lightDir, normalize(float3(0, 1, 0)));
                // sample the texture
                fixed4 col = _Albedo1;
                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
