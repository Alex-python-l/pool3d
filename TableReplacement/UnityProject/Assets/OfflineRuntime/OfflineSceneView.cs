// New scene assembly/desktop camera adaptation. Never uses PhysX for ball motion.
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Pool3D.Offline.Presentation;
using Pool3D.Offline.Geometry;

namespace Pool3D.Offline
{
    public sealed class OfflineSceneView : IDisposable
    {
        public Camera Camera { get; private set; }
        public CameraSF CameraRig { get; private set; }
        public bool IsOverview { get; private set; }
        public float Pitch { get { return CameraRig == null ? 32.752644f : CameraRig.Pitch; } set { if (CameraRig != null) CameraRig.IdealPitch = value; } }
        private float overviewScale = 1f;
        private readonly Transform parent;
        private GameObject sceneRoot,cameraObject,lightObject,environment;
        private bool ownsEnvironment;
        private Transform[] balls;
        private Transform cue;
        private IntegratedArtAssets art;
        private IntegratedBallView ballView;
        private IntegratedCueView cueView;
        private DynamicFeltContacts felt;
        private TableGeometryConfig geometry;
        private readonly Func<bool> canCameraAim;

        [Serializable] private sealed class CameraSettings
        {
            public float initialDistance, minDistance, maxDistance, yawSensitivity, pitchSensitivity, minPitch, maxPitch;
            public float smoothZoomSpeed, smoothOrbitSpeed, smoothPanningSpeed;
            public CurveKey[] curveKeys;
        }
        [Serializable] private sealed class CurveKey { public float time, value, inSlope, outSlope, inWeight, outWeight; public int weightedMode; }

        public OfflineSceneView(Transform parent,Func<bool> canCameraAim=null) { this.parent=parent;this.canCameraAim=canCameraAim??(()=>false); }
        public void PrepareResources()
        {
            art=Resources.Load<IntegratedArtAssets>("Integrated/PresentationAssets");
            if(art==null||art.environmentPrefab==null||art.cuePrefab==null||art.ballPrefabs.Length!=16)throw new InvalidOperationException("Integrated V3 assets missing");
            environment=GameObject.Find("Integrated Environment");
            ownsEnvironment=environment==null;
            if(ownsEnvironment) { environment=Object.Instantiate(art.environmentPrefab,parent);environment.name="Integrated Environment";environment.transform.localPosition=TableSpace.ArtOrigin;environment.transform.localScale=Vector3.one*100; }
            var fiberSurfaces=Array.FindAll(environment.GetComponentsInChildren<Renderer>(true),r=>r.name.StartsWith("Felt fibers /"));
            felt=fiberSurfaces.Length==0?null:new DynamicFeltContacts(fiberSurfaces);
            cameraObject = new GameObject("OfflineCamera");
            cameraObject.transform.SetParent(parent, false);
            Camera = cameraObject.AddComponent<Camera>();
            Camera.clearFlags = CameraClearFlags.SolidColor;
            Camera.backgroundColor = new Color(0.018f, 0.026f, 0.033f);
            Camera.fieldOfView = 60f; // Original Camera3D #4287.
            Camera.nearClipPlane = 2f;
            Camera.farClipPlane = 1700f;
            CameraRig = cameraObject.AddComponent<CameraSF>();
            CameraRig.enabled = false;
            TextAsset cameraConfig = Resources.Load<TextAsset>("OfflineConfig/Camera");
            if (cameraConfig == null) throw new InvalidOperationException("Missing original CameraSF settings");
            var settings = JsonUtility.FromJson<CameraSettings>(cameraConfig.text);
            if (settings == null || settings.curveKeys == null || settings.curveKeys.Length != 2) throw new InvalidOperationException("Invalid original camera curve");
            CameraRig.initialDistance = settings.initialDistance;
            CameraRig.minDistance = settings.minDistance; CameraRig.maxDistance = settings.maxDistance;
            CameraRig.yawSensitivity = settings.yawSensitivity; CameraRig.pitchSensitivity = settings.pitchSensitivity;
            CameraRig.minPitch = settings.minPitch; CameraRig.maxPitch = settings.maxPitch;
            CameraRig.smoothZoomSpeed = settings.smoothZoomSpeed; CameraRig.smoothOrbitSpeed = settings.smoothOrbitSpeed;
            CameraRig.smoothPanningSpeed = settings.smoothPanningSpeed; CameraRig.smoothPanning = false;
            var keys = new Keyframe[settings.curveKeys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                var k = settings.curveKeys[i];
                keys[i] = new Keyframe(k.time, k.value, k.inSlope, k.outSlope, k.inWeight, k.outWeight) { weightedMode = (WeightedMode)k.weightedMode };
            }
            CameraRig.curve = new AnimationCurve(keys) { preWrapMode = WrapMode.ClampForever, postWrapMode = WrapMode.ClampForever };
            cameraObject.AddComponent<AudioListener>();
            lightObject = new GameObject("OfflineLight");
            lightObject.transform.SetParent(parent, false);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.25f;
            light.shadows=LightShadows.Soft;
            lightObject.transform.rotation = Quaternion.Euler(55f, -35f, 0f);
            RenderSettings.ambientLight = new Color(0.48f, 0.5f, 0.53f);
            Camera.transform.SetPositionAndRotation(new Vector3(0,300,0),Quaternion.Euler(90,90,0));
        }

        public void Build(PoolTable table)
        {
            Clear();geometry=table.Geometry;
            if(!string.IsNullOrEmpty(art.geometryId)&&art.geometryId!=geometry.GeometryId)throw new InvalidOperationException("Presentation and collision table IDs do not match");
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
            if(felt!=null)felt.Sync(table.balls,balls);
            cueView.Apply(balls[0].position,angle,contact??Vector2.zero,pull,showCue);
            CameraRig.enabled=!IsOverview;
            CameraRig.dirTarget=canCameraAim()?cue:null;
            if(IsOverview)ApplyOverview();
        }

        public void Orbit(Vector2 delta)
        {
            if (IsOverview) SetOverview(false, ReadCueAngle());
            CameraRig.EndFling();
            CameraRig.OnDrag(delta * Var.GetCueSensitive(1)); // Original normal sensitivity is0.5.
        }
        public void Zoom(float amount)
        {
            if (IsOverview) { overviewScale = Mathf.Clamp(overviewScale - amount * 0.08f, 0.4f, 2f); ApplyOverview(); }
            else CameraRig.IdealPitch += amount * 4f; // Desktop wheel; original curve couples pitch and follow distance.
        }
        public void FineAdjust(float angle) { CameraRig.RotateCamareY(angle / 0.04f); }
        public void EndDrag() { CameraRig.isOnDrageScreen = false; CameraRig.smoothPosition = true; }
        public void SetOverview(bool value, float angle)
        {
            IsOverview = value;
            CameraRig.enabled = !value && balls != null;
            Camera.orthographic = value;
            if (value) ApplyOverview(); else ResetCamera(angle);
        }
        public void ResetCamera(float angle)
        {
            if (IsOverview) { overviewScale = 1f; ApplyOverview(); return; }
            Camera.orthographic = false;
            CameraRig._Reset();
            CameraRig.InitCameraDir(angle);
            Camera.transform.rotation = Quaternion.Euler(32.752644f, angle, 0f);
            if (balls != null)
                Camera.transform.position = balls[0].position - Camera.transform.forward * CameraRig.initialDistance;
        }
        private void ApplyOverview()
        {
            Camera.orthographic = true;
            Camera.transform.SetPositionAndRotation(new Vector3(0f, 300f, 0f), Quaternion.Euler(90f, 90f, 0f));
            Camera.orthographicSize = Mathf.Max(geometry.ShortSide*.05f,geometry.LongSide*.05f/Mathf.Max(.1f,Camera.aspect))*1.12f*overviewScale;
        }
        public float ReadCueAngle()
        {
            return cue==null?0:MathUtil.AngleAroundAxis(Vector3.forward,cue.forward,Vector3.up);
        }
        public bool HitCue(Vector2 screen,out Vector2 backward)
        {
            backward=Vector2.zero;return cueView!=null&&cueView.Hit(Camera,screen,out backward);
        }
        public void Clear()
        {
            if(felt!=null)felt.Clear();
            if (CameraRig != null) { CameraRig.enabled = false; CameraRig.dirTarget = null; CameraRig.target = null; }
            if(ballView!=null)ballView.Dispose();if(cueView!=null)cueView.Dispose();
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
