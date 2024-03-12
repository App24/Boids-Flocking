Shader "Unlit/BoidShader"
{
    Properties
    {
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
                float3 color : TEXCOORD2;
            };

            struct Boid
            {
                float3 position;
                float3 velocity;
                float3 acceleration;
                float3 dir;
                float3 color;
                uint listIndex;
                uint boidSettingIndex;
                uint boidGroup;
                uint ignoreOtherBoids;
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

            StructuredBuffer<BoidSettings> boidSettings;

            float4 RotateAroundYInDegrees (float4 vertex, float degrees) {
                float alpha = degrees * UNITY_PI / 180.0;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                return float4(mul(m, vertex.xz), vertex.yw).xzyw;
            }
            
            float4 RotateAroundXInDegrees (float4 vertex, float degrees) {
                float alpha = degrees * UNITY_PI / 180.0;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                return float4(mul(m, vertex.yz), vertex.xw).zxyw;
            }
            
            float4 RotateAroundZInDegrees (float4 vertex, float degrees) {
                float alpha = degrees * UNITY_PI / 180.0;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                return float4(mul(m, vertex.xy), vertex.zw).yzxw;
            }

            float4 FromLookRotation(float3 forward, float3 up){
                forward = normalize(forward);

                float3 right = normalize(cross(up, forward));
                up = cross(forward, right);

                float m00 = right.x;
                float m01 = right.y;
                float m02 = right.z;
                float m10 = up.x;
                float m11 = up.y;
                float m12 = up.z;
                float m20 = forward.x;
                float m21 = forward.y;
                float m22 = forward.z;

                float num8 = (m00 + m11) + m22;
                float4 q;
                if(num8 > 0){
                    float num = sqrt(num8 + 1.f);
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

            float NormalizeAngle(float angle){
                float modAngle = angle % 360.f;

                if(modAngle < 0.f)
                    return modAngle + 360.f;
                return modAngle;
            }

            float3 NormalizeAngles(float3 angles){
                angles.x = NormalizeAngle(angles.x);
                angles.y = NormalizeAngle(angles.y);
                angles.z = NormalizeAngle(angles.z);
                return angles;
            }

            float3 ToEuler(float4 q){
                float sqw = q.w * q.w;
                float sqx = q.x * q.x;
                float sqy = q.y * q.y;
                float sqz = q.z * q.z;

                float unit = sqx + sqy + sqz + sqw;

                float test = q.x * q.w - q.y * q.z;
                float3 v;

                if(test > 0.4995f * unit){
                    v.y = 2.f * atan2(q.y, q.x);
                    v.x = UNITY_PI / 2.f;
                    v.z = 0;
                    return NormalizeAngles(v * (360 / (UNITY_PI * 2)));
                }

                if(test < -0.4995f * unit){
                    v.y = -2.f * atan2(q.y, q.x);
                    v.x = -UNITY_PI / 2.f;
                    v.z = 0;
                    return NormalizeAngles(v * (360 / (UNITY_PI * 2)));
                }

                float4 rot = float4(q.w, q.z, q.x, q.y);
                v.y = atan2(2.f * rot.x * rot.w + 2.f * rot.y * rot.z, 1 - 2.f * (rot.z * rot.z + rot.w * rot.w));
                v.x = asin(2.f * (rot.x * rot.z - rot.w * rot.y));
                v.z = atan2(2.f * rot.x * rot.y + 2.f * rot.z * rot.w, 1 - 2.f * (rot.y * rot.y + rot.z * rot.z));

                    return NormalizeAngles(v * (360 / (UNITY_PI * 2)));
            }

            v2f vert (appdata v, uint instanceID : SV_InstanceID)
            {
                //InitIndirectDrawArgs(0);
                v2f o;

                Boid boid = boids[instanceID];

                float3 boidPosition = boid.position;

                float3 rotation = ToEuler(FromLookRotation(boid.dir, float3(0, 1, 0)));

                //float4 localPosition = float4(mul(float4x4(rotation.x, -rotation.y, -rotation.z, -rotation.w, rotation.y, rotation.x, -rotation.w, rotation.z, rotation.z, rotation.w, rotation.x, -rotation.y, rotation.w, -rotation.z, rotation.y, rotation.x), v.vertex));

                float4 localPosition = RotateAroundXInDegrees(v.vertex, rotation.x);
                localPosition = RotateAroundYInDegrees(localPosition, 360.f - rotation.y);
                //localPosition = RotateAroundZInDegrees(localPosition, 360.f - rotation.z);
                //localPosition = RotateAroundYInDegrees(localPosition, ((-boid.dir.y + 1)/2.f) * 360.f);
                //localPosition = RotateAroundZInDegrees(localPosition, ((-boid.dir.z + 1)/2.f) * 360.f);

                float4 worldPos = float4(boidPosition + localPosition, 1);

                o.vertex = UnityObjectToClipPos(worldPos);
                //UNITY_TRANSFER_FOG(o,o.vertex);
                o.worldPos = worldPos;
                o.uv = v.uv;
                o.color = boid.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 lightDir = _WorldSpaceLightPos0.xyz;
                float ndotl = DotClamped(lightDir, normalize(float3(0, 1, 0)));
                // sample the texture
                fixed4 col = float4(i.color, 1);
                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
