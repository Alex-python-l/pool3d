using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Pool3D.Offline.Geometry;
using Pool3D.Offline.Presentation;

namespace Pool3D.Offline.Editor
{
    public static class TableReplacementChecks
    {
        [Serializable] public class Report {public bool passed,playMode,gameplay,rendered;public int checks,meshes,triangles,blockers;public string error;public List<string> evidence=new List<string>();public float[] clearApproaches;}
        private static Report report;
        private static void Check(bool ok,string label){if(!ok)throw new Exception(label);report.checks++;report.evidence.Add(label);}
        [MenuItem("Tools/Pool3D/Check replacement candidate (edit mode)")]
        public static void Run(){TableReplacementBuilder.Prepare();IntegratedPresentationChecks.Run();ValidateOnly();}
        public static void ValidateOnly()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new Exception("Edit mode only");
            report=new Report();
            try {
                var source=TableReplacementBuilder.Read();
                var assets=Resources.Load<IntegratedArtAssets>("Integrated/PresentationAssets");
                var geometry=TableGeometryConfig.Load(Path.Combine(Application.streamingAssetsPath,"TableGeometry.json"));
                Check(assets.geometryId==TableReplacementBuilder.Id&&geometry.GeometryId==assets.geometryId,"presentation and collision profile paired");
                var changed=geometry.Copy();var field=typeof(TableGeometryConfig).GetField("blockers",BindingFlags.NonPublic|BindingFlags.Instance);
                ((float[])field.GetValue(changed))[51]+=.2f;
                bool rejected=false;try{changed.Validate();}catch(InvalidDataException){rejected=true;}
                Check(rejected,"mutated jaw contour rejected by canonical payload hash");
                for(int i=0;i<source.meshes.Length;i++) {
                    var expected=source.meshes[i];var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(TableReplacementBuilder.Folder+"/Meshes/Mesh_"+i+".asset");
                    Check(mesh!=null&&mesh.vertexCount==expected.vertices.Length,"mesh vertex count "+i);
                    var vertices=mesh.vertices;var normals=mesh.normals;var uv=mesh.uv;
                    Check(vertices.Where((p,j)=>(p-expected.vertices[j]).sqrMagnitude>1e-12f).Count()==0,"metre axes and height preserved "+i);
                    Check(normals.Where((p,j)=>(p-expected.normals[j]).sqrMagnitude>1e-10f||float.IsNaN(p.x)).Count()==0,"source normal directions preserved "+i);
                    Check(uv.Where((p,j)=>(p-expected.uv[j]).sqrMagnitude>1e-12f).Count()==0,"original per-loop UV preserved "+i);
                    for(int j=0;j<expected.groups.Length;j++) {
                        var triangles=mesh.GetTriangles(j);
                        Check(triangles.SequenceEqual(expected.groups[j].indices),"triangle winding / material grouping "+i+"/"+j);
                        report.triangles+=triangles.Length/3;
                    }
                    report.meshes++;
                }
                var bodySource=source.meshes.Single(m=>m.role=="body");
                var atlasGroup=bodySource.groups.Single(g=>source.materials[g.material].atlas);
                var upperIds=Enumerable.Range(0,atlasGroup.indices.Length/3).Select(i=>atlasGroup.indices.Skip(i*3).Take(3).ToArray()).Where(t=>t.All(k=>bodySource.vertices[k].y>-.009999f && !PocketAppearanceRegion(bodySource.vertices[k]))).SelectMany(t=>t).Distinct().ToArray();
                Check(upperIds.Length>100,"original frame upper UV samples present");
                Check(upperIds.All(k=>bodySource.uv[k].x>=.44999f&&bodySource.uv[k].x<=.47501f&&bodySource.uv[k].y>=.64999f&&bodySource.uv[k].y<=.79001f),"upper wood UVs confined to the clean wood patch");
                Check(source.meshes.Where(m=>m.role=="cushion").All(m=>m.vertices.Length>=300),"all six cushion meshes include the resampled jaw curves");
                var gamePrefab=AssetDatabase.LoadAssetAtPath<GameObject>(TableReplacementBuilder.Folder+"/Prefabs/TableGameSpace.prefab");
                var instance=(GameObject)PrefabUtility.InstantiatePrefab(gamePrefab);
                try {
                    Check(instance.transform.localScale==Vector3.one*100,"one metre to 100 centimetres applied once");
                    Check((instance.transform.position-TableSpace.ArtOrigin).magnitude<.0001f,"cloth matches stationary ball support height");
                    Check(instance.GetComponentsInChildren<Collider>(true).Length==0&&instance.GetComponentsInChildren<Rigidbody>(true).Length==0,"display mesh introduces no second collision solver");
                    var renderers=instance.GetComponentsInChildren<Renderer>(true);
                    Check(renderers.Length==50,"all fifty source parts present");
                    Check(renderers.All(r=>r.sharedMaterials.All(m=>m!=null&&m.shader!=null&&m.shader.name!="Hidden/InternalErrorShader")),"all renderer materials bound");
                    Check(renderers.Where(r=>r.name.Contains("cushion")).Count()==6,"six independent render cushions");
                    var bound=renderers[0].bounds;foreach(var r in renderers)bound.Encapsulate(r.bounds);
                    Check(Mathf.Abs(bound.size.x-156)<.01f&&Mathf.Abs(bound.size.z-284)<.01f,"game-space width and length correct");
                    Check(Mathf.Abs(bound.min.y+82.8575f)<.01f,"feet on metre-calibrated floor");
                    foreach(var shader in renderers.SelectMany(r=>r.sharedMaterials).Select(m=>m.shader).Distinct())
                        Check(!ShaderUtil.GetShaderMessages(shader).Any(m=>m.severity.ToString()=="Error"),"no shader import errors: "+shader.name);
                }finally{UnityEngine.Object.DestroyImmediate(instance);}
                // Direct segment distance checks of candidate AI target lines: no AI host or simulation.
                var targets=geometry.BuildAiTargets();var blockers=geometry.CopyBlockers();report.blockers=geometry.BlockerCount;
                report.clearApproaches=new float[6];
                for(int pocket=0;pocket<6;pocket++) {
                    var center=geometry.Pocket(pocket);float capture=geometry.PocketRadius(pocket);float best=float.NegativeInfinity;
                    for(int target=0;target<3;target++) {
                        var direction=new Vector2(targets[pocket*6+target*2],targets[pocket*6+target*2+1]).normalized;
                        float projection=Vector2.Dot(center,direction);float perpendicular=Mathf.Abs(center.x*direction.y-center.y*direction.x);
                        Check(perpendicular<capture,"AI target line reaches capture circle "+pocket+"/"+target);
                        float entry=projection-Mathf.Sqrt(capture*capture-perpendicular*perpendicular),clearance=float.PositiveInfinity;
                        for(int step=0;step<320;step++) {
                            var p=direction*(entry-step);
                            for(int j=0;j<blockers.Length;j+=4) {
                                var a=new Vector2(blockers[j],blockers[j+1]);var b=new Vector2(blockers[j+2],blockers[j+3]);var delta=b-a;
                                float t=Mathf.Clamp01(Vector2.Dot(p-a,delta)/delta.sqrMagnitude);
                                clearance=Mathf.Min(clearance,Vector2.Distance(p,a+t*delta)-geometry.BallRadius);
                            }
                        }
                        best=Mathf.Max(best,clearance);
                    }
                    report.clearApproaches[pocket]=best;
                    Check(best>1,"at least one existing AI target has ball clearance from centre to pocket "+pocket);
                }
                EditorSceneManager.OpenScene("Assets/Scenes/IntegratedOffline.unity",OpenSceneMode.Single);
                var environment=GameObject.Find("Integrated Environment");
                Check(environment!=null,"saved candidate scene environment present");
                Check(!environment.GetComponentsInChildren<Transform>(true).Any(t=>t.name.StartsWith("PoolSet")||t.name=="Table apron"||t.name.StartsWith("Table support ")),"old table / legs removed from saved candidate");
                var floor=environment.GetComponentsInChildren<Renderer>(true).First(r=>r.name=="Room_Floor");
                Check(Mathf.Abs(floor.bounds.max.y+82.8575f)<.01f,"room floor aligned to new feet");
                Check(UnityEngine.Object.FindObjectsOfType<OfflineMatchController>().Length==0,"gameplay host never instantiated");
                Check(UnityEngine.Object.FindObjectsOfType<Transform>().Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject))==0,"saved candidate has no missing scripts");
                var dependencies=AssetDatabase.GetDependencies(new[]{TableReplacementBuilder.Folder+"/Prefabs/TableMetres.prefab","Assets/Resources/Integrated/PresentationAssets.asset","Assets/Scenes/IntegratedOffline.unity"},true);
                Check(dependencies.All(p=>!p.Contains(":")&&!p.Contains("Downloads")&&!p.EndsWith(".blend")),"runtime dependencies do not require Blender or absolute paths");
                report.passed=true;
            }catch(Exception e){report.error=e.ToString();}
            string docs=Path.GetFullPath(Path.Combine(Application.dataPath,"../../Docs"));
            File.WriteAllText(Path.Combine(docs,"replacement_checks.json"),JsonUtility.ToJson(report,true));
            if(!report.passed)throw new Exception(report.error);
            Debug.Log("[TableReplacement] CHECKS_OK "+report.checks+"; no gameplay / rendering");
        }
        public static void ExportPackage()
        {
            ValidateOnly();
            string output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../Pool3D-NewTable-Model.unitypackage"));
            AssetDatabase.ExportPackage(new[]{TableReplacementBuilder.Folder+"/Prefabs/TableMetres.prefab",TableReplacementBuilder.Folder+"/Prefabs/TableGameSpace.prefab"},output,ExportPackageOptions.IncludeDependencies);
            Debug.Log("[TableReplacement] MODEL_PACKAGE_OK");
        }
        private static bool PocketAppearanceRegion(Vector3 p)
        {
            return (Mathf.Abs(p.z)>1.16f&&Mathf.Abs(p.x)>.515f)||(Mathf.Abs(p.z)<.085f&&Mathf.Abs(Mathf.Abs(p.x)-.665f)<.130f);
        }
    }
}
