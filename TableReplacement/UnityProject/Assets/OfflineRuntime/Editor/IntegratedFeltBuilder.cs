using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Pool3D.Offline.Presentation;
namespace Pool3D.Offline.Editor
{
    public static class IntegratedFeltBuilder
    {
        public static void Prepare(IntegratedArtAssets assets)
        {
            var root=(GameObject)PrefabUtility.InstantiatePrefab(assets.environmentPrefab);
            try {
                PrefabUtility.UnpackPrefabInstance(root,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
                foreach(var t in root.GetComponentsInChildren<Transform>(true))if(t!=null&&t.name.StartsWith("Felt fibers /"))UnityEngine.Object.DestroyImmediate(t.gameObject);
                var sources=root.GetComponentsInChildren<MeshRenderer>(true).Where(r=>r.sharedMaterials.Any(m=>m!=null&&m.shader.name=="MaterialStudy/Short Felt")).ToArray();
                if(sources.Length!=2)throw new Exception("Expected cloth bed and cushion felt surfaces");
                var shader=Shader.Find("MaterialStudy/Short Felt Fibers");if(shader==null)throw new Exception("Fiber shader missing");
                foreach(var source in sources) {
                    var basis=source.sharedMaterials.First(m=>m!=null&&m.shader.name=="MaterialStudy/Short Felt");basis.SetFloat("_WorldUnitsPerMetre",100);EditorUtility.SetDirty(basis);
                    var mesh=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/IntegratedArt/Meshes/Fibers_"+source.name+".asset");
                    if(mesh==null)throw new Exception("Pre-authored felt mesh missing for "+source.name);
                    for(int layer=1;layer<=4;layer++) {
                        string path="Assets/IntegratedArt/Materials/Integrated Felt layer "+layer+".mat";
                        var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
                        if(mat==null){mat=new Material(shader);AssetDatabase.CreateAsset(mat,path);}
                        mat.shader=shader;
                        foreach(string tex in new[]{"_MainTex","_BumpMap","_FuzzMap"})mat.SetTexture(tex,basis.GetTexture(tex));
                        foreach(string value in new[]{"_ColorMeters","_FiberMeters","_BumpScale","_Glossiness","_SheenStrength"})mat.SetFloat(value,basis.GetFloat(value));
                        mat.SetFloat("_WorldUnitsPerMetre",100);mat.SetFloat("_FiberHeight",.00018f);mat.SetFloat("_Layer",layer/4f);mat.SetFloat("_FadeStart",.45f);mat.SetFloat("_FadeEnd",1.1f);mat.SetFloat("_Cutoff",.45f);mat.SetInt("_BallCount",0);EditorUtility.SetDirty(mat);
                        var go=new GameObject("Felt fibers / "+source.name+" / layer "+layer);go.layer=8;go.transform.SetParent(source.transform,false);
                        go.AddComponent<MeshFilter>().sharedMesh=mesh;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=mat;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=true;
                        r.lightProbeUsage=LightProbeUsage.BlendProbes;r.receiveGI=ReceiveGI.LightProbes;r.reflectionProbeUsage=ReflectionProbeUsage.BlendProbes;GameObjectUtility.SetStaticEditorFlags(go,0);
                    }
                }
                assets.environmentPrefab=PrefabUtility.SaveAsPrefabAsset(root,"Assets/Resources/Integrated/Environment.prefab");EditorUtility.SetDirty(assets);AssetDatabase.SaveAssets();
            }finally{UnityEngine.Object.DestroyImmediate(root);}
        }
    }
}
