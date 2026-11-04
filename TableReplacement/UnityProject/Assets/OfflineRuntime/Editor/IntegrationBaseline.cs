using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Pool3D.Offline.Editor
{
    public static class IntegrationBaseline
    {
        [Serializable] private class Export { public MeshData[] meshes; public bool playMode = false, gameplay = false; }
        [Serializable] private class MeshData { public string name; public Vector3[] vertices; public int[] triangles; }
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorUtility.scriptCompilationFailed)
                throw new Exception("Baseline inspection requires compiled edit mode");
            const string path = "Assets/ThirdParty/PoolSet/PoolTable_Chinese.fbx";
            var importer = (ModelImporter)AssetImporter.GetAtPath(path);
            importer.isReadable = true;
            importer.SaveAndReimport();
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null) throw new Exception("Table asset missing");
            var meshes = asset.GetComponentsInChildren<MeshFilter>().Select(m => new MeshData {
                name = m.name,
                vertices = m.sharedMesh.vertices.Select(v => m.transform.TransformPoint(v)).ToArray(),
                triangles = m.sharedMesh.triangles
            }).ToArray();
            var folder = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Docs"));
            File.WriteAllText(Path.Combine(folder, "table_mesh_export.json"), JsonUtility.ToJson(new Export { meshes = meshes }));
            SpinLabPhysicsChecks.Run();
            PlayerSettings.colorSpace = ColorSpace.Linear;
            UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline = null;
            AssetDatabase.SaveAssets();
            File.WriteAllText(Path.Combine(folder, "baseline_checks.json"), "{\"passed\":true,\"meshCount\":" + meshes.Length + ",\"gameplay\":false,\"playMode\":false}");
            Debug.Log("[Integration] BASELINE_OK; exported mesh data; no gameplay");
        }
    }
}
