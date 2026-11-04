using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace Pool3D.Offline.Editor {
 public static class IntegratedFeltChecks {
  public static void Run() {
   if(EditorApplication.isPlayingOrWillChangePlaymode)throw new Exception("Edit mode only");
   bool implemented=typeof(PoolRule).Assembly.GetType("Pool3D.Offline.Presentation.DynamicFeltContacts")!=null;
   File.WriteAllText(Path.GetFullPath(Path.Combine(Application.dataPath,"../../Docs/felt_red.json")),"{\"passed\":"+implemented.ToString().ToLowerInvariant()+",\"dynamicFeltMissing\":"+(!implemented).ToString().ToLowerInvariant()+"}");
   if(!implemented)throw new Exception("Dynamic sixteen-ball felt contacts are missing");
  }
 }
}
