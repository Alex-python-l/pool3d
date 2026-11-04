from pathlib import Path

PROJECT = Path(__file__).resolve().parent.parent / 'UnityProject'


def main():
    path = PROJECT / 'Assets/OfflineRuntime/OfflineSceneView.cs'
    text = path.read_text(encoding='utf-8')
    text = text.replace('using Object = UnityEngine.Object;', 'using Object = UnityEngine.Object;\nusing Pool3D.Offline.Presentation;\nusing Pool3D.Offline.Geometry;')
    start=text.index('        private readonly List<Material>')
    end=text.index('        [Serializable] private sealed class CameraSettings',start)
    text=text[:start]+'''        private GameObject sceneRoot,cameraObject,lightObject,environment;
        private bool ownsEnvironment;
        private Transform[] balls;
        private Transform cue;
        private IntegratedArtAssets art;
        private IntegratedBallView ballView;
        private IntegratedCueView cueView;
        private TableGeometryConfig geometry;
        private readonly Func<bool> canCameraAim;

'''+text[end:]
    start=text.index('        public OfflineSceneView(')
    end=text.index('            cameraObject = new GameObject',start)
    text=text[:start]+'''        public OfflineSceneView(Transform parent,Func<bool> canCameraAim=null) { this.parent=parent;this.canCameraAim=canCameraAim??(()=>false); }
        public void PrepareResources()
        {
            art=Resources.Load<IntegratedArtAssets>("Integrated/PresentationAssets");
            if(art==null||art.environmentPrefab==null||art.cuePrefab==null||art.ballPrefabs.Length!=16)throw new InvalidOperationException("Integrated V3 assets missing");
            environment=GameObject.Find("Integrated Environment");
            ownsEnvironment=environment==null;
            if(ownsEnvironment) { environment=Object.Instantiate(art.environmentPrefab,parent);environment.name="Integrated Environment";environment.transform.localPosition=TableSpace.ArtOrigin;environment.transform.localScale=Vector3.one*100; }
'''+text[end:]
    text=text.replace('light.intensity = 1.25f;', 'light.intensity = 1.25f;\n            light.shadows=LightShadows.Soft;')
    text=text.replace('            RenderSettings.ambientLight = new Color(0.48f, 0.5f, 0.53f);','            RenderSettings.ambientLight = new Color(0.48f, 0.5f, 0.53f);\n            Camera.transform.SetPositionAndRotation(new Vector3(0,300,0),Quaternion.Euler(90,90,0));')
    start=text.index('        private GameObject Primitive(')
    end=text.index('        public void Orbit(',start)
    text=text[:start]+'''        public void Build(PoolTable table)
        {
            Clear();geometry=table.Geometry;
            sceneRoot=new GameObject("Integrated Dynamic Presentation");sceneRoot.transform.SetParent(parent,false);
            ballView=new IntegratedBallView(art,sceneRoot.transform,table.balls.Length);balls=ballView.Balls;
            cueView=new IntegratedCueView(art.cuePrefab,sceneRoot.transform);cue=cueView.Root;
            CameraRig.SetTargetCenter(balls[0]);
            UpdateBalls(table,0,true);ResetCamera(0);
        }
        public void UpdateBalls(PoolTable table,float angle,bool showCue,float pull=0,Vector2? contact=null)
        {
            if(ballView==null)return;
            ballView.Apply(table.balls);
            cueView.Apply(balls[0].position,angle,contact??Vector2.zero,pull,showCue);
            CameraRig.enabled=!IsOverview;
            CameraRig.dirTarget=canCameraAim()?cue:null;
            if(IsOverview)ApplyOverview();
        }

'''+text[end:]
    text=text.replace('OfflineMatchController.Instance.AimAngle','ReadCueAngle()')
    text=text.replace('Mathf.Max(65f, 120f / Mathf.Max(0.1f, Camera.aspect)) * overviewScale','Mathf.Max(geometry.ShortSide*.05f,geometry.LongSide*.05f/Mathf.Max(.1f,Camera.aspect))*1.12f*overviewScale')
    start=text.index('        public float ReadCueAngle()')
    end=text.index('        public void Clear()',start)
    text=text[:start]+'''        public float ReadCueAngle()
        {
            return cue==null?0:MathUtil.AngleAroundAxis(Vector3.forward,cue.forward,Vector3.up);
        }
        public bool HitCue(Vector2 screen,out Vector2 backward)
        {
            backward=Vector2.zero;return cueView!=null&&cueView.Hit(Camera,screen,out backward);
        }
'''+text[end:]
    start=text.index('            if (cueMesh != null)')
    text=text[:start]+'''            if(ballView!=null)ballView.Dispose();if(cueView!=null)cueView.Dispose();
            ballView=null;cueView=null;
            if(sceneRoot!=null){sceneRoot.SetActive(false);PresentationLifetime.Release(sceneRoot);}
            sceneRoot=null;balls=null;cue=null;
        }
        public void Dispose()
        {
            Clear();
            if(cameraObject!=null)PresentationLifetime.Release(cameraObject);
            if(lightObject!=null)PresentationLifetime.Release(lightObject);
            if(ownsEnvironment&&environment!=null)PresentationLifetime.Release(environment);
        }
    }
}
'''
    path.write_text(text,encoding='utf-8')
    host=PROJECT/'Assets/OfflineRuntime/OfflineMatchController.cs'
    host.write_text(host.read_text(encoding='utf-8').replace('new OfflineSceneView(transform);','new OfflineSceneView(transform,()=>CanCameraAim);'),encoding='utf-8')
    controls=PROJECT/'Assets/OfflineRuntime/OfflineControls.cs'
    data=controls.read_text(encoding='utf-8').replace('float x = point.z / PoolRule.REAL_SIMULATOR_SCALE;\n            float y = point.x / PoolRule.REAL_SIMULATOR_SCALE;', 'Vector3 simulation=Pool3D.Offline.Geometry.TableSpace.WorldToSim(point);\n            float x=simulation.x,y=simulation.y;')
    controls.write_text(data,encoding='utf-8')
    # The base felt is needed in M1; shell layers and dynamic ball exclusions stay deferred.
    shader=PROJECT/'Assets/IntegratedArt/Shaders/ShortFelt.shader'
    shader.write_text(shader.read_text(encoding='utf-8').replace('FeltSurface(IN.worldPos,','FeltSurface(IN.worldPos*.01,'),encoding='utf-8')
    print('Dynamic V3 bindings applied in isolated copy')


if __name__=='__main__':main()
