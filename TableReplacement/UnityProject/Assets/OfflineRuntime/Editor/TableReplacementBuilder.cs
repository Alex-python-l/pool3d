using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Pool3D.Offline.Geometry;
using Pool3D.Offline.Presentation;

namespace Pool3D.Offline.Editor
{
    public static class TableReplacementBuilder
    {
        public const string Folder="Assets/TableReplacement";
        public const string Id="luxury-chinese8-20261030-r3";
        [Serializable] public class Payload {public int schema;public string sourceSha256;public MaterialData[] materials;public MeshData[] meshes;}
        [Serializable] public class MaterialData {public string name;public float[] color;public float metallic,smoothness;public bool atlas,cloth;}
        [Serializable] public class MeshData {public string name,role;public Vector3[] vertices,normals;public Vector2[] uv;public Group[] groups;}
        [Serializable] public class Group {public int material;public int[] indices;}
        public static Payload Read(){return JsonUtility.FromJson<Payload>(File.ReadAllText(Folder+"/Source/TableMesh.json"));}
        [MenuItem("Tools/Pool3D/Prepare replacement candidate")]
        public static void Prepare()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new Exception("Edit mode only");
            Directory.CreateDirectory(Folder+"/Meshes");Directory.CreateDirectory(Folder+"/Materials");Directory.CreateDirectory(Folder+"/Prefabs");
            AssetDatabase.Refresh();
            var data=Read();if(data.schema!=1||data.meshes.Length!=50)throw new Exception("Unexpected mesh source");
            var textureImporter=(TextureImporter)AssetImporter.GetAtPath(Folder+"/Textures/BaseColor.jpg");
            textureImporter.maxTextureSize=4096;textureImporter.mipmapEnabled=true;textureImporter.sRGBTexture=true;textureImporter.textureCompression=TextureImporterCompression.Uncompressed;textureImporter.SaveAndReimport();
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"/Textures/BaseColor.jpg");
            if(texture==null||texture.width!=4096)throw new Exception("Packed source atlas not imported");
            var template=AssetDatabase.FindAssets("t:Material",new[]{"Assets/IntegratedArt/Materials"}).Select(g=>AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g))).First(m=>m.shader.name=="MaterialStudy/Short Felt");
            var materials=new Material[data.materials.Length];
            for(int i=0;i<materials.Length;i++) {
                var source=data.materials[i];string path=Folder+"/Materials/Material_"+i+".mat";
                var material=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(material==null){material=source.cloth?new Material(template):new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(material,path);}
                if(source.cloth){material.SetFloat("_WorldUnitsPerMetre",100);material.SetFloat("_Metallic",0);}
                else {
                    material.color=source.atlas?Color.white:new Color(source.color[0],source.color[1],source.color[2],source.color[3]);
                    material.SetFloat("_Metallic",source.metallic);material.SetFloat("_Glossiness",source.smoothness);
                    material.mainTexture=source.atlas?texture:null;
                }
                material.name=source.name;EditorUtility.SetDirty(material);materials[i]=material;
            }
            var root=new GameObject("Luxury Chinese8 replacement");
            try {
                for(int i=0;i<data.meshes.Length;i++) {
                    var source=data.meshes[i];string path=Folder+"/Meshes/Mesh_"+i+".asset";
                    var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                    if(mesh==null){mesh=new Mesh();AssetDatabase.CreateAsset(mesh,path);}else mesh.Clear();
                    mesh.name=source.name;mesh.indexFormat=source.vertices.Length>65535?IndexFormat.UInt32:IndexFormat.UInt16;
                    mesh.vertices=source.vertices;mesh.normals=source.normals;mesh.uv=source.uv;mesh.subMeshCount=source.groups.Length;
                    for(int j=0;j<source.groups.Length;j++)mesh.SetTriangles(source.groups[j].indices,j);
                    mesh.RecalculateBounds();mesh.RecalculateTangents();EditorUtility.SetDirty(mesh);
                    var go=new GameObject(source.name);go.transform.SetParent(root.transform,false);
                    go.AddComponent<MeshFilter>().sharedMesh=mesh;
                    var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterials=source.groups.Select(g=>materials[g.material]).ToArray();
                    renderer.shadowCastingMode=ShadowCastingMode.On;renderer.receiveShadows=true;
                    GameObjectUtility.SetStaticEditorFlags(go,0);
                }
                PrefabUtility.SaveAsPrefabAsset(root,Folder+"/Prefabs/TableMetres.prefab");
                root.transform.position=TableSpace.ArtOrigin;root.transform.localScale=Vector3.one*100;
                root.name="Luxury Chinese8 - game centimetres";
                PrefabUtility.SaveAsPrefabAsset(root,Folder+"/Prefabs/TableGameSpace.prefab");
            }finally{UnityEngine.Object.DestroyImmediate(root);}
            var art=Resources.Load<IntegratedArtAssets>("Integrated/PresentationAssets");
            var environment=(GameObject)PrefabUtility.InstantiatePrefab(art.environmentPrefab);
            try {
                PrefabUtility.UnpackPrefabInstance(environment,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
                // Idempotent: the old source environment may already have been staged once.
                foreach(var t in environment.GetComponentsInChildren<Transform>(true)) {
                    if(t==null||t==environment.transform)continue;
                    if(t.name.StartsWith("PoolSet")||t.name.StartsWith("Table support ")||t.name=="Table apron"||t.name.StartsWith("Felt fibers /")||t.name=="Luxury Chinese8 replacement")UnityEngine.Object.DestroyImmediate(t.gameObject);
                }
                var table=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"/Prefabs/TableMetres.prefab"));
                table.transform.SetParent(environment.transform,false);table.name="Luxury Chinese8 replacement";
                var room=environment.GetComponentsInChildren<Transform>(true).First(t=>t.name=="Simplified room - reflection environment");
                var floor=room.GetComponentsInChildren<Renderer>(true).First(r=>r.name=="Room_Floor");
                room.position+=Vector3.up*(-.800f-floor.bounds.max.y);
                art.environmentPrefab=PrefabUtility.SaveAsPrefabAsset(environment,"Assets/Resources/Integrated/Environment.prefab");
                art.geometryId=Id;EditorUtility.SetDirty(art);
            }finally{UnityEngine.Object.DestroyImmediate(environment);}
            AssetDatabase.SaveAssets();
            // The candidate has its own copy of this scene. The original project is not opened.
            IntegratedArtBuilder.SaveScene(art);
            PlayerSettings.productName="Pool3D Table Replacement Preview";
            AssetDatabase.SaveAssets();
            Debug.Log("[TableReplacement] PREPARED; no Play Mode, screenshots or player");
        }
    }
}
