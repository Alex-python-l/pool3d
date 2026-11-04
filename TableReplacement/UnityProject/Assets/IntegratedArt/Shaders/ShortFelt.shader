Shader "MaterialStudy/Short Felt" {
 Properties {
  _MainTex ("Green colour (world tile)",2D)="white" {}
  [Normal] _BumpMap ("Independent micro fibre normal",2D)="bump" {}
  _FuzzMap ("Fibre density / variation",2D)="white" {}
  _ColorMeters ("Colour tile metres",Float)=.8
  _FiberMeters ("Fibre tile metres",Float)=.012
  _BumpScale ("Micro normal strength",Range(0,1))=.35
  _Glossiness ("Matte smoothness",Range(0,.15))=.035
  _Metallic ("Nonmetal (must stay zero)",Range(0,0))=0
  _SheenStrength ("Short pile grazing response",Range(0,.5))=.22
  _WorldUnitsPerMetre ("World units per metre",Float)=100
 }
 SubShader {
  Tags { "RenderType"="Opaque" "Queue"="Geometry" }
  LOD 300
  CGPROGRAM
  #pragma target 3.0
  #pragma surface surf Felt fullforwardshadows addshadow exclude_path:deferred
  #include "FeltLighting.cginc"
  float _WorldUnitsPerMetre;
  struct Input {float3 worldPos;float3 worldNormal;INTERNAL_DATA};
  void surf(Input IN,inout SurfaceOutputStandard o) {
   half3 n=normalize(WorldNormalVector(IN,half3(0,0,1))),wn,colour;half density;
   FeltSurface(IN.worldPos/max(_WorldUnitsPerMetre,.00001),n,colour,wn,density);
   half3 t=normalize(WorldNormalVector(IN,half3(1,0,0))),b=normalize(WorldNormalVector(IN,half3(0,1,0)));
   o.Albedo=colour;o.Normal=normalize(half3(dot(wn,t),dot(wn,b),dot(wn,n)));
   o.Metallic=0;o.Smoothness=_Glossiness;o.Occlusion=1;o.Emission=0;o.Alpha=1;
  }
  ENDCG
 }
 Fallback "Diffuse"
}
