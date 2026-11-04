import bpy,json,math,hashlib
from pathlib import Path
from mathutils import Vector,Quaternion
OUT=Path(__file__).parent
s=bpy.data.scenes['PROMO | cinematic scene'];bpy.context.window.scene=s
s.frame_set(1)
for m in list(s.timeline_markers):
    if m.camera and m.camera.name.startswith('PROMO |'):s.timeline_markers.remove(m)
for o in list(bpy.data.objects):
    if o.type=='CAMERA' and o.name.startswith('PROMO |'):bpy.data.objects.remove(o,do_unlink=True)
motion=json.loads((OUT/'motion_240fps.json').read_text())
def physical_frame(f):
    if f<208:return 1.
    if f<=216:return 1+(f-208)*4
    if f<=288:return 33+(f-217)*.6
    if f<=384:return 76+(f-289)*10
    if f<=456:return 1026+(f-385)*8
    return 1594+(f-457)*8
def sample(f,i):
    a=max(0,min(len(motion)-1,physical_frame(f)-1));k=int(a);u=a-k
    v=motion[k][i];w=motion[min(k+1,len(motion)-1)][i]
    return Vector(v[:3]).lerp(Vector(w[:3]),u),Quaternion(v[3:]).slerp(Quaternion(w[3:]),u)
for i in range(16):
    o=bpy.data.objects[f'Ball | {i:02}'];original=o.rotation_euler.to_quaternion();o.rotation_mode='QUATERNION'
    for f in range(1,529):
        pos,rot=sample(f,i);o.location=pos;o.rotation_quaternion=rot@original
        o.keyframe_insert('location',frame=f);o.keyframe_insert('rotation_quaternion',frame=f)
# This authoring-only tuft patch was static at the original ball locations.
# Its visual counterpart is the existing live 16-ball compression material.
bpy.data.objects['Preview | compressed cloth nap at balls R13'].hide_render=True
cue=bpy.data.objects['Cue | logical tip root'];base=cue.location.copy()
for f,x in [(1,base.x),(164,base.x),(182,base.x-.15),(195,base.x-.15),(207,base.x-.15),(208,-.928575),(210,-.88),(214,-.90),(216,-1.00)]:
    cue.location=(x,base.y,base.z);cue.keyframe_insert('location',frame=f)
cuemesh=bpy.data.objects['Cue | supplied magic staff model']
for f,hidden in [(1,False),(216,False),(217,True)]:cuemesh.hide_render=hidden;cuemesh.keyframe_insert('hide_render',frame=f)
shots=[
 (1,72,'01 | A room for the break',(2.95,-2.5,1.65),(2.70,-2.20,1.58),(-.85,0,.95),(-.75,0,.93),22,24,False),
 (73,144,'02 | Cloth and craftsmanship',(1.60,-1.03,1.15),(1.35,-1.10,1.12),(1.14,-.60,.83),(1.00,-.62,.83),55,55,True),
 (145,216,'03 | Draw and strike',(-1.46,-.30,1.04),(-1.38,-.27,1.02),(-1.00,0,.835),(-.96,0,.835),50,53,True),
 (217,288,'04 | The collision',(.43,-.43,1.04),(.50,-.39,1.025),(.74,0,.828),(.76,.015,.828),48,48,True),
 (289,384,'05 | Overhead release',(-.03,-.015,1.72),(.045,.015,1.72),(0,0,.80),(.02,0,.80),8.4,8.4,False),
 (385,456,'06 | Across the cloth',(1.85,-1.55,1.50),(1.60,-1.65,1.48),(-.25,0,.825),(-.38,.03,.825),30,30,False),
 (457,528,'07 | The room remains',(2.65,-2.15,1.60),(2.95,-2.5,1.65),(-.75,0,.93),(-.85,0,.95),24,22,False)
]
def mix(a,b,u):return Vector(a).lerp(Vector(b),u)
report=[]
for start,end,name,eye0,eye1,at0,at1,l0,l1,dof in shots:
    cam=bpy.data.objects.new('PROMO | '+name,bpy.data.cameras.new('PROMO | '+name));s.collection.objects.link(cam)
    cam.data.clip_start=.003;cam.data.clip_end=30;cam.data.sensor_width=36
    cam.data.dof.use_dof=dof;cam.data.dof.aperture_fstop=6.3
    for f in range(start,end+1):
        u=(f-start)/(end-start);u=u*u*(3-2*u)
        cam.location=mix(eye0,eye1,u);at=mix(at0,at1,u)
        cam.rotation_mode='QUATERNION';cam.rotation_quaternion=(at-cam.location).to_track_quat('-Z','Y')
        if start==289:cam.rotation_quaternion=Quaternion((1,0,0,0))
        cam.data.lens=l0+(l1-l0)*u
        cam.data.dof.focus_distance=(at-cam.location).length
        cam.keyframe_insert('location',frame=f);cam.keyframe_insert('rotation_quaternion',frame=f)
        cam.data.keyframe_insert('lens',frame=f);cam.data.dof.keyframe_insert('focus_distance',frame=f)
    marker=s.timeline_markers.new(name,frame=start);marker.camera=cam
    report.append({'name':name,'start':start,'end':end,'duration_seconds':(end-start+1)/24,'camera':cam.name})
s.camera=bpy.data.objects['PROMO | '+shots[0][2]]
s.frame_start=1;s.frame_end=528;s.render.fps=24;s.render.fps_base=1
s.render.resolution_x=1920;s.render.resolution_y=804;s.render.resolution_percentage=100
s.render.engine='CYCLES';s.cycles.samples=32;s.cycles.use_denoising=True;s.cycles.adaptive_threshold=.04
s.cycles.max_bounces=6;s.cycles.diffuse_bounces=3;s.cycles.glossy_bounces=3;s.cycles.transmission_bounces=4
s.render.use_persistent_data=True;s.render.use_motion_blur=True;s.render.motion_blur_shutter=.25
prefs=bpy.context.preferences.addons['cycles'].preferences
prefs.compute_device_type='OPTIX';prefs.get_devices()
for d in prefs.devices:d.use=d.type=='OPTIX'
s.cycles.device='GPU'
s.render.image_settings.file_format='PNG';s.render.image_settings.color_mode='RGB'
s['promo_duration_seconds']=22;s['promo_physics']='Blender Bullet rigid bodies, 240 fps, 48 substeps, 100 solver iterations; retimed and baked transforms'
s['promo_original_geometry_unchanged']=True
exec(compile((OUT/'refine_cue.py').read_text(),str(OUT/'refine_cue.py'),'exec'),{'__file__':str(OUT/'refine_cue.py')})
s.frame_set(1)
(OUT/'shot_list.json').write_text(json.dumps(report,indent=2))
(OUT/'time_mapping.json').write_text(json.dumps({str(f):physical_frame(f) for f in range(1,529)}))
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'break_promo_working.blend'))
print(json.dumps({'animation_frames':528,'shots':7,'duration':22,'resolution':[1920,1080]}))
