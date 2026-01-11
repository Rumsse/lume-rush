#ifndef CELESTIAL_TRIPLANAR_INCLUDED
#define CELESTIAL_TRIPLANAR_INCLUDED

float4 triplanar(float3 vertPos, float3 normal, float scale, Texture2D tex, SamplerState samplerTex) {

	// Calculate triplanar coordinates
	float2 uvX = vertPos.zy * scale;
	float2 uvY = vertPos.xz * scale;
	float2 uvZ = vertPos.xy * scale;

    float4 colX = tex.Sample(samplerTex, uvX);
    float4 colY = tex.Sample(samplerTex, uvY);
    float4 colZ = tex.Sample(samplerTex, uvZ);
	// Square normal to make all values positive + increase blend sharpness
	float3 blendWeight = normal * normal;
	// Divide blend weight by the sum of its components. This will make x + y + z = 1
	blendWeight /= dot(blendWeight, 1);
	return colX * blendWeight.x + colY * blendWeight.y + colZ * blendWeight.z;
}

float4 triplanarOffset(float3 vertPos, float3 normal, float3 scale, float2 offset, Texture2D tex, SamplerState samplerTex) {
	float3 scaledPos = vertPos / scale;
    float4 colX = tex.Sample(samplerTex, scaledPos.zy + offset);
    float4 colY = tex.Sample(samplerTex, scaledPos.xz + offset);
    float4 colZ = tex.Sample(samplerTex, scaledPos.xy + offset);
	
	// Square normal to make all values positive + increase blend sharpness
	float3 blendWeight = normal * normal;
	// Divide blend weight by the sum of its components. This will make x + y + z = 1
	blendWeight /= dot(blendWeight, 1);
	return colX * blendWeight.x + colY * blendWeight.y + colZ * blendWeight.z;
}

float3 ObjectToTangentVector(float4 tangent, float3 normal, float3 objectSpaceVector) {
	float3 normalizedTangent = normalize(tangent.xyz);
	float3 binormal = cross(normal, normalizedTangent) * tangent.w;
	float3x3 rot = float3x3 (normalizedTangent, binormal, normal);
	return mul(rot, objectSpaceVector);
}

// Reoriented Normal Mapping
// http://blog.selfshadow.com/publications/blending-in-detail/
// Altered to take normals (-1 to 1 ranges) rather than unsigned normal maps (0 to 1 ranges)
float3 blend_rnm(float3 n1, float3 n2)
{
	n1.z += 1;
	n2.xy = -n2.xy;

	return n1 * dot(n1, n2) / n1.z - n2;
}

float3 Celestial_UnpackNormal(float4 n)
{
    float3 xyz = n.xyz * 2.0 - 1.0;
    return normalize(xyz);
}

// Sample normal map with triplanar coordinates
// Returned normal will be in obj/world space (depending whether pos/normal are given in obj or world space)
// Based on: medium.com/@bgolus/normal-mapping-for-a-triplanar-shader-10bf39dca05a
float3 triplanarNormal(float3 vertPos, float3 normal, float3 scale, float2 offset, Texture2D normalMap, SamplerState samplerNormal) {
	float3 absNormal = abs(normal);

	// Calculate triplanar blend
	float3 blendWeight = saturate(pow(normal, 4));
	// Divide blend weight by the sum of its components. This will make x + y + z = 1
	blendWeight /= dot(blendWeight, 1);

	// Calculate triplanar coordinates
	float2 uvX = vertPos.zy * scale.x + offset;
	float2 uvY = vertPos.xz * scale.y + offset;
	float2 uvZ = vertPos.xy * scale.z + offset;

	// Sample tangent space normal maps
	// UnpackNormal puts values in range [-1, 1] (and accounts for DXT5nm compression)
    float3 tangentNormalX = Celestial_UnpackNormal(normalMap.Sample(samplerNormal, uvX));
    float3 tangentNormalY = Celestial_UnpackNormal(normalMap.Sample(samplerNormal, uvY));
    float3 tangentNormalZ = Celestial_UnpackNormal(normalMap.Sample(samplerNormal, uvZ));

	// Swizzle normals to match tangent space and apply reoriented normal mapping blend
	tangentNormalX = blend_rnm(half3(normal.zy, absNormal.x), tangentNormalX);
	tangentNormalY = blend_rnm(half3(normal.xz, absNormal.y), tangentNormalY);
	tangentNormalZ = blend_rnm(half3(normal.xy, absNormal.z), tangentNormalZ);

	// Apply input normal sign to tangent space Z
	float3 axisSign = sign(normal);
	tangentNormalX.z *= axisSign.x;
	tangentNormalY.z *= axisSign.y;
	tangentNormalZ.z *= axisSign.z;

	// Swizzle tangent normals to match input normal and blend together
	float3 outputNormal = normalize(
		tangentNormalX.zyx * blendWeight.x +
		tangentNormalY.xzy * blendWeight.y +
		tangentNormalZ.xyz * blendWeight.z
	);

	return outputNormal;
}

float3 triplanarNormalTangentSpace(float3 vertPos, float3 normal, float3 scale, float4 tangent, Texture2D normalMap, SamplerState samplerNormal) {
    float3 textureNormal = triplanarNormal(vertPos, normal, scale, 0, normalMap, samplerNormal);
	return ObjectToTangentVector(tangent, normal, textureNormal);
}

float3 triplanarNormalTangentSpace(float3 vertPos, float3 normal, float3 scale, float2 offset, float4 tangent, Texture2D normalMap, SamplerState samplerNormal) {
    float3 textureNormal = triplanarNormal(vertPos, normal, scale, offset, normalMap, samplerNormal);
	return ObjectToTangentVector(tangent, normal, textureNormal);
}

#endif