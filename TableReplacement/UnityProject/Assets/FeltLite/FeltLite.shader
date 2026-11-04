Shader "Pool3D/Felt Lite" {
 Properties {
  _Color ("Felt dye", Color) = (.170,.532,.272,1)
  _Packed ("Linear: colour variation / roughness / height", 2D) = "white" {}
  [Normal] _BumpMap ("Micro fibre normal", 2D) = "bump" {}
  _FiberMeters ("Fibre repeat in metres", Float) = .018
  _WorldUnitsPerMetre ("World units per metre", Float) = 100
  _BumpScale ("Micro normal strength", Range(0,1)) = .65
  _SpecularF0 ("Nonmetal specular reflectance", Range(0,.08)) = .0224
  _SheenStrength ("Soft grazing response", Range(0,.3)) = .12
 }
 SubShader {
  Tags {"RenderType"="Opaque" "Queue"="Geometry"}
  LOD 200
  CGPROGRAM
  #pragma target 3.0
  #pragma surface surf FeltLite fullforwardshadows addshadow exclude_path:deferred
  #include "UnityPBSLighting.cginc"
  #include "UnityStandardUtils.cginc"
  sampler2D _Packed, _BumpMap;
  half4 _Color;
  half _BumpScale, _SpecularF0, _SheenStrength;
  float _FiberMeters, _WorldUnitsPerMetre;
  struct Input { float3 worldPos; float3 worldNormal; INTERNAL_DATA };

  // Reuses the project's world triplanar approach, with two maps instead of three.
  void surf(Input IN, inout SurfaceOutputStandardSpecular o) {
   float3 n=normalize(WorldNormalVector(IN,half3(0,0,1)));
   float3 p=IN.worldPos/max(_WorldUnitsPerMetre*_FiberMeters,.00001);
   float3 w=pow(abs(n),6);w/=max(w.x+w.y+w.z,.00001);
   float3 signN=step(0,n)*2-1;
   half3 packed=tex2D(_Packed,p.zy).rgb*w.x+tex2D(_Packed,p.xz).rgb*w.y+tex2D(_Packed,p.xy).rgb*w.z;
   half3 nx=UnpackScaleNormal(tex2D(_BumpMap,p.zy),_BumpScale);
   half3 ny=UnpackScaleNormal(tex2D(_BumpMap,p.xz),_BumpScale);
   half3 nz=UnpackScaleNormal(tex2D(_BumpMap,p.xy),_BumpScale);
   half3 wn=normalize(half3(signN.x*nx.z,nx.y,nx.x)*w.x+half3(ny.x,signN.y*ny.z,ny.y)*w.y+half3(nz.x,nz.y,signN.z*nz.z)*w.z);
   half3 t=normalize(WorldNormalVector(IN,half3(1,0,0))),b=normalize(WorldNormalVector(IN,half3(0,1,0)));
   o.Normal=normalize(half3(dot(wn,t),dot(wn,b),dot(wn,n)));
   o.Albedo=_Color.rgb*(.94+.12*packed.r);
   o.Specular=_SpecularF0.xxx;o.Smoothness=1-packed.g;
   o.Occlusion=1;o.Emission=0;o.Alpha=1;
  }
  // Bounded, lit grazing response; no emission or extra passes for fur.
  half4 LightingFeltLite(SurfaceOutputStandardSpecular s,half3 viewDir,UnityGI gi) {
   half4 c=LightingStandardSpecular(s,viewDir,gi);
   half edge=1-saturate(dot(normalize(s.Normal),normalize(viewDir)));
   half3 irradiance=gi.indirect.diffuse+gi.light.color*saturate(dot(s.Normal,gi.light.dir));
   c.rgb+=s.Albedo*irradiance*(_SheenStrength*edge*edge*edge*edge*.35);
   return c;
  }
  void LightingFeltLite_GI(SurfaceOutputStandardSpecular s,UnityGIInput data,inout UnityGI gi) {
   LightingStandardSpecular_GI(s,data,gi);
  }
  ENDCG
 }
 Fallback "Standard (Specular setup)"
}
