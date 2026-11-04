#ifndef MATERIAL_STUDY_FELT_INCLUDED
#define MATERIAL_STUDY_FELT_INCLUDED
#include "UnityPBSLighting.cginc"
#include "UnityStandardUtils.cginc"
sampler2D _MainTex, _BumpMap, _FuzzMap;
half _BumpScale, _Glossiness, _SheenStrength;
float _ColorMeters, _FiberMeters;

// World triplanar coordinates keep millimetre fibres independent of the tabletop UV atlas.
float3 FeltWeights(float3 n) {
    float3 w=pow(abs(n),6);return w/max(w.x+w.y+w.z,0.00001);
}
half4 FeltSample(sampler2D map,float3 p,float3 weights,float metres) {
    return tex2D(map,p.zy/metres)*weights.x+tex2D(map,p.xz/metres)*weights.y+tex2D(map,p.xy/metres)*weights.z;
}
void FeltSurface(float3 p,float3 n,out half3 albedo,out half3 worldNormal,out half density) {
    float3 w=FeltWeights(n),sgn=step(0,n)*2-1;
    albedo=FeltSample(_MainTex,p,w,_ColorMeters).rgb;
    half3 nx=UnpackScaleNormal(tex2D(_BumpMap,p.zy/_FiberMeters),_BumpScale);
    half3 ny=UnpackScaleNormal(tex2D(_BumpMap,p.xz/_FiberMeters),_BumpScale);
    half3 nz=UnpackScaleNormal(tex2D(_BumpMap,p.xy/_FiberMeters),_BumpScale);
    worldNormal=normalize(half3(sgn.x*nx.z,nx.y,nx.x)*w.x+half3(ny.x,sgn.y*ny.z,ny.y)*w.y+half3(nz.x,nz.y,sgn.z*nz.z)*w.z);
    density=FeltSample(_FuzzMap,p,w,_FiberMeters).r;
    albedo*=lerp(.985,1.015,FeltSample(_FuzzMap,p,w,_FiberMeters).b);
}
// A bounded empirical short-pile lobe. It responds to direct and baked light, never emission.
half4 LightingFelt(SurfaceOutputStandard s,half3 viewDir,UnityGI gi) {
    half edge=1-saturate(dot(normalize(s.Normal),normalize(viewDir)));
    half sheen=saturate(_SheenStrength*edge*edge*edge*edge);
    half4 result=LightingStandard(s,viewDir,gi);
    half3 illumination=gi.light.color*saturate(dot(s.Normal,gi.light.dir))+gi.indirect.diffuse;
    result.rgb=lerp(result.rgb,s.Albedo*illumination,sheen);
    return result;
}
void LightingFelt_GI(SurfaceOutputStandard s,UnityGIInput data,inout UnityGI gi) {
    LightingStandard_GI(s,data,gi);
}
#endif
