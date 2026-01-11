#ifndef CE_MATH_INCLUDED
#define CE_MATH_INCLUDED

#ifndef PI
#define PI 3.14159265358979323846
#endif
#define TAU       (PI * 2.0)
#define MAX_FLOAT 3.402823466e+38

inline float remap(float v, float minOld, float maxOld, float minNew, float maxNew)
{
    return saturate(minNew + (v - minOld) * (maxNew - minNew) / (maxOld - minOld));
}

inline float4 remap(float4 v, float minOld, float maxOld, float minNew, float maxNew)
{
    return saturate(minNew + (v - minOld) * (maxNew - minNew) / (maxOld - minOld));
}

inline float remap01(float v, float minOld, float maxOld)
{
    return saturate((v - minOld) / (maxOld - minOld));
}

inline float2 remap01(float2 v, float2 minOld, float2 maxOld)
{
    return saturate((v - minOld) / (maxOld - minOld));
}

inline float smoothMin(float a, float b, float k)
{
    k = max(0, k);
    float h = saturate((b - a + k) / (2 * k));
    return a * h + b * (1 - h) - k * h * (1 - h);
}

inline float smoothMax(float a, float b, float k)
{
    k = min(0, -k);
    float h = saturate((b - a + k) / (2 * k));
    return a * h + b * (1 - h) - k * h * (1 - h);
}

inline float Blend(float startHeight, float blendDst, float height)
{
    return smoothstep(startHeight - blendDst * 0.5, startHeight + blendDst * 0.5, height);
}

inline float2 raySphere(float3 centre, float radius, float3 rayOrigin, float3 rayDir)
{
    float3 offset = rayOrigin - centre;
    float b = 2 * dot(offset, rayDir);
    float c = dot(offset, offset) - radius * radius;

    float discriminant = b * b - 4 * c; // a=1 dla znormalizowanego rayDir
    if (discriminant > 0)
    {
        float s = sqrt(discriminant);
        float dstToSphereNear = max(0, (-b - s) * 0.5);
        float dstToSphereFar = (-b + s) * 0.5;
        if (dstToSphereFar >= 0)
            return float2(dstToSphereNear, dstToSphereFar - dstToSphereNear);
    }

    return float2(MAX_FLOAT, 0);
}

#endif