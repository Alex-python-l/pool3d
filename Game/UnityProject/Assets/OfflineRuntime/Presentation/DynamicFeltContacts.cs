using System;
using UnityEngine;
namespace Pool3D.Offline.Presentation
{
    public sealed class DynamicFeltContacts
    {
        public const int Capacity=16;
        private readonly Renderer[] surfaces;
        private readonly Vector4[] contacts=new Vector4[Capacity];
        private readonly MaterialPropertyBlock block=new MaterialPropertyBlock();
        private static readonly int BallsId=Shader.PropertyToID("_Balls"),CountId=Shader.PropertyToID("_BallCount");
        public int Count {get;private set;}
        public DynamicFeltContacts(Renderer[] surfaces)
        {
            if(surfaces==null||surfaces.Length==0)throw new ArgumentException("Felt surfaces are missing");
            this.surfaces=(Renderer[])surfaces.Clone();Clear();
        }
        private static bool Finite(float v){return !float.IsNaN(v)&&!float.IsInfinity(v);}
        public void Apply(Vector4[] centresAndRadii,int count)
        {
            if(centresAndRadii==null||count<0||count>Capacity||count>centresAndRadii.Length)throw new ArgumentException("Invalid felt contact count");
            for(int i=0;i<count;i++){var p=centresAndRadii[i];if(!Finite(p.x)||!Finite(p.y)||!Finite(p.z)||!Finite(p.w)||p.w<=0)throw new ArgumentException("Invalid felt sphere");}
            if(!ReferenceEquals(centresAndRadii,contacts))Array.Copy(centresAndRadii,contacts,count);
            Array.Clear(contacts,count,Capacity-count);Count=count;
            block.SetVectorArray(BallsId,contacts);block.SetInt(CountId,count);
            foreach(var surface in surfaces)if(surface!=null)surface.SetPropertyBlock(block);
        }
        public void Sync(PoolBall[] states,Transform[] views)
        {
            if(states==null||views==null||states.Length!=views.Length||states.Length>Capacity)throw new ArgumentException("Invalid felt ball views");
            int count=0;
            for(int i=0;i<states.Length;i++) {
                if(!states[i].visible||!states[i].isAvailable||views[i]==null||!views[i].gameObject.activeInHierarchy)continue;
                Vector3 p=views[i].position;contacts[count++]=new Vector4(p.x,p.y,p.z,states[i].scale*.1f);
            }
            Apply(contacts,count);
        }
        public void Clear(){Apply(contacts,0);}
    }
}
