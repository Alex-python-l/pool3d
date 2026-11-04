Shader "MaterialStudy/Short Felt Fibers" {
 Properties {
  _MainTex ("Green colour",2D)="white" {}
  [Normal] _BumpMap ("Micro fibre normal",2D)="bump" {}
  _FuzzMap ("Fibre density",2D)="white" {}
  _ColorMeters ("Colour tile metres",Float)=.8
  _FiberMeters ("Fibre tile metres",Float)=.012
  _BumpScale ("Micro normal strength",Range(0,1))=.35
  _Glossiness ("Matte smoothness",Range(0,.15))=.035
  _SheenStrength ("Short pile grazing response",Range(0,.5))=.22
  _FiberHeight ("Maximum pile metres",Float)=.00018
  _Layer ("Height fraction",Range(0,1))=1
  _FadeStart ("Near fade start metres",Float)=.45
  _FadeEnd ("Near fade end metres",Float)=1.1
  _Cutoff ("Fibre coverage cutoff",Range(0,1))=.45
  _WorldUnitsPerMetre ("World units per metre",Float)=100
  _BallCount ("Active sphere count",Float)=0
 }
 SubShader {
  Tags { "RenderType"="TransparentCutout" "Queue"="AlphaTest" }
  LOD 300
  Cull Back ZWrite On AlphaToMask On
  CGPROGRAM
  #pragma target 3.0
  #pragma surface surf Felt fullforwardshadows vertex:vert alphatest:_Cutoff keepalpha exclude_path:deferred
  #include "FeltLighting.cginc"
  float _FiberHeight,_Layer,_FadeStart,_FadeEnd,_WorldUnitsPerMetre;float4 _Balls[16];int _BallCount;
  struct Input {float3 worldPos;float3 worldNormal;INTERNAL_DATA};
  void vert(inout appdata_full v) {
   float3 worldNormal=UnityObjectToWorldNormal(v.normal);
   v.vertex.xyz+=mul((float3x3)unity_WorldToObject,worldNormal)*(_FiberHeight*_Layer*_WorldUnitsPerMetre);
  }
  void ExcludeBall(float3 p,float4 ball) {clip(distance(p,ball.xyz)-ball.w-.00004*_WorldUnitsPerMetre);}
  void surf(Input IN,inout SurfaceOutputStandard o) {
   // Geometry stops inside the sphere, including its tiny compressed contact patch.
   for(int i=0;i<16;i++){if(i>=_BallCount)break;ExcludeBall(IN.worldPos,_Balls[i]);}
   half nearFade=1-smoothstep(_FadeStart,_FadeEnd,distance(IN.worldPos,_WorldSpaceCameraPos)/max(_WorldUnitsPerMetre,.00001));
   half3 n=normalize(WorldNormalVector(IN,half3(0,0,1))),wn,colour;half density;
   FeltSurface(IN.worldPos/max(_WorldUnitsPerMetre,.00001),n,colour,wn,density);
   half3 t=normalize(WorldNormalVector(IN,half3(1,0,0))),b=normalize(WorldNormalVector(IN,half3(0,1,0)));
   o.Albedo=colour;o.Normal=normalize(half3(dot(wn,t),dot(wn,b),dot(wn,n)));
   o.Metallic=0;o.Smoothness=_Glossiness;o.Occlusion=1;o.Emission=0;
   o.Alpha=saturate((density-lerp(.12,.70,_Layer))*7)*nearFade;
  }
  ENDCG
 }
 Fallback Off
}
