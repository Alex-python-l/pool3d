// Pure geometry/state fixtures. No game host, AI instance, simulation tick or Play Mode.
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;
using Pool3D.Offline.Geometry;

namespace Pool3D.Offline.Editor
{
    public static class IntegratedGeometryChecks
    {
        [Serializable] private class Report {public bool passed,gameplay=false,playMode=false;public int cases;public List<string> checks=new List<string>();public string error;}
        private static Report report;
        private static void Check(bool ok,string message){if(!ok)throw new Exception(message);report.checks.Add(message);report.cases++;}
        private static void Near(float a,float b,string message){Check(!float.IsNaN(a)&&Mathf.Abs(a-b)<=.001f,message);}
        private static void Reject(Action code,string message){bool rejected=false;try{code();}catch(InvalidDataException){rejected=true;}Check(rejected,message);}
        public static void Run()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new Exception("Edit mode only");
            report=new Report();
            string folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../../Docs"));
            try {
                string json=File.ReadAllText(Path.Combine(Application.streamingAssetsPath,"TableGeometry.json"));
                var g=TableGeometryConfig.FromJson(json);
                Near(g.LongSide,2540,"V3 long side");Near(g.ShortSide,1270,"V3 short side");Near(g.BallRadius*2,57.15f,"ball diameter unchanged");
                Check(g.BlockerCount>30,"mesh outline exceeds old cache capacity");
                Reject(()=>TableGeometryConfig.FromJson(json.Replace("\"longSide\": 2540","\"longSide\": 1981.2")),"mixed old/V3 dimensions rejected");
                Reject(()=>TableGeometryConfig.FromJson(json.Replace("\"metresPerUnit\": 0.001","\"metresPerUnit\": 0")),"zero unit scale rejected");
                Reject(()=>TableGeometryConfig.FromJson(json.Replace("\"schemaVersion\": 1","\"schemaVersion\": 2")),"unknown schema rejected");
                Reject(()=>TableGeometryConfig.FromJson("{}"),"empty config rejected");
                var acceptedMixedCases=new List<string>();
                try{TableGeometryConfig.FromJson(json.Replace("luxury-chinese8-20261030-r3","legacy"));acceptedMixedCases.Add("legacy ID with replacement body");}catch(InvalidDataException){}
                var staleRails=TableGeometryConfig.FromJson(json);
                typeof(TableGeometryConfig).GetField("blockers",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(staleRails,new EightBallVsRule().Geometry.CopyBlockers(true));
                try{staleRails.Validate();acceptedMixedCases.Add("old cushion array with V3 dimensions");}catch(InvalidDataException){}
                var staleHead=TableGeometryConfig.FromJson(json);
                typeof(TableGeometryConfig).GetField("freeCorners",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(staleHead,new[]{-1270f,-495.3f,635f,-635f});
                try{staleHead.Validate();acceptedMixedCases.Add("old head-string area with new head string");}catch(InvalidDataException){}
                Check(acceptedMixedCases.Count==0,"reject mixed geometry contracts: "+string.Join(", ",acceptedMixedCases));
                for(int i=0;i<100;i++) {
                    Vector3 p=new Vector3(i*23-1200,i*11-600,-i*.3f),q=TableSpace.WorldToSim(TableSpace.SimToWorld(p));
                    Check((q-p).magnitude<=.001f,"coordinate roundtrip "+i);
                }
                Vector3 mapped=TableSpace.SimToWorld(new Vector3(1000,500,10));
                Check((mapped-new Vector3(50,1,100)).sqrMagnitude<.00001f,"axis mapping is y/z/x");
                Near((TableSpace.ArtOrigin+new Vector3(0,.028575f,0)*100).y,0,"art cloth and ball contact height");
                var cue=new Vector2(100,200);
                Near(g.ReflectCue(cue,0).x,-2582.85f,"AI left reflection");Near(g.ReflectCue(cue,1).y,-1412.85f,"AI lower reflection");
                Near(g.ReflectCue(cue,2).x,2382.85f,"AI right reflection");Near(g.ReflectCue(cue,3).y,1012.85f,"AI upper reflection");
                float[] targets=g.BuildAiTargets();Check(targets.Length==36,"three AI targets per pocket");
                for(int i=0;i<6;i++) {
                    var p=g.Pocket(i);Near(targets[i*6],p.x,"AI pocket x "+i);Near(targets[i*6+1],p.y,"AI pocket y "+i);
                    Check(g.PocketRadius(i)>0,"positive ball-centre capture radius "+i);
                    Check(!g.CanPlace(p,28.575f),"cannot place ball in pocket "+i);
                }
                var oldRule=new EightBallVsRule();var newRule=new EightBallVsRule();newRule.ConfigureGeometry(g);
                Check(oldRule.getBlockerCount()==22,"legacy blocker count retained");Check(newRule.getBlockerCount()==g.BlockerCount,"rule consumes mesh blockers");
                var ruleCopy=newRule.Geometry;
                ((float[])typeof(TableGeometryConfig).GetField("blockers",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(g))[0]=123;
                Check(ruleCopy.CopyBlockers(true)[0]==-1500,"configuration copied per rule");
                var balls=new PoolBall[16];for(int i=0;i<16;i++)balls[i]=new PoolBall{scale=28.575f};
                Check(!oldRule.canPut(balls,0,1100,0)&&newRule.canPut(balls,0,1100,0),"new free-ball area extends beyond old table");
                Check(!newRule.canPut(balls,0,1260,0),"ball radius respected at end rail");
                var ball=new PoolBall{scale=28.575f,x=-900,y=0};var state=new VSRule.GameRoundState{campstate=0};
                Check(newRule.checkFreeballArea(state,ball),"opening cue behind head string");ball.x=-620;Check(!newRule.checkFreeballArea(state,ball),"opening cue cannot cross new head string");
                var blockers=new PoolBlocker[newRule.getBlockerCount()];for(int i=0;i<blockers.Length;i++)blockers[i]=new PoolBlocker();newRule.resetBlockers(blockers);
                var copiedBlockers=ruleCopy.CopyBlockers(true);
                for(int i=0;i<blockers.Length;i++)Near(blockers[i].getX0(),copiedBlockers[i*4],"bound blocker "+i);
                var holes=new PoolHole[6];for(int i=0;i<6;i++)holes[i]=new PoolHole();newRule.resetHoles(holes);
                for(int i=0;i<6;i++)Near(holes[i].width,ruleCopy.PocketRadius(i)*2,"bound capture diameter "+i);
                UnityEngine.Random.InitState(1026);newRule.InitPos();
                Near(newRule.ballsPos[0],-900,"new opening cue spot");
                for(int i=1;i<16;i++) {
                    Vector2 p=new Vector2(newRule.ballsPos[i*2],newRule.ballsPos[i*2+1]);Check(ruleCopy.CanPlace(p,28.575f),"rack inside table "+i);
                    for(int j=1;j<i;j++)Check(Vector2.Distance(p,new Vector2(newRule.ballsPos[j*2],newRule.ballsPos[j*2+1]))>=57.15f,"rack no overlap "+i+"/"+j);
                }
                var table=(PoolTable)FormatterServices.GetUninitializedObject(typeof(PoolTable));
                table.balls=balls;table.blockers=blockers;table.holes=holes;
                typeof(PoolTable).GetMethod("EnsureCollisionCapacity",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(table,null);
                var cache=PoolTable.nextHitTime;
                Check(cache.GetLength(1)>=g.BlockerCount&&cache.GetLength(2)>=g.BlockerCount,"collision cache expands for full mesh outline");
                Near(PoolRule.INNER_TABLE_WIDTH,1981.19995f,"legacy global width not mutated");Near(PoolRule.INNER_TABLE_HEIGHT,990.599976f,"legacy global height not mutated");
                SpinLabPhysicsChecks.Run();
                report.passed=true;
            }catch(Exception e){report.error=e.ToString();}
            File.WriteAllText(Path.Combine(folder,"geometry_checks.json"),JsonUtility.ToJson(report,true));
            if(!report.passed)throw new Exception(report.error);
            Debug.Log("[Integration] GEOMETRY_OK "+report.cases+"; no gameplay");
        }
    }
}
