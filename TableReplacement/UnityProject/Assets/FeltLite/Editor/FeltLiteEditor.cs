using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Pool3D.Offline.Editor {
 public static class FeltLiteEditor {
  const string Folder="Assets/FeltLite";
  [Serializable] class Receipt {
   public bool passed,shaderSupported,forwardVariantsWarmed,playMode=false,scenesModified=false;
   public string unityVersion,graphicsDevice,packedFormat,normalFormat,package;
   public int shaderErrors,runtimeMaps=2,textureResolution=1024,addedVertices=0;
   public long textureRuntimeBytes;
   public string[] shaderMessages;
  }
  static string Output {get {return Path.GetFullPath(Path.Combine(Application.dataPath,"../../../../art-source/luxury_pool_table/20261030_felt_lite"));}}
  public static void PrepareAndCheck() {
   var r=new Receipt();
   try {
    if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit mode required");
    AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
    var packed=Import("FeltLite_Packed.png",false);
    var normal=Import("FeltLite_NormalGL.png",true);
    var shader=AssetDatabase.LoadAssetAtPath<Shader>(Folder+"/FeltLite.shader");
    if(!shader)throw new InvalidDataException("Missing shader");
    MakeMaterial(shader,packed,normal,"FeltLite_GameCentimetre.mat",100);
    MakeMaterial(shader,packed,normal,"FeltLite_Metre.mat",1);
    var variants=new ShaderVariantCollection();
    variants.Add(new ShaderVariantCollection.ShaderVariant(shader,PassType.ForwardBase,"DIRECTIONAL"));
    variants.Add(new ShaderVariantCollection.ShaderVariant(shader,PassType.ForwardAdd,"POINT"));
    variants.Add(new ShaderVariantCollection.ShaderVariant(shader,PassType.ShadowCaster));
    variants.WarmUp();
    var messages=ShaderUtil.GetShaderMessages(shader);
    r.shaderMessages=messages.Select(m=>m.severity+": "+m.message+" ("+m.platform+")").ToArray();
    r.shaderErrors=messages.Count(m=>m.severity.ToString()=="Error");
    r.shaderSupported=shader.isSupported;r.forwardVariantsWarmed=variants.isWarmedUp;
    r.unityVersion=Application.unityVersion;r.graphicsDevice=SystemInfo.graphicsDeviceType.ToString();
    r.packedFormat=packed.format.ToString();r.normalFormat=normal.format.ToString();
    r.textureRuntimeBytes=UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(packed)+UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(normal);
    if(r.shaderErrors!=0||!r.shaderSupported||!r.forwardVariantsWarmed)throw new InvalidDataException("Shader check failed");
    if(packed.width!=1024||normal.width!=1024||!packed.mipmapCount.Equals(normal.mipmapCount))throw new InvalidDataException("Texture import mismatch");
    AssetDatabase.SaveAssets();
    r.package=Path.Combine(Output,"Pool3D-FeltLite-R5.unitypackage");
    AssetDatabase.ExportPackage(new[]{Folder+"/FeltLite.shader",Folder+"/FeltLite_Packed.png",Folder+"/FeltLite_NormalGL.png",Folder+"/FeltLite_GameCentimetre.mat",Folder+"/FeltLite_Metre.mat"},r.package,ExportPackageOptions.IncludeDependencies);
    r.passed=true;
    File.WriteAllText(Path.Combine(Output,"unity_material_checks.json"),JsonUtility.ToJson(r,true));
    Debug.Log("FELT_LITE_OK: 2 maps; no scene or gameplay changes");
   }catch(Exception e) {
    File.WriteAllText(Path.Combine(Output,"unity_material_checks.json"),JsonUtility.ToJson(r,true));
    Debug.LogError("FELT_LITE_FAILED: "+e);EditorApplication.Exit(1);
   }
  }
  static Texture2D Import(string filename,bool normal) {
   string path=Folder+"/"+filename;
   var i=(TextureImporter)AssetImporter.GetAtPath(path);
   i.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;
   i.sRGBTexture=false;i.convertToNormalmap=false;i.isReadable=false;
   i.mipmapEnabled=true;i.streamingMipmaps=false;i.wrapMode=TextureWrapMode.Repeat;
   i.filterMode=FilterMode.Trilinear;i.anisoLevel=4;i.maxTextureSize=1024;
   var settings=i.GetPlatformTextureSettings("Standalone");settings.overridden=true;
   settings.maxTextureSize=1024;settings.format=normal?TextureImporterFormat.BC5:TextureImporterFormat.BC7;settings.compressionQuality=100;
   i.SetPlatformTextureSettings(settings);i.SaveAndReimport();
   return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
  }
  static void MakeMaterial(Shader shader,Texture2D packed,Texture2D normal,string filename,float units) {
   string path=Folder+"/"+filename;var m=AssetDatabase.LoadAssetAtPath<Material>(path);
   if(!m){m=new Material(shader);AssetDatabase.CreateAsset(m,path);}else m.shader=shader;
   // Color properties are stored in sRGB; the project's Linear renderer converts to the Blender dye values.
   m.SetColor("_Color",new Color(.025f,.245f,.06f,1).gamma);
   m.SetTexture("_Packed",packed);m.SetTexture("_BumpMap",normal);
   m.SetFloat("_FiberMeters",.018f);m.SetFloat("_WorldUnitsPerMetre",units);
   m.SetFloat("_BumpScale",.65f);m.SetFloat("_SpecularF0",.0224f);m.SetFloat("_SheenStrength",.12f);
   EditorUtility.SetDirty(m);
  }
 }
}
