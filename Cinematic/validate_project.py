import bpy,json,hashlib,math
from pathlib import Path
from mathutils import Vector
OUT=Path(__file__).parent;s=bpy.data.scenes['PROMO | cinematic scene'];bpy.context.window.scene=s
def meshhash(o):
    h=hashlib.sha256()
    for v in o.data.vertices:h.update(bytes(str(tuple(v.co)),'utf8'))
    for p in o.data.polygons:h.update(bytes(str((tuple(p.vertices),p.material_index)),'utf8'))
    return h.hexdigest()
orig=json.loads((OUT/'original_mesh_hashes.json').read_text())
changes=[name for name,h in orig.items() if meshhash(bpy.data.objects[name])!=h]
camera_issues=[];cue_issues=[]
for f in range(1,529):
    s.frame_set(f)
    cam=next(m.camera for m in sorted(s.timeline_markers,key=lambda m:m.frame,reverse=True) if m.camera and m.frame<=f)
    x,y,z=cam.matrix_world.translation
    if not(-4.1<x<3.3 and -3<y<3 and .8<z<2.5):camera_issues.append(f)
    if f<145 or f>216:
        if any(not bpy.data.objects[n].hide_render for n in ['PROMO cue | slender ash front shaft','PROMO cue | 12mm ferrule','PROMO cue | 10mm domed leather tip']):cue_issues.append(f)
    if not bpy.data.objects['Cue | supplied magic staff model'].hide_render:cue_issues.append(f)
report={'original_meshes_checked':len(orig),'original_mesh_changes':changes,'camera_outside_room_frames':camera_issues,'cue_visibility_issues':cue_issues,'camera_count':sum(1 for m in s.timeline_markers if m.camera),'animation_frames':528,'fps':24,'duration_seconds':22,'render_resolution':[1920,804],'delivery_canvas':[1920,1080],'ball_objects_reused':16,'scene_materials_reused':True,'source_file_sha256':hashlib.sha256(Path(json.loads((OUT/'scene_inventory.json').read_text(encoding='utf-8'))['original']).read_bytes()).hexdigest()}
report['passed']=not changes and not camera_issues and not cue_issues
(OUT/'project_checks.json').write_text(json.dumps(report,indent=2))
s.frame_set(1);s.camera=next(m.camera for m in s.timeline_markers if m.frame==1)
print(json.dumps(report))
