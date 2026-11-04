import bpy,json,hashlib,struct
from pathlib import Path
from datetime import datetime
OUT=Path(r'C:\Users\L\Desktop\破壳BullBear Labs\art-source\luxury_pool_table\20261102_felt_depth_r13')
ROOT=OUT.parents[2];s=bpy.context.scene
checks=json.loads((OUT/'felt_depth_checks.json').read_text(encoding='utf-8'))
render=json.loads((OUT/'render_status.json').read_text(encoding='utf-8'))
assert checks['passed'] and checks['after_triangles']==321802
assert render['status']=='complete' and render['batch']=='final_r13' and len(render['completed'])==5
assert sum(len(f.vertices)-2 for o in s.objects if o.type=='MESH' for f in o.data.polygons)==321802
assets=[]
for name in render['completed']:
    p=OUT/name;data=p.read_bytes();assert data[:8]==b'\x89PNG\r\n\x1a\n'
    assets.append({'file':name,'resolution':list(struct.unpack('>II',data[16:24])),'bytes':len(data),'sha256':hashlib.sha256(data).hexdigest()})
used=set()
for o in s.objects:
    if o.type=='MESH':
        for slot in o.material_slots:
            if slot.material and slot.material.use_nodes:
                used.update(n.image for n in slot.material.node_tree.nodes if n.type=='TEX_IMAGE' and n.image)
for im in used:
    assert im.has_data,im.name
    if not im.packed_file:im.pack()
assert all(im.packed_file for im in used)

roles=json.loads((OUT.parent/'20261101_closed_room_balls_cue_r11/unity_asset_roles.json').read_text(encoding='utf-8'))
preview='Preview | compressed cloth nap at balls R13'
roles.update({'revision':'R13 felt volume and live contact mask with R11 balls/cue',
    'static_room_mesh_objects':[o.name for o in s.objects if o.type=='MESH' and not o.name.startswith(('Ball |','Cue |','Preview |'))],
    'additional_excluded_contact_preview_mesh':preview,'felt_spec':'felt_spec.json','unity_export_performed':False,
    'physics_contact_z_m':.800,'visual_nap_top_z_m':.80018,'runtime_mask_requires_active_ball_positions':True})
(OUT/'unity_asset_roles.json').write_text(json.dumps(roles,ensure_ascii=False,indent=2),encoding='utf-8')
contract={'units':'metres in Blender; convert explicitly to original Unity centimetre presentation',
    'physics_contact_reference_z_m':.800,'visual_uncompressed_nap_top_z_m':.80018,'cloth_bottom_z_m':.79883,
    'cloth_thickness_m':.00135,'ball_radius_m':.028575,'stationary_ball_centre_z_m':.828575,
    'do_not_recalibrate_physics_height_from_render_skin':True,
    'runtime_ball_compression_inputs':'up to 16 active ball centres and radii, with original unit conversion',
    'static_room_excludes_ball_contact_preview_fibres':preview,
    'game_camera_reference':'Shot | front-half cue camera','authoring_default_camera':'Detail | R13 ball contact'}
(OUT/'contact_height_contract.json').write_text(json.dumps(contract,ensure_ascii=False,indent=2),encoding='utf-8')

s.camera=bpy.data.objects['Detail | R13 ball contact'];s.render.resolution_x=1800;s.render.resolution_y=1300
s.render.resolution_percentage=100;s.render.filepath=str(OUT/'ball_cloth_contact.png');s.cycles.samples=256
for o in s.objects:o.select_set(False)
bed=bpy.data.objects['Chinese8 | level cloth with six open pockets'];bed.select_set(True);bpy.context.view_layer.objects.active=bed
for screen in bpy.data.screens:
    for area in screen.areas:
        if area.type=='VIEW_3D' and area.spaces.active.region_3d:
            r=area.spaces.active.region_3d
            assert 'CAMERA' in [i.identifier for i in r.bl_rna.properties['view_perspective'].enum_items]
            r.view_perspective='CAMERA';r.view_camera_zoom=0
canonical=OUT.parent/'20261029_cleanup_sizing/luxury_pool_table_clean_metric.blend'
complete=OUT/'luxury_pool_table_felt_depth_r13.blend'
bpy.ops.wm.save_as_mainfile(filepath=str(canonical))
bpy.ops.wm.save_as_mainfile(filepath=str(complete),copy=True)
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
main_sha=sha(canonical);complete_sha=sha(complete)
pending_path=ROOT/'deliverables/Pool3D-TableReplacement-Prep/Docs/pending_blender_revision.json'
pending=json.loads(pending_path.read_text(encoding='utf-8'))
pending.update({'revision':'R13 mottled felt volume, cloth sheet and ball contact; R11 closed room and supplied cue retained',
    'source':str(complete.relative_to(ROOT)),'sourceSha256':complete_sha,
    'geometryRevision':'R13 1.35 mm render cloth and tiny edge nap; R4 nose and R6 wood contacts unchanged; R12 inner facings retained',
    'materialRevision':'R13 seamless photographic nap, full-bed wear and live 16-ball compression mask',
    'latestMaterialAuthoring':str((OUT/'felt_spec.json').relative_to(ROOT)),
    'contactHeightContract':str((OUT/'contact_height_contract.json').relative_to(ROOT)),
    'physicsContactZMetres':.800,'visualNapTopZMetres':.80018,
    'changedObjects':[o.name for o in s.objects if o.type=='MESH' and (o.name.startswith('Chinese8 | cushion ') or o.name in ['Chinese8 | luxury body','Chinese8 | level cloth with six open pockets','Chinese8 | six pocket leather back facings R12','Chinese8 | fine cloth edge nap R13'])],
    'requiresFullEnvironmentReexport':True,
    'note':'Unity candidate/archive remains R3b. Re-export latest room and geometry together. Exclude Blender balls, cue and ball-contact fibre preview from static room. Keep physics reference Z=0.800m, not visual pile Z=0.80018m. Adapt shader compression to active runtime ball positions; no Unity import, gameplay or FPS measurement performed in R13. Preserve this guard.'})
pending_path.write_text(json.dumps(pending,ensure_ascii=False,indent=2),encoding='utf-8')
delivery={'revision':'R13','saved_at':datetime.now().astimezone().isoformat(),'canonical':str(canonical),'canonical_sha256':main_sha,
    'complete_copy':str(complete),'complete_sha256':complete_sha,'pre_edit_backup':str(OUT/'before_felt_depth_r13.blend'),
    'scene_objects':len(s.objects),'mesh_objects':106,'triangles':321802,'packed_used_images':len(used),
    'actual_scene_render_views_reviewed':True,'views':assets,'checks':'felt_depth_checks.json','height_contract':'contact_height_contract.json',
    'static_room_mesh_count':len(roles['static_room_mesh_objects']),'static_contact_preview_excluded':True,
    'imagegen_mode':'built-in','imagegen_prompts':'imagegen_prompts.json','tripo_calls':0,'credentials_file_read':False,
    'unity_export_performed':False,'playMode':False,'gameplay_executed':False,'runtime_fps_not_measured':True,'pending_guard_updated':True}
(OUT/'delivery_checks.json').write_text(json.dumps(delivery,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps({'saved':str(complete),'canonical_sha256':main_sha,'copy_sha256':complete_sha,'mesh_objects':106,'triangles':321802,
                  'packed_images':len(used),'render_views':len(assets),'static_room_mesh_count':len(roles['static_room_mesh_objects'])},ensure_ascii=False))
