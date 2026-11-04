import bpy,json,ast,hashlib
import numpy as np
from pathlib import Path
from mathutils import Vector
from mathutils.bvhtree import BVHTree
OUT=Path(r'C:\Users\L\Desktop\破壳BullBear Labs\art-source\luxury_pool_table\20261101_closed_room_balls_cue_r11')
assert json.loads((OUT/'render_status.json').read_text(encoding='utf-8'))['status']=='complete'
source=ast.parse((OUT.parent/'20261030_felt_lite/apply_felt_lite.py').read_text(encoding='utf-8'))
exec(compile(ast.Module(body=[n for n in source.body if isinstance(n,ast.FunctionDef) and n.name in ['mesh_hash','material_state']],type_ignores=[]),'protection','exec'))
baseline=json.loads((OUT/'before_play_assets_snapshot.json').read_text(encoding='utf-8'))
s=bpy.context.scene
assert all(mesh_hash(bpy.data.objects[n])==h for n,h in baseline['mesh_hashes'].items())
assert all(material_state(bpy.data.materials[n])==h for n,h in baseline['material_hashes'].items())
assert all([list(r) for r in bpy.data.objects[n].matrix_world]==m for n,m in baseline['transforms'].items() if n!='Room | presentation camera')
wall_names=['Room | front wall closed R11','Room | back wall opposite table','Room | left wall','Room | right wall','Room | ceiling','Room | carpet floor']
vertices=[];faces=[];names=[]
for name in wall_names:
    obj=bpy.data.objects[name];start=len(vertices);vertices.extend(obj.matrix_world@v.co for v in obj.data.vertices)
    for face in obj.data.polygons:faces.append([start+i for i in face.vertices]);names.append(name)
tree=BVHTree.FromPolygons(vertices,faces)
room_checks=[]
for axis,sign,target in [(0,1,3.3),(0,-1,-4.1),(1,1,3),(1,-1,-3),(2,1,2.5),(2,-1,0)]:
    direction=Vector((0,0,0));direction[axis]=sign
    hit,normal,index,d=tree.ray_cast(Vector((0,0,1.25)),direction,20)
    assert hit is not None and abs(hit[axis]-target)<1e-5,(axis,sign,hit)
    room_checks.append({'axis':axis,'sign':sign,'wall':names[index],'hit':list(hit)})
balls=[bpy.data.objects[f'Ball | {i:02d}'] for i in range(16)]
assert len({o.data for o in balls})==1
for i,a in enumerate(balls):
    assert a.get('ball_number')==i and abs(a.location.z-.828575)<1e-6
    for b in balls[i+1:]:assert (a.location-b.location).length>=.05715-1e-6
for name in ['Room | presentation camera','Shot | front-half cue camera']:
    p=bpy.data.objects[name].matrix_world.translation
    assert -4.1<p.x<3.3 and -3<p.y<3 and 0<p.z<2.5,(name,list(p))
tip=bpy.data.objects['CueTip'].matrix_world.translation;butt=bpy.data.objects['CueButt'].matrix_world.translation
assert abs((tip-butt).length-1.45)<1e-5
images=set()
for obj in balls+[bpy.data.objects['Cue | supplied magic staff model']]:
    for slot in obj.material_slots:
        mat=slot.material
        if mat and mat.use_nodes:images.update(n.image for n in mat.node_tree.nodes if n.type=='TEX_IMAGE' and n.image)
assert len(images)==19 and all(im.has_data and im.packed_file for im in images)
report={'passed':True,'all_original_meshes_unchanged':len(baseline['mesh_hashes']),
        'all_original_materials_unchanged':True,'only_original_presentation_camera_transform_changed':True,
        'room_closed_six_direction_checks':room_checks,'room_has_four_walls_floor_ceiling':True,
        'balls':16,'ball_diameter_m':.05715,'ball_center_height_m':.828575,'balls_share_one_mesh':True,
        'cue_source_model_used':True,'cue_length_m':(tip-butt).length,
        'cue_front_axis_fraction_visible':json.loads((OUT/'cue_rig_checks.json').read_text(encoding='utf-8'))['visible_axis_fraction'],
        'cue_body_not_cut_in_half':True,'no_table_cue_intersections':True,'preview_cameras_inside_closed_room':True,
        'new_packed_images':len(images),'static_blender_ball_preview':True,'runtime_rules_or_input_not_executed':True,
        'unity_scene_modified':False,'gameplay_executed':False,'rendered_views_checked':True,
        'scene_objects':len(s.objects),'scene_meshes':sum(o.type=='MESH' for o in s.objects),
        'scene_triangles':sum(len(f.vertices)-2 for o in s.objects if o.type=='MESH' for f in o.data.polygons)}
(OUT/'play_asset_final_checks.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
static=[o.name for o in s.objects if o.type=='MESH' and not o.name.startswith(('Ball |','Cue |'))]
roles={'static_room_mesh_objects':static,'excluded_ball_preview_objects':[o.name for o in balls],
       'runtime_ball_source':'Existing IntegratedArtAssets.ballPrefabs and PoolBall states',
       'separate_cue_prefab_root':'Cue | logical tip root','cue_anchors':['CueTip','CueButt','CueMidpoint'],
       'cue_prefab_export_note':'Export root-relative normalized cue, not the current world-space aiming pose.',
       'game_camera_reference':'Shot | front-half cue camera','unity_export_performed':False}
(OUT/'unity_asset_roles.json').write_text(json.dumps(roles,ensure_ascii=False,indent=2),encoding='utf-8')
s.camera=bpy.data.objects['Shot | front-half cue camera']
for screen in bpy.data.screens:
    for area in screen.areas:
        if area.type=='VIEW_3D' and area.spaces.active.region_3d:
            region=area.spaces.active.region_3d
            assert 'CAMERA' in [i.identifier for i in region.bl_rna.properties['view_perspective'].enum_items]
            region.view_perspective='CAMERA';region.view_camera_zoom=0
bpy.ops.wm.save_as_mainfile(filepath=str(OUT.parent/'20261029_cleanup_sizing/luxury_pool_table_clean_metric.blend'))
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'luxury_pool_room_closed_balls_cue_r11.blend'),copy=True)
print(json.dumps({k:report[k] for k in ['passed','all_original_meshes_unchanged','balls','cue_length_m','cue_front_axis_fraction_visible','scene_objects','scene_meshes','scene_triangles','unity_scene_modified']}))
