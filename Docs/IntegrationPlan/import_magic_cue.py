import bpy,json,ast,hashlib
import numpy as np
from pathlib import Path
from mathutils import Vector,Matrix
OUT=Path(r'C:\Users\L\Desktop\破壳BullBear Labs\art-source\luxury_pool_table\20261101_closed_room_balls_cue_r11')
SOURCE=OUT/'Source/magic staff 3d model.glb'
assert not (OUT/'before_wall_balls_cue.blend').exists()
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'before_wall_balls_cue.blend'),copy=True)
source=ast.parse((OUT.parent/'20261030_felt_lite/apply_felt_lite.py').read_text(encoding='utf-8'))
exec(compile(ast.Module(body=[n for n in source.body if isinstance(n,ast.FunctionDef) and n.name in ['mesh_hash','material_state']],type_ignores=[]),'protection','exec'))
s=bpy.context.scene
baseline={'mesh_hashes':{o.name:mesh_hash(o) for o in s.objects if o.type=='MESH'},
          'material_hashes':{m.name:material_state(m) for m in bpy.data.materials if m.users},
          'transforms':{o.name:[list(r) for r in o.matrix_world] for o in s.objects},
          'scene_objects':len(s.objects),'source_sha256':hashlib.sha256(SOURCE.read_bytes()).hexdigest(),
          'old_camera':s.camera.name,'old_engine':s.render.engine}
(OUT/'before_play_assets_snapshot.json').write_text(json.dumps(baseline,ensure_ascii=False,indent=2),encoding='utf-8')
previous=set(bpy.data.objects)
options={'filepath':str(SOURCE)}
if 'import_pack_images' in bpy.ops.import_scene.gltf.get_rna_type().properties:options['import_pack_images']=True
bpy.ops.import_scene.gltf(**options)
objects=list(set(bpy.data.objects)-previous)
assert len(objects)==1 and objects[0].type=='MESH'
obj=objects[0];obj.name='Cue | supplied magic staff model'
collection=bpy.data.collections.new('Gameplay preview | balls and supplied cue');s.collection.children.link(collection)
for c in list(obj.users_collection):c.objects.unlink(obj)
collection.objects.link(obj)
obj.matrix_world=Matrix.Translation((0,8,0))@obj.matrix_world
bpy.context.view_layer.update()
points=np.array([obj.matrix_world@v.co for v in obj.data.vertices]);center=points.mean(axis=0)
values,axes=np.linalg.eigh((points-center).T@(points-center));axis=axes[:,-1];q=(points-center)@axis
radial=(points-center)-q[:,None]*axis
ends=[]
for sign in [-1,1]:
    mask=(q<q.min()+.04*(q.max()-q.min())) if sign==-1 else (q>q.max()-.04*(q.max()-q.min()))
    ends.append({'sign':sign,'vertices':int(mask.sum()),'radius_90_percentile':float(np.quantile(np.linalg.norm(radial[mask],axis=1),.9)),
                 'center':points[mask].mean(axis=0).tolist()})
images=[]
for mat in obj.data.materials:
    for n in mat.node_tree.nodes:
        if n.type=='TEX_IMAGE' and n.image:
            im=n.image;assert len(im.pixels)>0;im.pack()
            images.append({'name':im.name,'resolution':list(im.size),'packed':bool(im.packed_file)})
report={'object':obj.name,'vertices':len(obj.data.vertices),'triangles':sum(len(f.vertices)-2 for f in obj.data.polygons),
        'principal_axis':axis.tolist(),'principal_span':float(q.max()-q.min()),'ends':ends,'images':images,
        'bounds':[[float(points[:,i].min()),float(points[:,i].max())] for i in range(3)]}
(OUT/'cue_import_inspection.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(report,ensure_ascii=False))
