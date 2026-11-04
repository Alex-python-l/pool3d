// Optional bridge authored for this experimental copy; recovered master sources remain intact.
using UnityEngine;
using Pool3D.Offline.Physics;
using SoundManager=Pool3D.Offline.OfflineAudio;
public partial class PoolTable {
 private SpinPhysicsConfig spinConfig;
 public bool EnhancedSpinEnabled {get{return spinConfig!=null;}}
 public void ConfigureSpin(SpinPhysicsConfig config) {if(config!=null)config.Validate();spinConfig=config==null?null:config.Copy();}
 static SpinBody ReadSpin(PoolBall b){return new SpinBody(new Vector2(b.vx,b.vy),new Vector2(b.rvx,b.rvy),b.w);}
 static void WriteSpin(PoolBall b,SpinBody s){b.vx=s.velocity.x;b.vy=s.velocity.y;b.rvx=s.rolling.x;b.rvy=s.rolling.y;b.w=s.side;}
 private bool TryEnhancedStrike(float strength,float px,float py,float angle) {
  if(spinConfig==null)return false;
  var result=SpinPhysicsMath.StrikeFromUi(strength,rule.getBallRadius(),new Vector2(MathUtil.Cos(angle),MathUtil.Sin(angle)),new Vector2(px,py),spinConfig);
  WriteSpin(balls[0],result);return true;
 }
 private bool TryEnhancedBallHit(PoolBall a,PoolBall b) {
  if(spinConfig==null)return false;
  Vector2 delta=new Vector2(b.x-a.x,b.y-a.y);float distance=delta.magnitude;
  Vector2 n=distance>1e-6f?delta/distance:Vector2.right;
  // Symmetric positional correction; never divide by a coincident centre distance.
  float overlap=a.scale+b.scale-distance;
  if(overlap>0){Vector2 correction=n*(overlap*.5f+.00001f);a.x-=correction.x;a.y-=correction.y;b.x+=correction.x;b.y+=correction.y;}
  var first=ReadSpin(a);var second=ReadSpin(b);
  bool hit=SpinPhysicsMath.BallBall(ref first,ref second,n,rule.getBallRadius(),spinConfig.ballRadiusMetres/rule.getBallRadius(),spinConfig);
  WriteSpin(a,first);WriteSpin(b,second);
  if(hit){float volume=Mathf.Clamp((first.velocity-second.velocity).magnitude*.0002f,.02f,1);SoundManager.Instance().PlaySound2D(0,volume);}
  return true;
 }
 private bool TryEnhancedCushionHit(PoolBall ball) {
  if(spinConfig==null)return false;
  Vector2 delta=new Vector2(ball.x-hitx,ball.y-hity);if(delta.sqrMagnitude<1e-12f)return true;
  var state=ReadSpin(ball);bool hit=SpinPhysicsMath.Cushion(ref state,delta.normalized,rule.getBallRadius(),spinConfig);WriteSpin(ball,state);
  if(hit)SoundManager.Instance().PlaySound2D(1,Mathf.Clamp(state.velocity.magnitude*.0005f,.2f,1));
  return true;
 }
 private bool TryEnhancedFriction(PoolBall ball,float dt) {
  if(spinConfig==null||ball.inHole!=null)return false;
  var state=ReadSpin(ball);bool moving=SpinPhysicsMath.Cloth(ref state,dt,spinConfig.ballRadiusMetres/rule.getBallRadius(),spinConfig);WriteSpin(ball,state);
  if(moving)stillRunning=true;return true;
 }
}
