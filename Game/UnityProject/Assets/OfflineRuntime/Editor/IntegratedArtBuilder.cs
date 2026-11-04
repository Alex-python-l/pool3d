using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using Pool3D.Offline.Geometry;
using Pool3D.Offline.Presentation;

namespace Pool3D.Offline.Editor
{
    public static class IntegratedArtBuilder
    {
        private const string Art="Assets/IntegratedArt";
        public static IntegratedArtAssets Prepare()
        {
            const string resources="Assets/Resources/Integrated";
            Directory.CreateDirectory(resources);AssetDatabase.Refresh();
            string dataPath=resources+"/PresentationAssets.asset";
            var data=AssetDatabase.LoadAssetAtPath<IntegratedArtAssets>(dataPath);
            if(data==null){data=ScriptableObject.CreateInstance<IntegratedArtAssets>();AssetDatabase.CreateAsset(data,dataPath);}
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(Art+"/Prefabs/StaticSource.prefab");
            if(source!=null) {
                var root=(GameObject)PrefabUtility.InstantiatePrefab(source);
                try {
                    PrefabUtility.UnpackPrefabInstance(root,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
                    root.name="Integrated Environment";
                    foreach(var t in root.GetComponentsInChildren<Transform>(true)) {
                        if(t==null||t==root.transform)continue;
                        bool remove=t.GetComponent<Camera>()!=null||t.GetComponent<Light>()!=null||t.GetComponent<ReflectionProbe>()!=null||t.GetComponent<LightProbeGroup>()!=null||t.GetComponent<PostProcessVolume>()!=null||t.name.StartsWith("Ball ")||t.name.StartsWith("Felt fibers /")||t.name.StartsWith("Foreground reused Cue0");
                        if(remove)UnityEngine.Object.DestroyImmediate(t.gameObject);
                    }
                    foreach(var renderer in root.GetComponentsInChildren<Renderer>(true)) {
                        GameObjectUtility.SetStaticEditorFlags(renderer.gameObject,0);
                        renderer.lightmapIndex=-1;renderer.realtimeLightmapIndex=-1;
                        renderer.shadowCastingMode=ShadowCastingMode.On;renderer.receiveShadows=true;
                    }
                    data.environmentPrefab=PrefabUtility.SaveAsPrefabAsset(root,resources+"/Environment.prefab");
                }finally{UnityEngine.Object.DestroyImmediate(root);}
                AssetDatabase.DeleteAsset(Art+"/Prefabs/StaticSource.prefab");
            }
            if(data.environmentPrefab==null)throw new Exception("Stripped V3 environment missing");
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(Art+"/Meshes/Ball_Hero.asset");
            data.ballPrefabs=new GameObject[16];
            for(int i=0;i<16;i++) {
                string texturePath=Art+"/Textures/Ball_"+i+".png";
                var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                if(texture==null||texture.width!=2048||texture.height!=2048)throw new Exception("Missing full-resolution ball "+i);
                string materialPath=Art+"/Materials/Resin Ball "+i+".mat";
                var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if(material==null){material=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(material,materialPath);}
                material.color=Color.white;material.mainTexture=texture;material.SetFloat("_Metallic",0);material.SetFloat("_Glossiness",.9f);EditorUtility.SetDirty(material);
                var go=new GameObject("Ball "+i);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=material;
                try {data.ballPrefabs[i]=PrefabUtility.SaveAsPrefabAsset(go,Art+"/Prefabs/Ball_"+i+".prefab");}finally{UnityEngine.Object.DestroyImmediate(go);}
            }
            var cueImporter=(ModelImporter)AssetImporter.GetAtPath(Art+"/Models/Cue_Adapted.obj");
            if(!cueImporter.isReadable){cueImporter.isReadable=true;cueImporter.SaveAndReimport();}
            data.cuePrefab=AssetDatabase.LoadAssetAtPath<GameObject>(Art+"/Prefabs/Cue0_Hero.prefab");
            if(data.cuePrefab==null)throw new Exception("V3 cue missing");
            EditorUtility.SetDirty(data);AssetDatabase.SaveAssets();return data;
        }
        public static void SaveScene(IntegratedArtAssets assets)
        {
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var env=(GameObject)PrefabUtility.InstantiatePrefab(assets.environmentPrefab,scene);
            env.name="Integrated Environment";env.transform.position=TableSpace.ArtOrigin;env.transform.localScale=Vector3.one*100;
            var bootstrap=new GameObject("Integrated Offline Bootstrap");bootstrap.AddComponent<OfflineBootstrap>();
            const string scenePath="Assets/Scenes/IntegratedOffline.unity";
            if(!EditorSceneManager.SaveScene(scene,scenePath))throw new Exception("Integrated scene could not be saved");
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(scenePath,true)};
            AssetDatabase.SaveAssets();EditorSceneManager.OpenScene(scenePath,OpenSceneMode.Single);
            if(UnityEngine.Object.FindObjectsOfType<OfflineMatchController>().Length!=0)throw new Exception("A gameplay host was unexpectedly created in edit mode");
        }
    }
}
