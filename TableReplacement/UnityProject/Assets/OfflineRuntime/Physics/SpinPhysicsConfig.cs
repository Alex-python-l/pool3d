// New optional profile; never changes the recovered global constants.
using System;
using System.IO;
using UnityEngine;
namespace Pool3D.Offline.Physics {
 [Serializable]public sealed class SpinPhysicsConfig {
  public int schemaVersion=1;
  public bool enabledByDefault=true,useAlciatoreBallFriction=true;
  public float ballRadiusMetres=.028575f,ballMassKg=.170097f,cueMassKg=.567f;
  public float maxTipOffset=.7f,englishThrottle=1;
  public float ballFriction=.05f,ballRestitution=.95f,cushionFriction=.2f,cushionRestitution=.85f;
  public float slidingFriction=.2f,rollingFriction=.01f,spinFrictionProportionality=4f/9f,gravity=9.81f;
  public float linearStopMetresPerSecond=.0005f,angularStopRadiansPerSecond=.05f;
  public void Validate() {
   if(schemaVersion!=1)throw new InvalidDataException("Unsupported SpinPhysics schemaVersion");
   Range("ballRadiusMetres",ballRadiusMetres,.01f,.05f);Range("ballMassKg",ballMassKg,.05f,.5f);Range("cueMassKg",cueMassKg,.1f,2);
   Range("maxTipOffset",maxTipOffset,.05f,.8f);Range("englishThrottle",englishThrottle,0,2);
   Range("ballFriction",ballFriction,0,.5f);Range("ballRestitution",ballRestitution,0,1);
   Range("cushionFriction",cushionFriction,0,1);Range("cushionRestitution",cushionRestitution,0,1);
   Range("slidingFriction",slidingFriction,.001f,1);Range("rollingFriction",rollingFriction,.0001f,.5f);
   Range("spinFrictionProportionality",spinFrictionProportionality,.001f,5);Range("gravity",gravity,1,20);
   Range("linearStopMetresPerSecond",linearStopMetresPerSecond,.00001f,.05f);Range("angularStopRadiansPerSecond",angularStopRadiansPerSecond,.00001f,1);
  }
  static void Range(string name,float value,float min,float max) {
   if(float.IsNaN(value)||float.IsInfinity(value)||value<min||value>max)throw new InvalidDataException("Invalid SpinPhysics "+name);
  }
  public SpinPhysicsConfig Copy(){return (SpinPhysicsConfig)MemberwiseClone();}
  public static SpinPhysicsConfig Load() {
   string path=Path.Combine(Application.streamingAssetsPath,"SpinPhysics.json");
   if(!File.Exists(path))throw new FileNotFoundException("Missing SpinPhysics.json",path);
   var config=JsonUtility.FromJson<SpinPhysicsConfig>(File.ReadAllText(path));
   if(config==null)throw new InvalidDataException("Empty SpinPhysics configuration");config.Validate();return config;
  }
 }
}
