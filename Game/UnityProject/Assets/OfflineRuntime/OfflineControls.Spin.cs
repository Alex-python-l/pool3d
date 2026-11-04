using UnityEngine;
namespace Pool3D.Offline {public sealed partial class OfflineControls {
 private Texture2D spinDisc;
 private void DrawSpinInput() {
  if(!host.EnhancedSpinActive) {
   Label("左右旋转 "+host.SpinX.ToString("0.00"));host.SpinX=GUILayout.HorizontalSlider(host.SpinX,-1,1,GUILayout.Height(24));
   Label("旧纵轴：正值回拉 "+host.SpinY.ToString("0.00"));host.SpinY=GUILayout.HorizontalSlider(host.SpinY,-1,1,GUILayout.Height(24));return;
  }
  Label("拖动母球上的击球点");Label("上：高杆　下：低杆　左右：加塞");
  Rect box=GUILayoutUtility.GetRect(174,174,GUILayout.ExpandWidth(false));
  Vector2 center=box.center;float radius=box.width*.5f;
  if(spinDisc==null) {
   spinDisc=new Texture2D(128,128,TextureFormat.RGBA32,false){hideFlags=HideFlags.HideAndDontSave};
   var pixels=new Color[128*128];
   for(int y=0;y<128;y++)for(int x=0;x<128;x++) {
    float d=new Vector2((x-63.5f)/63.5f,(y-63.5f)/63.5f).magnitude;
    pixels[y*128+x]=new Color(.92f,.92f,.87f,Mathf.Clamp01((1-d)*64));
   }
   spinDisc.SetPixels(pixels);spinDisc.Apply(false,true);
  }
  Color colour=GUI.color;GUI.DrawTexture(box,spinDisc);
  GUI.color=new Color(.08f,.35f,.29f,.32f);
  float allowed=radius*host.MaximumTipOffset;
  GUI.DrawTexture(new Rect(center.x-allowed,center.y-allowed,2*allowed,2*allowed),spinDisc);
  GUI.color=new Color(.18f,.23f,.20f,.65f);
  GUI.DrawTexture(new Rect(center.x-1,center.y-radius*.85f,2,radius*1.7f),Texture2D.whiteTexture);
  GUI.DrawTexture(new Rect(center.x-radius*.85f,center.y-1,radius*1.7f,2),Texture2D.whiteTexture);
  GUI.color=new Color(.85f,.25f,.10f,1);
  Vector2 dot=center+new Vector2(-host.SpinX,-host.SpinY)*radius;
  GUI.DrawTexture(new Rect(dot.x-5,dot.y-5,10,10),spinDisc);GUI.color=colour;
  int id=GUIUtility.GetControlID("Pool3D.SpinDisc".GetHashCode(),FocusType.Passive,box);Event e=Event.current;
  if(!GUI.enabled&&GUIUtility.hotControl==id)GUIUtility.hotControl=0;
  if(GUI.enabled&&e.type==EventType.MouseDown&&e.button==0&&box.Contains(e.mousePosition)) {GUIUtility.hotControl=id;ApplySpinPointer(e.mousePosition,center,radius);e.Use();}
  else if(e.type==EventType.MouseDrag&&GUIUtility.hotControl==id) {ApplySpinPointer(e.mousePosition,center,radius);e.Use();}
  else if(e.type==EventType.MouseUp&&GUIUtility.hotControl==id) {GUIUtility.hotControl=0;e.Use();}
  GUILayout.BeginHorizontal();SpinPreset("中杆",0,0);SpinPreset("高杆",0,.5f);SpinPreset("低杆",0,-.5f);GUILayout.EndHorizontal();
  GUILayout.BeginHorizontal();SpinPreset("左塞",.5f,0);SpinPreset("右塞",-.5f,0);GUILayout.EndHorizontal();
 }
 private void ApplySpinPointer(Vector2 mouse,Vector2 center,float radius) {host.SetSpinContact((center.x-mouse.x)/radius,(center.y-mouse.y)/radius);}
 private void SpinPreset(string label,float left,float top) {if(GUILayout.Button(label,button))host.SetSpinContact(left,top);}
 private void ReleaseSpinDisc(){if(spinDisc!=null)Destroy(spinDisc);}
} }
