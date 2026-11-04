// Adapted from pooltool 7632b1c66f19159741a4cfe48b23d18f079d9a87, Apache-2.0.
// See ThirdPartyNotices/pooltool. Changes: C#, xy-plane projection, legacy rv/w representation.
using System;
using UnityEngine;
namespace Pool3D.Offline.Physics {
 [Serializable]public struct SpinBody {
  public Vector2 velocity,rolling;
  public float side;
  public SpinBody(Vector2 velocity,Vector2 rolling,float side){this.velocity=velocity;this.rolling=rolling;this.side=side;}
 }
 public static class SpinPhysicsMath {
  public static Vector2 ClampContact(Vector2 point,float maxRadius) {
   if(!Finite(point.x)||!Finite(point.y))throw new ArgumentException("Non-finite cue contact");
   return Vector2.ClampMagnitude(point,maxRadius);
  }
  public static SpinBody Strike(float centerSpeed,float radius,Vector2 direction,Vector2 contact,SpinPhysicsConfig config) {
   Check(radius);if(!Finite(centerSpeed)||centerSpeed<0)throw new ArgumentException("Invalid cue speed");
   contact=ClampContact(contact,config.maxTipOffset);direction=Unit(direction);
   // theta=0 reduction of cue_strike; preserve the existing centre-ball speed scale.
   float ratio=1+config.ballMassKg/config.cueMassKg;
   float speed=centerSpeed*ratio/(ratio+2.5f*contact.sqrMagnitude);
   return new SpinBody(direction*speed,direction*(-2.5f*speed*contact.y*config.englishThrottle),-2.5f*speed*contact.x/radius*config.englishThrottle);
  }
  public static float BallFriction(float relativeContactSpeed,float metresPerUnit,SpinPhysicsConfig config) {
   return config.useAlciatoreBallFriction?(float)(.009951+.108*Math.Exp(-1.088*relativeContactSpeed*metresPerUnit)):config.ballFriction;
  }
  public static SpinBody StrikeFromUi(float centerSpeed,float radius,Vector2 direction,Vector2 contact,SpinPhysicsConfig config) {
   // UI +x is player-left. After the legacy xy -> Unity z/x swizzle,
   // it is opposite to pooltool's cue-frame horizontal contact axis.
   return Strike(centerSpeed,radius,direction,new Vector2(-contact.x,contact.y),config);
  }
  public static bool BallBall(ref SpinBody first,ref SpinBody second,Vector2 normal,float radius,float metresPerUnit,SpinPhysicsConfig config) {
   Check(radius);State(first);State(second);Vector2 n=Unit(normal),t=new Vector2(-n.y,n.x);
   float approach=Vector2.Dot(first.velocity-second.velocity,n);if(approach<=0)return false;
   float st=Vector2.Dot(first.velocity-second.velocity,t)+radius*(first.side+second.side);
   float sz=Vector2.Dot(first.rolling+second.rolling,n);
   float slip=Mathf.Sqrt(st*st+sz*sz),qt=0,qz=0;
   if(slip>1e-8f) {
    // Deliberately matches upstream FrictionalInelastic2D's mu*e*|gn| limit.
    float budget=BallFriction(slip,metresPerUnit,config)*config.ballRestitution*approach;
    float magnitude=Mathf.Min(slip/7f,budget);qt=-magnitude*st/slip;qz=-magnitude*sz/slip;
   }
   float jn=.5f*(1+config.ballRestitution)*approach;
   first.velocity+=-jn*n+qt*t;second.velocity+=jn*n-qt*t;
   Vector2 changeRolling=2.5f*qz*n;first.rolling+=changeRolling;second.rolling+=changeRolling;
   float changeSide=2.5f*qt/radius;first.side+=changeSide;second.side+=changeSide;
   return true;
  }
  public static bool Cushion(ref SpinBody ball,Vector2 inwardNormal,float radius,SpinPhysicsConfig config) {
   Check(radius);State(ball);Vector2 n=Unit(inwardNormal),t=new Vector2(-n.y,n.x);
   float vn=Vector2.Dot(ball.velocity,n);if(vn>=0)return false;
   float st=Vector2.Dot(ball.velocity,t)-radius*ball.side,sz=-Vector2.Dot(ball.rolling,n);
   float slip=Mathf.Sqrt(st*st+sz*sz),qt=0,qz=0;
   float jn=-(1+config.cushionRestitution)*vn;
   if(slip>1e-8f){float magnitude=Mathf.Min(2*slip/7f,config.cushionFriction*jn);qt=-magnitude*st/slip;qz=-magnitude*sz/slip;}
   ball.velocity+=jn*n+qt*t;ball.rolling-=2.5f*qz*n;ball.side-=2.5f*qt/radius;
   return true;
  }
  public static bool Cloth(ref SpinBody ball,float dt,float metresPerUnit,SpinPhysicsConfig config) {
   State(ball);if(!Finite(dt)||dt<0||!Finite(metresPerUnit)||metresPerUnit<=0)throw new ArgumentException("Invalid integration units/time");
   float slideAcceleration=config.slidingFriction*config.gravity/metresPerUnit;
   float rollAcceleration=config.rollingFriction*config.gravity/metresPerUnit;
   Vector2 slip=ball.velocity+ball.rolling;float length=slip.magnitude,remaining=dt;
   if(length>1e-7f) {
    float transition=length/(3.5f*slideAcceleration),used=Mathf.Min(dt,transition);
    Vector2 dv=slip*(slideAcceleration*used/length);ball.velocity-=dv;ball.rolling-=2.5f*dv;remaining-=used;
    if(dt>=transition)ball.rolling=-ball.velocity;
   }else ball.rolling=-ball.velocity;
   if(remaining>0) {
    float speed=ball.velocity.magnitude;
    ball.velocity*=speed>0?Mathf.Max(0,1-rollAcceleration*remaining/speed):0;ball.rolling=-ball.velocity;
   }
   float loss=2.5f*config.spinFrictionProportionality*config.gravity*dt;
   ball.side=Mathf.MoveTowards(ball.side,0,loss);
   float stop=config.linearStopMetresPerSecond/metresPerUnit;
   if(ball.velocity.sqrMagnitude<=stop*stop&&ball.rolling.sqrMagnitude<=stop*stop){ball.velocity=Vector2.zero;ball.rolling=Vector2.zero;}
   if(Mathf.Abs(ball.side)<=config.angularStopRadiansPerSecond)ball.side=0;
   return ball.velocity.sqrMagnitude>0||ball.rolling.sqrMagnitude>0||ball.side!=0;
  }
  static bool Finite(float v){return !float.IsNaN(v)&&!float.IsInfinity(v);}
  static void Check(float radius){if(!Finite(radius)||radius<=0)throw new ArgumentException("Invalid ball radius");}
  static Vector2 Unit(Vector2 v){if(!Finite(v.x)||!Finite(v.y)||v.sqrMagnitude<1e-12f)throw new ArgumentException("Invalid contact direction");return v.normalized;}
  static void State(SpinBody b){if(!Finite(b.velocity.x)||!Finite(b.velocity.y)||!Finite(b.rolling.x)||!Finite(b.rolling.y)||!Finite(b.side))throw new ArgumentException("Non-finite ball state");}
 }
}
