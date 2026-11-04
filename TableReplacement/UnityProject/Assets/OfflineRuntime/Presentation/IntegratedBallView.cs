using System;
using UnityEngine;
using Pool3D.Offline.Geometry;
namespace Pool3D.Offline.Presentation
{
    public sealed class IntegratedBallView : IDisposable
    {
        public Transform[] Balls {get;private set;}
        public IntegratedBallView(IntegratedArtAssets assets,Transform parent,int count)
        {
            if(count<1||count>16)throw new ArgumentOutOfRangeException(nameof(count));
            Balls=new Transform[count];
            for(int i=0;i<count;i++) {
                if(assets.ballPrefabs[i]==null)throw new InvalidOperationException("Missing V3 ball "+i);
                var go=UnityEngine.Object.Instantiate(assets.ballPrefabs[i],parent);go.name="Ball "+i;go.transform.localScale=Vector3.one*2.8575f;
                Balls[i]=go.transform;
            }
        }
        public void Apply(PoolBall[] states)
        {
            if(states.Length!=Balls.Length)throw new ArgumentException("Ball view/state count mismatch");
            for(int i=0;i<states.Length;i++) {
                PoolBall state=states[i];Transform view=Balls[i];
                view.gameObject.SetActive(state.visible&&state.isAvailable);
                view.position=TableSpace.SimToWorld(new Vector3(state.x,state.y,state.z));
                view.rotation=Util3D.XYZ_2_ZNXY(state.quat);
            }
        }
        public void Dispose(){if(Balls==null)return;foreach(var t in Balls)if(t!=null)PresentationLifetime.Release(t.gameObject);Balls=null;}
    }
}
