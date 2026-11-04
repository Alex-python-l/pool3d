// Edit-mode data/transform/projection checks only. Never constructs a match host or renders.
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Pool3D.Offline.Geometry;
using Pool3D.Offline.Presentation;

namespace Pool3D.Offline.Editor
{
    public static class IntegratedPresentationChecks
    {
        [Serializable] private class Report {public bool passed,gameplay=false,playMode=false,screenshots=false,rendered=false;public int cases,ballResources;public string error;public List<string> checks=new List<string>();}
        private static Report report;
        private static void Check(bool ok,string message){if(!ok)throw new Exception(message);report.cases++;report.checks.Add(message);}
        private static void Near(float a,float b,string message){Check(!float.IsNaN(a)&&Mathf.Abs(a-b)<.03f,message);}
        public static void Run()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new Exception("Edit mode only");
            report=new Report();var originalRandom=UnityEngine.Random.state;
            string folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../../Docs"));
            try {
                IntegratedGeometryChecks.Run();
                var art=IntegratedArtBuilder.Prepare();
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                for(int i=0;i<16;i++) {
                    var ball=art.ballPrefabs[i];Check(ball!=null,"ball prefab "+i);
                    var renderer=ball.GetComponent<Renderer>();var material=renderer.sharedMaterial;var texture=(Texture2D)material.mainTexture;
                    Check(texture.width==2048&&texture.height==2048,"2048 ball texture "+i);
                    Near(material.GetFloat("_Metallic"),0,"nonmetallic resin "+i);
                    Check(ball.GetComponent<Rigidbody>()==null&&ball.GetComponent<Collider>()==null,"ball has no second physics solver "+i);
                    report.ballResources++;
                }
                var container=new GameObject("Independent presentation checks");
                try {
                    using(var cue=new IntegratedCueView(art.cuePrefab,container.transform)) {
                        Near(Vector3.Distance(cue.Tip,cue.Butt),145,"cue length in world centimetres");
                        foreach(float angle in new[]{0,45,90,135,180,225,270,315})
                        foreach(Vector2 contact in new[]{Vector2.zero,new Vector2(.7f,0),new Vector2(-.7f,0),new Vector2(0,.7f),new Vector2(0,-.7f),new Vector2(.4f,.4f)}) {
                            cue.Apply(Vector3.zero,angle,contact,.5f,true);
                            float a=angle*Mathf.Deg2Rad;var dir=new Vector3(Mathf.Sin(a),0,Mathf.Cos(a));var left=new Vector3(-Mathf.Cos(a),0,Mathf.Sin(a));
                            Check(Vector3.Dot(cue.Root.forward,dir)>.99999f,"contact preserves aim "+angle+"/"+contact);
                            Near(Vector3.Dot(cue.Tip,left),2.8575f*contact.x,"left tip contact "+angle+"/"+contact);
                            Near(cue.Tip.y,2.8575f*contact.y,"high tip contact "+angle+"/"+contact);
                            Near(Vector3.Dot(cue.Tip,dir),-(2.8575f*Mathf.Sqrt(1-contact.sqrMagnitude)+10.3f),"spherical tip gap and pull "+angle+"/"+contact);
                        }
                        cue.Apply(Vector3.zero,0,Vector2.zero,0,false);Check(!cue.Root.gameObject.activeSelf,"cue hidden outside aiming");
                    }
                    var cameraObject=new GameObject("Projection fixture");var camera=cameraObject.AddComponent<Camera>();
                    camera.nearClipPlane=1;camera.pixelRect=new Rect(0,0,900,600);camera.fieldOfView=60;
                    Vector2 backward;
                    // nographics Editor uses its own 640x480 viewport: derive its centre, not a requested window size.
                    Vector2 viewportCentre=camera.pixelRect.center;
                    Check(IntegratedCueView.Pick(camera,viewportCentre,new Vector3(0,0,10),new Vector3(10,0,-10),out backward),"visible shaft remains pickable when butt behind near plane");
                    Check(backward.x>0,"pull direction follows projected shaft");
                    Check(!IntegratedCueView.Pick(camera,viewportCentre,new Vector3(0,0,-10),new Vector3(10,0,-20),out backward),"fully behind-camera cue rejected");
                    UnityEngine.Object.DestroyImmediate(cameraObject);
                    int baselineMaterials=Resources.FindObjectsOfTypeAll<Material>().Count(m=>!AssetDatabase.Contains(m));
                    for(int pass=0;pass<2;pass++) {
                        using(var view=new OfflineSceneView(container.transform)) {
                            view.PrepareResources();
                            foreach(int count in new[]{16,10}) {
                                var rule=new EightBallVsRule();rule.ConfigureGeometry(TableGeometryConfig.Load(Path.Combine(Application.streamingAssetsPath,"TableGeometry.json")));
                                var table=(PoolTable)FormatterServices.GetUninitializedObject(typeof(PoolTable));
                                typeof(PoolTable).GetField("rule",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(table,rule);
                                table.balls=new PoolBall[count];
                                for(int i=0;i<count;i++)table.balls[i]=new PoolBall{id=i,x=100+i*20,y=50,z=0,scale=28.575f,visible=true,isAvailable=true,quat=new Quaternion(0,0,1,0)};
                                view.Build(table);table.balls[8].visible=false;table.balls[9].isAvailable=false;
                                view.UpdateBalls(table,45,true,.2f,new Vector2(.4f,.4f));
                                var transforms=container.GetComponentsInChildren<Transform>(true);
                                Transform first=transforms.First(t=>t.name=="Ball 0");
                                Check((first.position-new Vector3(5,0,10)).magnitude<.001f,"state follows simulator axes "+pass+"/"+count);
                                Near(Mathf.Abs(Quaternion.Dot(first.rotation,new Quaternion(0,1,0,0))),1,"original quaternion permutation "+pass+"/"+count);
                                Near(first.GetComponent<Renderer>().bounds.size.x,5.715f,"rendered ball diameter "+pass+"/"+count);
                                Check(!transforms.First(t=>t.name=="Ball 8").gameObject.activeSelf&&!transforms.First(t=>t.name=="Ball 9").gameObject.activeSelf,"hidden/unavailable states "+pass+"/"+count);
                                Near(Mathf.DeltaAngle(view.ReadCueAngle(),45),0,"side contact does not change host aim "+pass+"/"+count);
                                view.Camera.pixelRect=new Rect(336,0,944,720);view.SetOverview(true,45);
                                foreach(Vector3 corner in new[]{new Vector3(-63.5f,0,-127),new Vector3(63.5f,0,127)}) {
                                    Vector3 p=view.Camera.WorldToViewportPoint(corner);Check(p.x>=0&&p.x<=1&&p.y>=0&&p.y<=1,"overview fits new table "+pass+"/"+count);
                                }
                                view.Clear();Check(container.GetComponentsInChildren<Transform>(true).All(t=>!t.name.StartsWith("Ball ")),"clear removes all ball states "+pass+"/"+count);
                            }
                        }
                        Check(container.GetComponentsInChildren<Camera>(true).Length==0&&container.GetComponentsInChildren<AudioListener>(true).Length==0,"camera/listener cleanup "+pass);
                    }
                    Check(Resources.FindObjectsOfTypeAll<Material>().Count(m=>!AssetDatabase.Contains(m))==baselineMaterials,"no runtime material instances leaked");
                }finally{UnityEngine.Object.DestroyImmediate(container);}
                IntegratedArtBuilder.SaveScene(art);
                Check(UnityEngine.Object.FindObjectsOfType<OfflineMatchController>().Length==0,"game host never instantiated during checks");
                Check(UnityEngine.Object.FindObjectsOfType<OfflineBootstrap>().Length==1,"one saved bootstrap reference");
                Check(UnityEngine.Object.FindObjectsOfType<Transform>().All(t=>!t.name.StartsWith("Felt fibers /")),"static four-ball fibers excluded until M2");
                Check(UnityEngine.Object.FindObjectsOfType<Transform>().Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject))==0,"saved scene has no missing scripts");
                Check(PlayerSettings.colorSpace==ColorSpace.Linear,"integrated project uses Linear");
                report.passed=true;
            }catch(Exception e){report.error=e.ToString();}
            finally{UnityEngine.Random.state=originalRandom;}
            File.WriteAllText(Path.Combine(folder,"presentation_checks.json"),JsonUtility.ToJson(report,true));
            if(!report.passed)throw new Exception(report.error);
            Debug.Log("[Integration] PRESENTATION_OK "+report.cases+"; assets/scene saved; no gameplay/rendering");
        }
    }
}
