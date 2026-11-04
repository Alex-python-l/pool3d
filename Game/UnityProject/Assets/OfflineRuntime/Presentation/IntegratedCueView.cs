using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pool3D.Offline.Presentation
{
    public sealed class IntegratedCueView : IDisposable
    {
        public Transform Root {get;private set;}
        private Vector3 tipLocal,buttLocal;
        public Vector3 Tip {get{return Root.TransformPoint(tipLocal);}}
        public Vector3 Butt {get{return Root.TransformPoint(buttLocal);}}
        public IntegratedCueView(GameObject prefab,Transform parent)
        {
            Root=new GameObject("Integrated Cue").transform;Root.SetParent(parent,false);
            var model=UnityEngine.Object.Instantiate(prefab,Root).transform;
            model.localPosition=Vector3.zero;model.localRotation=Quaternion.identity;model.localScale*=100;
            Vector3 tip,butt;Measure(model,out tip,out butt);
            model.rotation=Quaternion.FromToRotation((tip-butt).normalized,Vector3.forward)*model.rotation;
            Measure(model,out tip,out butt);
            tipLocal=Root.InverseTransformPoint(tip);buttLocal=Root.InverseTransformPoint(butt);
            if(Mathf.Abs(Vector3.Distance(tip,butt)-145)>.05f)throw new InvalidOperationException("Cue length is not 1450 mm");
        }
        private static void Measure(Transform model,out Vector3 tip,out Vector3 butt)
        {
            var tipPoints=new List<Vector3>();var buttPoints=new List<Vector3>();
            foreach(var filter in model.GetComponentsInChildren<MeshFilter>()) {
                var mesh=filter.sharedMesh;var vertices=mesh.vertices;var materials=filter.GetComponent<Renderer>().sharedMaterials;
                for(int i=0;i<Math.Min(materials.Length,mesh.subMeshCount);i++) {
                    string name=materials[i]==null?"":materials[i].name.ToLowerInvariant();
                    List<Vector3> target=name.Contains("chalked")||name.Contains("leather_tip")?tipPoints:name.Contains("rubber")?buttPoints:null;
                    if(target!=null)target.AddRange(mesh.GetTriangles(i).Distinct().Select(v=>filter.transform.TransformPoint(vertices[v])));
                }
            }
            if(tipPoints.Count==0||buttPoints.Count==0)throw new InvalidOperationException("Cue tip/butt material islands missing");
            Vector3 tc=tipPoints.Aggregate(Vector3.zero,(a,b)=>a+b)/tipPoints.Count,bc=buttPoints.Aggregate(Vector3.zero,(a,b)=>a+b)/buttPoints.Count;
            Vector3 axis=(tc-bc).normalized;
            float far=tipPoints.Max(p=>Vector3.Dot(p,axis)),near=buttPoints.Min(p=>Vector3.Dot(p,axis));
            var front=tipPoints.Where(p=>Vector3.Dot(p,axis)>=far-.001f).ToArray();var back=buttPoints.Where(p=>Vector3.Dot(p,axis)<=near+.001f).ToArray();
            tip=front.Aggregate(Vector3.zero,(a,b)=>a+b)/front.Length;butt=back.Aggregate(Vector3.zero,(a,b)=>a+b)/back.Length;
        }
        public void Apply(Vector3 centre,float angle,Vector2 contact,float pull,bool visible)
        {
            if(contact.sqrMagnitude>=1||float.IsNaN(contact.x)||float.IsNaN(contact.y))throw new ArgumentException("Invalid cue contact");
            Root.gameObject.SetActive(visible);if(!visible)return;
            float a=angle*Mathf.Deg2Rad;Vector3 direction=new Vector3(Mathf.Sin(a),0,Mathf.Cos(a));
            Vector3 left=new Vector3(-Mathf.Cos(a),0,Mathf.Sin(a));
            float radius=2.8575f;
            Vector3 point=centre+radius*(left*contact.x+Vector3.up*contact.y)-direction*(radius*Mathf.Sqrt(1-contact.sqrMagnitude)+.3f+20*Mathf.Clamp01(pull));
            Root.rotation=Quaternion.Euler(0,angle,0);Root.position=point-Root.rotation*tipLocal;
        }
        public static bool Pick(Camera camera,Vector2 screen,Vector3 frontWorld,Vector3 backWorld,out Vector2 backward)
        {
            backward=Vector2.zero;float frontDepth=camera.transform.InverseTransformPoint(frontWorld).z,backDepth=camera.transform.InverseTransformPoint(backWorld).z;
            float near=camera.nearClipPlane+.01f;if(frontDepth<=near&&backDepth<=near)return false;
            if(backDepth<=near)backWorld=Vector3.Lerp(frontWorld,backWorld,(frontDepth-near)/(frontDepth-backDepth));
            else if(frontDepth<=near)frontWorld=Vector3.Lerp(backWorld,frontWorld,(backDepth-near)/(backDepth-frontDepth));
            Vector3 front=camera.WorldToScreenPoint(frontWorld),back=camera.WorldToScreenPoint(backWorld);Vector2 axis=(Vector2)(back-front);
            if(axis.sqrMagnitude<4)return false;float t=Mathf.Clamp01(Vector2.Dot(screen-(Vector2)front,axis)/axis.sqrMagnitude);backward=axis.normalized;
            return (screen-((Vector2)front+axis*t)).sqrMagnitude<=18*18;
        }
        public bool Hit(Camera camera,Vector2 screen,out Vector2 backward){backward=Vector2.zero;return Root.gameObject.activeInHierarchy&&Pick(camera,screen,Tip,Butt,out backward);}
        public void Dispose(){if(Root!=null)PresentationLifetime.Release(Root.gameObject);Root=null;}
    }
}
