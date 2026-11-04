// Leaf numeric checks only. No scene, camera, host, AI, match or player is instantiated.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Pool3D.Offline.Physics;
namespace Pool3D.Offline.Editor {
 public static class SpinLabPhysicsChecks {
  [Serializable]class Fixtures {public string upstreamCommit;public Fixture[] fixtures;}
  [Serializable]class Fixture {
   public string name,kind;public SpinBody first,second,expectedFirst,expectedSecond;
   public Vector2 normal,contact;public float radius,metresPerUnit,speed,angle,friction,restitution,dt;public bool useAlciatore;
  }
  [Serializable]class Report {public bool passed,gameplayExecuted,playModeEntered,screenshots;public string unity,upstreamCommit;public int numericFixtures,invariantCases,configurationGuards;public List<string> checks=new List<string>(),errors=new List<string>();}
  static Report report;
  static void Check(bool value,string name){if(!value)throw new Exception(name);report.checks.Add(name);}
  static void Near(float a,float b,string name){if(float.IsNaN(a)||float.IsInfinity(a)||Mathf.Abs(a-b)>3e-5f*Mathf.Max(1,Mathf.Abs(b)))throw new Exception(name+": "+a+" != "+b);}
  static void Near(SpinBody a,SpinBody b,string name){Near(a.velocity.x,b.velocity.x,name+" vx");Near(a.velocity.y,b.velocity.y,name+" vy");Near(a.rolling.x,b.rolling.x,name+" rvx");Near(a.rolling.y,b.rolling.y,name+" rvy");Near(a.side,b.side,name+" w");}
  static void Throws(Action action,string name){bool threw=false;try{action();}catch(ArgumentException){threw=true;}catch(InvalidDataException){threw=true;}Check(threw,name);report.configurationGuards++;}
  public static void Run() {
   if(EditorApplication.isPlayingOrWillChangePlaymode)throw new Exception("Numeric checks must not enter Play Mode");
   string delivery=Path.GetFullPath(Path.Combine(Application.dataPath,"../.."));report=new Report{unity=Application.unityVersion};
   try {
    var fixtures=JsonUtility.FromJson<Fixtures>(File.ReadAllText("Assets/OfflineRuntime/Editor/SpinReferenceFixtures.json"));report.upstreamCommit=fixtures.upstreamCommit;
    Check(fixtures.fixtures[0].first.velocity.x==1f&&fixtures.fixtures[0].expectedSecond.velocity.x>.9f,"nonzero fixture data survives JsonUtility");
    foreach(var f in fixtures.fixtures) {
     var config=new SpinPhysicsConfig();var first=f.first;var second=f.second;
     if(f.kind=="ball") {config.useAlciatoreBallFriction=f.useAlciatore;config.ballFriction=f.friction;config.ballRestitution=f.restitution;SpinPhysicsMath.BallBall(ref first,ref second,f.normal,f.radius,f.metresPerUnit,config);Near(second,f.expectedSecond,f.name+" second");}
     else if(f.kind=="cushion"){config.cushionFriction=f.friction;config.cushionRestitution=f.restitution;SpinPhysicsMath.Cushion(ref first,f.normal,f.radius,config);}
     else if(f.kind=="strike"){float a=f.angle*Mathf.Deg2Rad;first=SpinPhysicsMath.Strike(f.speed,f.radius,new Vector2(Mathf.Cos(a),Mathf.Sin(a)),f.contact,config);}
     else if(f.kind=="cloth")SpinPhysicsMath.Cloth(ref first,f.dt,f.metresPerUnit,config);
     else throw new Exception("Unknown fixture kind "+f.kind);
     Near(first,f.expectedFirst,f.name);report.numericFixtures++;report.checks.Add("pooltool: "+f.name);
    }
    Check(report.numericFixtures==24,"24 upstream numerical comparisons");
    UiContactGeometry();
    var loaded=SpinPhysicsConfig.Load();loaded.Validate();
    foreach(var field in typeof(SpinPhysicsConfig).GetFields().Where(f=>f.FieldType==typeof(float))) {
     var invalid=loaded.Copy();field.SetValue(invalid,float.NaN);Throws(invalid.Validate,"reject NaN "+field.Name);
     invalid=loaded.Copy();field.SetValue(invalid,float.PositiveInfinity);Throws(invalid.Validate,"reject Inf "+field.Name);
    }
    var bad=loaded.Copy();bad.ballRestitution=1.01f;Throws(bad.Validate,"reject restitution >1");
    bad=loaded.Copy();bad.maxTipOffset=1;Throws(bad.Validate,"reject contact outside sphere");
    Throws(()=>SpinPhysicsMath.ClampContact(new Vector2(float.NaN,0),.7f),"reject nonfinite contact");
    Near(SpinPhysicsMath.ClampContact(Vector2.one,.7f).magnitude,.7f,"diagonal contact radial clamp");
    var follow=SpinPhysicsMath.Strike(1,1,Vector2.right,new Vector2(0,.4f),loaded);
    Near((follow.velocity+follow.rolling).magnitude,0,"positive top gives rolling/follow");
    var draw=SpinPhysicsMath.Strike(1,1,Vector2.right,new Vector2(0,-.4f),loaded);
    Check(Vector2.Dot(draw.velocity,draw.rolling)>0,"negative top gives backspin/draw");
    var spinOnly=new SpinBody(Vector2.zero,Vector2.zero,4);
    Check(SpinPhysicsMath.Cloth(ref spinOnly,.1f,1,loaded),"side-only spin remains active");
    Check(!SpinPhysicsMath.Cloth(ref spinOnly,10,1,loaded),"side-only spin eventually stops");
    var table=new PoolTable();Check(!table.EnhancedSpinEnabled,"new legacy table stays legacy");
    foreach(string method in new[]{"TryEnhancedStrike","TryEnhancedBallHit","TryEnhancedCushionHit","TryEnhancedFriction"}) {
     object[] args=method=="TryEnhancedStrike"?new object[]{1f,0f,0f,0f}:method=="TryEnhancedBallHit"?new object[]{new PoolBall(),new PoolBall()}:method=="TryEnhancedFriction"?new object[]{new PoolBall(),.01f}:new object[]{new PoolBall()};
     Check(!(bool)typeof(PoolTable).GetMethod(method,BindingFlags.NonPublic|BindingFlags.Instance).Invoke(table,args),method+" falls through when disabled");
    }
    float oldSliding=PoolRule.SLIDING_FORCE,oldRolling=PoolRule.ROLLING_FORCE,oldSpin=PoolRule.SPIN_DECE_RATE,oldCushion=PoolTable.BLOCK_HIT_LOST;
    table.ConfigureSpin(loaded);loaded.ballRestitution=.2f;
    var copied=(SpinPhysicsConfig)typeof(PoolTable).GetField("spinConfig",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(table);
    Check(copied.ballRestitution!=loaded.ballRestitution,"table owns a copied immutable-in-match profile");table.ConfigureSpin(null);
    Check(!table.EnhancedSpinEnabled&&oldSliding==PoolRule.SLIDING_FORCE&&oldRolling==PoolRule.ROLLING_FORCE&&oldSpin==PoolRule.SPIN_DECE_RATE&&oldCushion==PoolTable.BLOCK_HIT_LOST,"no recovered global coefficient changes");
    Invariants();report.passed=true;
   }catch(Exception e){report.errors.Add(e.ToString());}
   File.WriteAllText(Path.Combine(delivery,"numeric_checks.json"),JsonUtility.ToJson(report,true));
   if(!report.passed)throw new Exception("Spin numeric checks failed: "+string.Join(";",report.errors));
   Debug.Log("[SpinLab] "+report.numericFixtures+" upstream fixtures, "+report.invariantCases+" invariants, "+report.configurationGuards+" config guards passed; no gameplay");
  }
  static float Energy(SpinBody b,float radius){return .5f*b.velocity.sqrMagnitude+.2f*(b.rolling.sqrMagnitude+radius*radius*b.side*b.side);}
  static void UiContactGeometry() {
   var config=new SpinPhysicsConfig();
   foreach(float angle in new[]{0f,37f,90f,210f})foreach(float side in new[]{-.4f,.4f}) {
    float a=angle*Mathf.Deg2Rad;Vector2 direction=new Vector2(Mathf.Cos(a),Mathf.Sin(a));Vector2 point=new Vector2(side,.2f);
    var body=SpinPhysicsMath.StrikeFromUi(2,1,direction,point,config);
    Vector3 worldDirection=new Vector3(direction.y,0,direction.x),left=new Vector3(-direction.x,0,direction.y);
    Vector3 lever=left*point.x+Vector3.up*point.y-worldDirection*Mathf.Sqrt(1-point.sqrMagnitude);
    Vector3 expected=2.5f*Vector3.Cross(lever,worldDirection*body.velocity.magnitude);
    Vector3 actual=new Vector3(-body.rolling.x,body.side,body.rolling.y);
    Near((actual-expected).magnitude,0,"UI contact torque axis "+angle+" / "+side);
   }
   report.checks.Add("8 UI contact/Unity angular-vector cross-product contracts");
  }
  static void Invariants() {
   var random=new System.Random(261026);var config=new SpinPhysicsConfig{useAlciatoreBallFriction=false};
   Func<float> f=()=>(float)(random.NextDouble()*4-2);
   for(int i=0;i<120;i++) {
    var a=new SpinBody(new Vector2(f(),f()),new Vector2(f(),f()),f());var b=new SpinBody(new Vector2(f(),f()),new Vector2(f(),f()),f());
    Vector2 n=new Vector2(f(),f()).normalized;if(Vector2.Dot(a.velocity-b.velocity,n)<0)n=-n;
    var beforeA=a;var beforeB=b;float energy=Energy(a,1)+Energy(b,1);Vector2 momentum=a.velocity+b.velocity;
    SpinPhysicsMath.BallBall(ref a,ref b,n,1,.001f,config);
    Near((a.velocity+b.velocity-momentum).magnitude,0,"pair momentum");
    Check(Energy(a,1)+Energy(b,1)<=energy+1e-4f,"pair energy nonincrease "+i);
    Near(Vector2.Dot(a.velocity-b.velocity,n),-config.ballRestitution*Vector2.Dot(beforeA.velocity-beforeB.velocity,n),"relative restitution");
    var swappedA=beforeB;var swappedB=beforeA;SpinPhysicsMath.BallBall(ref swappedA,ref swappedB,-n,1,.001f,config);
    Near(a,swappedB,"swap symmetry A");Near(b,swappedA,"swap symmetry B");report.invariantCases++;
   }
  }
 }
}
